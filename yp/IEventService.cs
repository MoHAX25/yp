namespace yp
{
    using System;
    using Models;

    public interface IEventService
    {
        /// <summary>
        /// Возвращает события с опциональной фильтрацией и пагинацией.
        /// </summary>
        PaginatedResult<Event> GetAll(
            string? title = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 10);

        /// <summary>
        /// Возвращает событие по id. Возвращает null, если событие не найдено.
        /// </summary>
        Event? Get(Guid id);

        /// <summary>
        /// Создает новое событие. Если Id события пусто, генерирует новый Guid.
        /// Возвращает сохранённый экземпляр (с проставленным Id).
        /// </summary>
        Event Create(Event ev);

        /// <summary>
        /// Создает новое событие с валидацией TotalSeats.
        /// </summary>
        /// <exception cref="Exceptions.ValidationAppException">Выбрасывается, если TotalSeats меньше или равно нулю.</exception>
        Event CreateEventAsync(string title, string description, DateTime startAt, DateTime endAt, int? totalSeats);

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
