using EventManager.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Добавляем функциональность приложения.
builder.Services.ConfigureApplication();

// Добавляем инфраструктуру.
builder.Services.ConfigureInfrastructure(builder.Configuration, builder.Environment);

// Добавляем представления.
builder.Services.ConfigurePresentation();

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

// Запускаем Presentation
app.RunPresentation();

// Запускаем приложение.
await app.Services.RunInfrastructure(CancellationToken.None);
await app.RunAsync();
