using yp.Models;

namespace yp.Services
{
    /// <summary>
    /// In-memory хранилище событий. Используется как для IEventService, так и для фонового сервиса обработки броней.
    /// </summary>
    public class InMemoryEventStore
    {
        private readonly List<Event> _events = new();

        public Event? Get(Guid id)
        {
            lock (_lock)
            {
                return _events.FirstOrDefault(e => e.Id == id);
            }
        }

        public bool Update(Guid id, Event ev)
        {
            lock (_lock)
            {
                var idx = _events.FindIndex(e => e.Id == id);
                if (idx == -1) return false;
                _events[idx] = ev with { Id = id };
                return true;
            }
        }

        public void Add(Event ev)
        {
            lock (_lock)
            {
                _events.Add(ev);
            }
        }

        public bool Remove(Guid id)
        {
            lock (_lock)
            {
                var ev = _events.FirstOrDefault(e => e.Id == id);
                if (ev == null) return false;
                return _events.Remove(ev);
            }
        }

        public List<Event> GetAll()
        {
            lock (_lock)
            {
                return new List<Event>(_events);
            }
        }

        private readonly object _lock = new();
    }
}
