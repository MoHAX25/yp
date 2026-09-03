using yp;
using yp.BackgroundServices;
using yp.Middleware;
using yp.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddSingleton<InMemoryEventStore>();
builder.Services.AddSingleton<IEventService>(sp => new EventService(sp.GetRequiredService<InMemoryEventStore>()));
builder.Services.AddSingleton<IBookingService, BookingService>();
builder.Services.AddHostedService<BookingProcessingService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandling();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "YP API V1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
