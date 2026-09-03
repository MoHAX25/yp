using yp.Models;

namespace yp.Services
{
    public class EventService : IEventService
    {
        private readonly InMemoryEventStore _store;

        public EventService(InMemoryEventStore store)
        {
            _store = store;
        }

        public PaginatedResult<Event> GetAll(
            string? title = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 10)
        {
            var allEvents = _store.GetAll();
            var query = allEvents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(e =>
                    e.Title != null &&
                    e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.StartAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.EndAt <= to.Value);
            }

            var totalCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResult<Event>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Event? Get(Guid id)
        {
            return _store.Get(id);
        }

        public Event Create(Event ev)
        {
            var newEvent = ev.Id == Guid.Empty
                ? ev with { Id = Guid.NewGuid() }
                : ev;

            _store.Add(newEvent);

            return newEvent;
        }

        public Event CreateEventAsync(string title, string description, DateTime startAt, DateTime endAt, int? totalSeats)
        {
            var ev = Event.Create(title, description, startAt, endAt, totalSeats);
            var newEvent = ev with { Id = Guid.NewGuid() };
            _store.Add(newEvent);
            return newEvent;
        }

        public bool Update(Guid id, Event ev)
        {
            return _store.Update(id, ev);
        }

        public bool Delete(Guid id)
        {
            return _store.Remove(id);
        }
    }
}