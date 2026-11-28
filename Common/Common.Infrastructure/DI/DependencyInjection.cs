using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure.DI;

public static class DependencyInjection
{
    public static IMvcBuilder ConfigureJsonSerializer(this IMvcBuilder builder)
    {
        builder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        builder.Services.AddSingleton(p => p.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions);

        return builder;
    }
}