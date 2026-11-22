namespace Common.Kafka.Configuration;

public class KafkaSettings
{
    public string BootstrapServer { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}
