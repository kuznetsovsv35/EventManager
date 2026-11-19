using Microsoft.Extensions.Configuration;

namespace Common.Kafka.Configuration;

public class ConfigBase(IConfiguration config)
{
    static readonly string _serversKey = $"{nameof(Kafka)}:{nameof(BootstrapServers)}";
    
    public string BootstrapServers { get; set; } = config[_serversKey]
        ?? throw new InvalidOperationException($@"""{_serversKey}"" configuration key is required");
}
