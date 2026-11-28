using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Kafka.Services;

public class EventConsumer<T>(
    IEventConsumerConfig<T> config,
    ILogger<EventConsumer<T>> logger,
    IEventSubscriber<T> subscriber) : BackgroundService
{
    static readonly TimeSpan RecoveryTime = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureTopic();
        await ProcessEvents(stoppingToken);
    }

    async Task ProcessEvents(CancellationToken cancellation)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = config.BootstrapServers,
            GroupId = config.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        }).Build();

        consumer.Subscribe(config.Topic);

        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null) continue;

                try
                {
                    if (JsonSerializer.Deserialize<T>(result.Message.Value) is T message)
                        await subscriber.HandleMessage(message, cancellation);

                    consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process {M}", typeof(T).Name);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Consumer failed");
                if (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(RecoveryTime, cancellation);
                    logger.LogInformation("Consumer will be recovery");
                }
            }
        }

    }

    async Task EnsureTopic()
    {
        var adminConfig = new AdminClientConfig { BootstrapServers = config.BootstrapServers };
        using var admin = new AdminClientBuilder(adminConfig).Build();

        try
        {
            var metadata = admin.GetMetadata(config.Topic, TimeSpan.FromSeconds(10));
            if (metadata.Topics.Any(t => t.Topic == config.Topic && !t.Error.IsError))
                return; // уже есть
        }
        catch (KafkaException) { /* метаданные недоступны — попробуем создать */ }

        try
        {
            await admin.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = config.Topic,
                    NumPartitions = 3,
                    ReplicationFactor = 1
                }
            ]);
        }
        catch (CreateTopicsException ex)
        {
            // TOPIC_ALREADY_EXISTS — норма, топик создал другой сервис
            if (!ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
                throw;
        }
    }
}
