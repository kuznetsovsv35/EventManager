using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Kafka.Services;

public class EventConsumer<T>(
    IEventConsumerConfig<T> config, 
    ILogger<EventConsumer<T>> logger, 
    IEventSubscriber<T> subscriber) : BackgroundService
{
    static readonly TimeSpan RecoveryTime = TimeSpan.FromSeconds(1);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = config.BootstrapServers,
                GroupId = config.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }).Build();

            consumer.Subscribe(config.Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (result is null) continue;

                    try
                    {
                        if (JsonSerializer.Deserialize<T>(result.Message.Value) is T message)
                            await subscriber.HandleMessage(message, stoppingToken);

                        consumer.Commit(result);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to process {M}", nameof(T));
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) 
                { 
                    logger.LogError(ex, "Consumer failed");
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(RecoveryTime, stoppingToken); 
                        logger.LogInformation("Consumer will be recovery");
                    }
                }
            }
        }, stoppingToken);
    }
}
