using yp.Models;

namespace yp.Services
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events = new List<Event>();

        public PaginatedResult<Event> GetAll(
            string? title = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _events.AsEnumerable();

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
            return _events.FirstOrDefault(e => e.Id == id);
        }

        public void Create(Event ev)
        {
            if (ev.Id == Guid.Empty) ev.Id = Guid.NewGuid();

            _events.Add(ev);
        }

        public bool Update(Guid id, Event ev)
        {
            var idx = _events.FindIndex(e => e.Id == id);

            if (idx == -1) return false;

            ev.Id = id;
            _events[idx] = ev;

            return true;
        }

        public bool Delete(Guid id)
        {
            var ev = Get(id);

            if (ev == null) return false;

            return _events.Remove(ev);
        }
    }
}