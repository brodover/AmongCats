using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Server.Helpers;
using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSignalR().AddJsonProtocol(o => { o.PayloadSerializerOptions.PropertyNamingPolicy = null; });

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


public class Vector3JsonConverter : System.Text.Json.Serialization.JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float x = 0, y = 0, z = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "X": x = reader.GetSingle(); break;
                    case "Y": y = reader.GetSingle(); break;
                    case "Z": z = reader.GetSingle(); break;
                }
            }
        }
        return new Vector3(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteEndObject();
    }
}