using BombermanServer.Hubs;
using BombermanServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
	options.EnableDetailedErrors = true;
	options.KeepAliveInterval = TimeSpan.FromSeconds(10);
	options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<IRoomService, RoomService>();
// Mevcut kodu bul ve þöyle deðiþtir:
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		// SetIsOriginAllowed(origin => true) diyerek her yerden gelen baðlantýyý kabul ediyoruz.
		policy.SetIsOriginAllowed(origin => true)
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors();

app.MapHub<GameHub>("/gameHub");
app.MapControllers();

var roomService = app.Services.GetRequiredService<IRoomService>();
var cleanupTimer = new System.Timers.Timer(300000);
cleanupTimer.Elapsed += (sender, e) => roomService.CleanupAbandonedRooms();
cleanupTimer.Start();

app.Run();