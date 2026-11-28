using Microsoft.Extensions.Configuration;

namespace Common.Kafka.Configuration;

public class ConfigBase(IConfiguration config)
{
    static readonly string _serversKey = $"{nameof(Kafka)}:{nameof(BootstrapServers)}";
    static readonly string _topicKey = $"{nameof(Kafka)}:{nameof(Topic)}";

    public string BootstrapServers { get; set; } = config[_serversKey]
        ?? throw new InvalidOperationException($@"""{_serversKey}"" configuration key is required");
    public string Topic { get; } = config[_topicKey] ?? throw new InvalidOperationException($"{_topicKey} is required");
}

