using Microsoft.Extensions.Logging;
using Server.Helpers;
using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();
//builder.Services.AddSignalR().AddJsonProtocol(o => { o.PayloadSerializerOptions.PropertyNamingPolicy = null; });

/*builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
        x =>
        {
            x.AllowAnyHeader()
                   .AllowAnyMethod()
                   .SetIsOriginAllowed((host) => true)
                   .AllowCredentials();
        }));*/

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsDevelopment())
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
else
    builder.Logging.SetMinimumLevel(LogLevel.Error);

builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<MatchmakingManager>();
var app = builder.Build();

Logger.LogFactory = app.Services.GetRequiredService<ILoggerFactory>();
Logger.Init();

//app.UseSwagger();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

/*app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();*/

app.MapControllers();

app.MapHub<GameHub>("/GameHub");

app.Run();
