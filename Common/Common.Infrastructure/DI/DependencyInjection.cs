using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.DI;

public static class DependencyInjection
{
    public static IMvcBuilder ConfigureJsonSerializer(this IMvcBuilder builder)
        => builder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });
}