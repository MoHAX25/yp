using yp.Models;
using yp.Services;

namespace yp.BackgroundServices
{
    public class BookingProcessingService : BackgroundService
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingProcessingService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _simulatedExternalDelay = TimeSpan.FromSeconds(2);

        public BookingProcessingService(
            IBookingService bookingService,
            ILogger<BookingProcessingService> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingProcessingService запущен.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var pendingBookings = await _bookingService.GetPendingBookingsAsync();

                    foreach (var booking in pendingBookings)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        _logger.LogInformation(
                            "Обработка брони {BookingId} (событие {EventId})...",
                            booking.Id, booking.EventId);

                        await Task.Delay(_simulatedExternalDelay, stoppingToken);

                        booking.Confirm(DateTime.UtcNow);

                        await _bookingService.UpdateBookingAsync(booking);

                        _logger.LogInformation(
                            "Бронь {BookingId} переведена в статус {Status}.",
                            booking.Id, booking.Status);
                    }
                }
                catch(OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, "Сервис BookingProcessingService был отменен.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке очереди бронирований.");
                }

                try
                {
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException )
                {
                    _logger.LogInformation("Остановка сервиса BookingProcessingService.");
                }
            }

            _logger.LogInformation("BookingProcessingService остановлен.");
        }
    }
}