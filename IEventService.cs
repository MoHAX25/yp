namespace yp
{
    using System;
    using System.Collections.Generic;
    using Models;

    public interface IEventService
    {
        IEnumerable<Event> GetAll();
        Event? Get(Guid id);
        void Create(Event ev);
        /// <summary>
        /// Обновляет событие по id. Возвращает true, если событие найдено и обновлено.
        /// </summary>
        bool Update(Guid id, Event ev);
        /// <summary>
        /// Удаляет событие по id. Возвращает true, если событие найдено и удалено.
        /// </summary>
        bool Delete(Guid id);
    }
}
