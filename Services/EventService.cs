using yp.Models;

namespace yp.Services
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events = new List<Event>();

        public IEnumerable<Event> GetAll()
        {
            return _events;
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
