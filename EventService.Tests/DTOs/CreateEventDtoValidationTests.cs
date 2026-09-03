using System.ComponentModel.DataAnnotations;
using yp.Models;

namespace Tests.DTOs
{
    public class CreateEventDtoValidationTests
    {
        private static List<ValidationResult> Validate(CreateEventDTO dto)
        {
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Validate_ShouldFail_WhenTotalSeatsIsZero()
        {
            var dto = new CreateEventDTO
            {
                Title = "Событие",
                Description = "Описание события",
                StartAt = new DateTime(2026, 1, 1, 10, 0, 0),
                EndAt = new DateTime(2026, 1, 1, 11, 0, 0),
                TotalSeats = 0
            };

            var results = Validate(dto);

            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(CreateEventDTO.TotalSeats)));
        }

        [Fact]
        public void Validate_ShouldFail_WhenTotalSeatsIsNegative()
        {
            var dto = new CreateEventDTO
            {
                Title = "Событие",
                Description = "Описание события",
                StartAt = new DateTime(2026, 1, 1, 10, 0, 0),
                EndAt = new DateTime(2026, 1, 1, 11, 0, 0),
                TotalSeats = -5
            };

            var results = Validate(dto);

            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(CreateEventDTO.TotalSeats)));
        }

        [Fact]
        public void Validate_ShouldFail_WhenTotalSeatsIsNull()
        {
            var dto = new CreateEventDTO
            {
                Title = "Событие",
                Description = "Описание события",
                StartAt = new DateTime(2026, 1, 1, 10, 0, 0),
                EndAt = new DateTime(2026, 1, 1, 11, 0, 0),
                TotalSeats = null
            };

            var results = Validate(dto);

            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(CreateEventDTO.TotalSeats)));
        }

        [Fact]
        public void Validate_ShouldFail_WhenEndAtIsBeforeStartAt()
        {
            var dto = new CreateEventDTO
            {
                Title = "Некорректное событие",
                Description = "Описание события",
                StartAt = new DateTime(2026, 1, 10),
                EndAt = new DateTime(2026, 1, 5),
                TotalSeats = 100
            };

            var results = Validate(dto);

            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(CreateEventDTO.EndAt)));
        }

        [Fact]
        public void Validate_ShouldFail_WhenEndAtEqualsStartAt()
        {
            var sameTime = new DateTime(2026, 1, 10, 10, 0, 0);
            var dto = new CreateEventDTO
            {
                Title = "Событие без длительности",
                Description = "Описание события",
                StartAt = sameTime,
                EndAt = sameTime,
                TotalSeats = 100
            };

            var results = Validate(dto);

            Assert.NotEmpty(results);
        }

        [Fact]
        public void Validate_ShouldFail_WhenTitleIsMissing()
        {
            var dto = new CreateEventDTO
            {
                Title = null!,
                Description = "Описание события",
                StartAt = new DateTime(2026, 1, 1),
                EndAt = new DateTime(2026, 1, 2),
                TotalSeats = 100
            };

            var results = Validate(dto);

            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(CreateEventDTO.Title)));
        }

        [Fact]
        public void Validate_ShouldPass_WhenDataIsCorrect()
        {
            var dto = new CreateEventDTO
            {
                Title = "Корректное событие",
                Description = "Описание события",
                StartAt = new DateTime(2026, 1, 1, 10, 0, 0),
                EndAt = new DateTime(2026, 1, 1, 11, 0, 0),
                TotalSeats = 100
            };

            var results = Validate(dto);

            Assert.Empty(results);
        }
    }
}
