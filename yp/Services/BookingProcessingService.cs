using yp.Models;
using yp.Services;

namespace yp.BackgroundServices
{
    public class BookingProcessingService : BackgroundService
    {
        private readonly IBookingService _bookingService;
        private readonly InMemoryEventStore _eventStore;
        private readonly ILogger<BookingProcessingService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _simulatedExternalDelay = TimeSpan.FromSeconds(2);
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

        public BookingProcessingService(
            IBookingService bookingService,
            InMemoryEventStore eventStore,
            ILogger<BookingProcessingService> logger)
        {
            _bookingService = bookingService;
            _eventStore = eventStore;
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

                    var tasks = pendingBookings.Select(booking =>
                        ProcessBookingAsync(booking, stoppingToken));

                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException ex)
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
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Остановка сервиса BookingProcessingService.");
                }
            }

            _logger.LogInformation("BookingProcessingService остановлен.");
        }

        private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation(
                    "Обработка брони {BookingId} (событие {EventId})...",
                    booking.Id, booking.EventId);

                await Task.Delay(_simulatedExternalDelay, stoppingToken);

                await _processingSemaphore.WaitAsync(stoppingToken);

                try
                {
                    var @event = _eventStore.Get(booking.EventId);

                    if (@event == null)
                    {
                        _logger.LogWarning(
                            "Событие {EventId} для брони {BookingId} не найдено. Бронь отклонена.",
                            booking.EventId, booking.Id);

                        await _bookingService.RejectBookingAsync(booking.Id);
                        return;
                    }

                    booking.Confirm(DateTime.UtcNow);
                    await _bookingService.UpdateBookingAsync(booking);

                    _logger.LogInformation(
                        "Бронь {BookingId} переведена в статус {Status}.",
                        booking.Id, booking.Status);
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Обработка брони {BookingId} была отменена.", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке брони {BookingId}. Бронь отклонена.", booking.Id);

                try
                {
                    await _bookingService.RejectBookingAsync(booking.Id);
                }
                catch (Exception releaseEx)
                {
                    _logger.LogError(releaseEx,
                        "Ошибка при откате брони {BookingId} после исключения.", booking.Id);
                }
            }
        }
    }
}
