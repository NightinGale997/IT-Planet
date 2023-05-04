using Microsoft.EntityFrameworkCore;
using PlanetIT;
using PlanetIT.DAL;
using PlanetIT.Middleware;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    // Конвертер enum
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

// Добавление DB контекста для DAL

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

//Development values
//var dbHost = "localhost";
//var dbName = "animal-chipization";
//var dbUser = "postgres";
//var dbPassword = "root";


builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
    $"Host={dbHost}; Database={dbName}; Username={dbUser}; Password={dbPassword}; Include Error Detail=True;"
));

//Инициализация репозиториев через расширенный метод, чтобы использовать их внутри сервисов
builder.Services.InitializeRepositories();
//Инициализация сервисов, чтобы использовать их в контроллерах
builder.Services.InitializeServices();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.MapHealthChecks("/health");
app.UseAuthorization();
app.UseMiddleware<AuthMiddleware>();

app.MapControllers();

app.Run();


