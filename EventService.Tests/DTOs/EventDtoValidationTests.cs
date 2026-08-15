using System.ComponentModel.DataAnnotations;
using yp.Models;

namespace Tests.DTOs
{
    public class EventDtoValidationTests
    {
        private static List<ValidationResult> Validate(EventDTO dto)
        {
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Validate_ShouldFail_WhenEndAtIsBeforeStartAt()
        {
            var dto = new EventDTO
            {
                Id = Guid.NewGuid(),
                Title = "Некорректное событие",
                StartAt = new DateTime(2026, 1, 10),
                EndAt = new DateTime(2026, 1, 5),
                Description = "Описание события"
            };

            var results = Validate(dto);

            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(EventDTO.EndAt)));
        }

        [Fact]
        public void Validate_ShouldFail_WhenEndAtEqualsStartAt()
        {
            var sameTime = new DateTime(2026, 1, 10, 10, 0, 0);
            var dto = new EventDTO
            {
                Id = Guid.NewGuid(),
                Title = "Событие без длительности",
                StartAt = sameTime,
                EndAt = sameTime,
                Description = "Описание события"
            };

            var results = Validate(dto);

            Assert.NotEmpty(results);
        }

        [Fact]
        public void Validate_ShouldFail_WhenTitleIsMissing()
        {
            var dto = new EventDTO
            {
                Id = Guid.NewGuid(),
                Title = null!,
                StartAt = new DateTime(2026, 1, 1),
                EndAt = new DateTime(2026, 1, 2),
                Description = "Описание события"
            };

            var results = Validate(dto);

            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(EventDTO.Title)));
        }

        [Fact]
        public void Validate_ShouldPass_WhenDataIsCorrect()
        {
            var dto = new EventDTO
            {
                Id = Guid.NewGuid(),
                Title = "Корректное событие",
                StartAt = new DateTime(2026, 1, 1, 10, 0, 0),
                EndAt = new DateTime(2026, 1, 1, 11, 0, 0),
                Description = "Описание события"
            };

            var results = Validate(dto);

            Assert.Empty(results);
        }
    }
}