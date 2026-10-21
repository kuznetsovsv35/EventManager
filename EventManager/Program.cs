using EventManager.DependencyInjection;

namespace EventManager;

public partial class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Добавление поддержки IOption<>
        builder.Services.AddOptions();

        // Добавляем функциональность приложения.
        builder.Services.ConfigureApplication();

        // Добавляем инфраструктуру.
        builder.Services.ConfigureInfrastructure(builder.Configuration, builder.Environment);

        // Добавляем представления.
        builder.Services.ConfigurePresentation(builder.Configuration);

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
    }
}

