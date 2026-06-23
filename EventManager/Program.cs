using EventManager.Application;
using EventManager.Infrastructure;
using EventManager.Presentation;

var builder = WebApplication.CreateBuilder(args);

// Добавляем инфраструктуру.
builder.Services.AddInfrastructure(builder.Configuration);
// Добавляем функциональность приложения.
builder.Services.AddApplication();
// Добавляем представления.
builder.Services.AddPresentation();

// Включаем проверку построения.
if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });
}

// Строим приложение.
var app = builder.Build();

// В разработке работа с API в веб-интерфейсе.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Мапируем контролеры.
app.MapControllers();

// Запускаем приложение.
await app.RunAsync();
