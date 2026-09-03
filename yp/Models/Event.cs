namespace yp.Models
{
    public record Event
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; private set; }

        public Event(int totalSeats)
        {
            if (totalSeats <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(totalSeats), "Количество мест должно быть положительным.");

            TotalSeats = totalSeats;
            AvailableSeats = totalSeats;
        }

        /// <summary>
        /// Фабричный метод для создания события с валидацией TotalSeats.
        /// </summary>
        /// <exception cref="yp.Exceptions.ValidationAppException">Выбрасывается, если TotalSeats меньше или равно нулю.</exception>
        public static Event Create(string title, string description, DateTime startAt, DateTime endAt, int? totalSeats)
        {
            if (totalSeats == null || totalSeats <= 0)
            {
                throw new yp.Exceptions.ValidationAppException("TotalSeats должно быть больше нуля.");
            }

            return new Event(totalSeats.Value)
            {
                Id = Guid.Empty,
                Title = title,
                Description = description,
                StartAt = startAt,
                EndAt = endAt
            };
        }

        public bool TryReserveSeats(int count = 1)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(count), "Количество мест для резервирования должно быть положительным.");

            if (count > AvailableSeats)
                return false;

            AvailableSeats -= count;
            return true;
        }

        public void ReleaseSeats(int count = 1)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(count), "Количество мест для освобождения должно быть положительным.");

            if (AvailableSeats + count > TotalSeats)
                throw new InvalidOperationException(
                    $"Нельзя освободить {count} мест(а): доступно станет больше, чем всего мест ({TotalSeats}).");

            AvailableSeats += count;
        }
    }
}
