using yp.Models;
using yp.Services;

namespace Tests.Services
{
    public class EventServiceTests
    {
        private static Event MakeEvent(
            string title = "Событие",
            string? description = "Описание",
            DateTime? startAt = null,
            DateTime? endAt = null)
        {
            var start = startAt ?? new DateTime(2026, 1, 1, 10, 0, 0);
            var end = endAt ?? start.AddHours(1);

            return new Event
            {
                Title = title,
                Description = description,
                StartAt = start,
                EndAt = end
            };
        }

        // Успешные сценарии

        [Fact]
        public void Create_ShouldAssignNewId_WhenIdIsEmpty()
        {
            var service = new EventService();
            var ev = MakeEvent();
            ev.Id = Guid.Empty;

            var created = service.Create(ev);

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Single(service.GetAll().Items);
        }

        [Fact]
        public void Create_ShouldKeepProvidedId_WhenIdIsNotEmpty()
        {
            var service = new EventService();
            var id = Guid.NewGuid();
            var ev = MakeEvent();
            ev.Id = id;

            service.Create(ev);

            var stored = service.Get(id);
            Assert.NotNull(stored);
            Assert.Equal(id, stored!.Id);
        }

        [Fact]
        public void GetAll_ShouldReturnAllEvents_WhenNoFiltersApplied()
        {
            var service = new EventService();
            service.Create(MakeEvent(title: "Первое"));
            service.Create(MakeEvent(title: "Второе"));
            service.Create(MakeEvent(title: "Третье"));

            var result = service.GetAll();

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count());
        }

        [Fact]
        public void Get_ShouldReturnEvent_WhenIdExists()
        {
            var service = new EventService();
            var ev = MakeEvent(title: "Найти меня");
            var created = service.Create(ev);

            var found = service.Get(created.Id);

            Assert.NotNull(found);
            Assert.Equal("Найти меня", found!.Title);
        }

        [Fact]
        public void Update_ShouldModifyEvent_WhenIdExists()
        {
            var service = new EventService();
            var ev = MakeEvent(title: "Старое название");
            var created = service.Create(ev);

            var updatedIdBefore = created.Id;

            var updated = MakeEvent(title: "Новое название");
            var result = service.Update(created.Id, updated);

            Assert.True(result);

            var fromService = service.Get(created.Id);

            Assert.Equal("Новое название", fromService!.Title);
            Assert.Equal(updatedIdBefore, fromService.Id);
        }

        [Fact]
        public void Delete_ShouldRemoveEvent_WhenIdExists()
        {
            var service = new EventService();
            var ev = MakeEvent();
            var created = service.Create(ev);

            var result = service.Delete(created.Id);

            Assert.True(result);
            Assert.Null(service.Get(created.Id));
        }

        [Fact]
        public void GetAll_ShouldFilterByTitle_CaseInsensitivePartialMatch()
        {
            var service = new EventService();
            service.Create(MakeEvent(title: "Годовая Конференция"));
            service.Create(MakeEvent(title: "Планёрка"));
            service.Create(MakeEvent(title: "конференция по продажам"));

            var result = service.GetAll(title: "конфер");

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, e =>
                Assert.Contains("конфер", e.Title, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetAll_ShouldFilterByDateRange()
        {
            var service = new EventService();
            service.Create(MakeEvent(
                title: "До диапазона",
                startAt: new DateTime(2026, 1, 1),
                endAt: new DateTime(2026, 1, 2)));
            service.Create(MakeEvent(
                title: "Внутри диапазона",
                startAt: new DateTime(2026, 2, 1),
                endAt: new DateTime(2026, 2, 2)));
            service.Create(MakeEvent(
                title: "После диапазона",
                startAt: new DateTime(2026, 3, 5),
                endAt: new DateTime(2026, 3, 6)));

            var result = service.GetAll(
                from: new DateTime(2026, 1, 15),
                to: new DateTime(2026, 2, 15));

            Assert.Single(result.Items);
            Assert.Equal("Внутри диапазона", result.Items.Single().Title);
        }

        [Fact]
        public void GetAll_ShouldPaginateResults()
        {
            var service = new EventService();
            for (var i = 1; i <= 25; i++)
            {
                service.Create(MakeEvent(title: $"Событие {i}"));
            }

            var page2 = service.GetAll(page: 2, pageSize: 10);

            Assert.Equal(25, page2.TotalCount);
            Assert.Equal(10, page2.Items.Count());
            Assert.Equal(2, page2.Page);
            Assert.Equal(10, page2.PageSize);
        }

        [Fact]
        public void GetAll_ShouldReturnEmptyItems_WhenPageIsBeyondTotalCount()
        {
            var service = new EventService();
            service.Create(MakeEvent());

            var result = service.GetAll(page: 5, pageSize: 10);

            Assert.Equal(1, result.TotalCount);
            Assert.Empty(result.Items);
        }

        [Fact]
        public void GetAll_ShouldCombineTitleDateAndPaginationFilters()
        {
            var service = new EventService();
            service.Create(MakeEvent(
                title: "Митап по C#",
                startAt: new DateTime(2026, 5, 1),
                endAt: new DateTime(2026, 5, 1, 12, 0, 0)));
            service.Create(MakeEvent(
                title: "Митап по Go",
                startAt: new DateTime(2026, 5, 3),
                endAt: new DateTime(2026, 5, 3, 12, 0, 0)));
            service.Create(MakeEvent(
                title: "Планёрка",
                startAt: new DateTime(2026, 5, 2),
                endAt: new DateTime(2026, 5, 2, 12, 0, 0)));
            service.Create(MakeEvent(
                title: "Митап по Rust",
                startAt: new DateTime(2026, 6, 1),
                endAt: new DateTime(2026, 6, 1, 12, 0, 0)));

            var result = service.GetAll(
                title: "митап",
                from: new DateTime(2026, 5, 1),
                to: new DateTime(2026, 5, 31),
                page: 1,
                pageSize: 1);

            Assert.Equal(2, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal(1, result.Page);
            Assert.Equal(1, result.PageSize);
        }

        // Неуспешные сценарии

        [Fact]
        public void Get_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var service = new EventService();

            var result = service.Get(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public void Update_ShouldReturnFalse_WhenIdDoesNotExist()
        {
            var service = new EventService();
            var ev = MakeEvent();

            var result = service.Update(Guid.NewGuid(), ev);

            Assert.False(result);
        }

        [Fact]
        public void Delete_ShouldReturnFalse_WhenIdDoesNotExist()
        {
            var service = new EventService();

            var result = service.Delete(Guid.NewGuid());

            Assert.False(result);
        }
    }
}