using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Shared;

public class Worker : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "admin",
            Password = "admin"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await DeclareTopologyAsync(stoppingToken);

        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var order = JsonSerializer.Deserialize<OrderCreatedEvent>(message);

                Console.WriteLine($"📦 Processando pedido {order?.OrderId}");

                if (order == null)
                    throw new Exception("Mensagem inválida");

                // Simular erro
                if (order.Amount > 100)
                    throw new Exception("Erro simulado");

                await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

                Console.WriteLine("✅ Processado com sucesso");
            }
            catch (Exception ex)
            {
                await HandleRetryAsync(ea, ex, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "orders-queue",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        Console.WriteLine("🚀 Worker iniciado e aguardando mensagens...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task DeclareTopologyAsync(CancellationToken cancellationToken)
    {
        // Exchanges
        await _channel!.ExchangeDeclareAsync("orders-exchange", ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync("orders-retry-exchange", ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync("orders-dlx", ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken);

        // Fila principal
        await _channel.QueueDeclareAsync(
            queue: "orders-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", "orders-retry-exchange" }
            },
            cancellationToken: cancellationToken);

        // Retry queue (TTL 5s)
        await _channel.QueueDeclareAsync(
            queue: "orders-retry-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-message-ttl", 5000 },
                { "x-dead-letter-exchange", "orders-exchange" }
            },
            cancellationToken: cancellationToken);

        // DLQ final
        await _channel.QueueDeclareAsync(
            queue: "orders-dlq",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Bindings
        await _channel.QueueBindAsync("orders-queue", "orders-exchange", "order.created", cancellationToken: cancellationToken);
        await _channel.QueueBindAsync("orders-retry-queue", "orders-retry-exchange", "order.created", cancellationToken: cancellationToken);
        await _channel.QueueBindAsync("orders-dlq", "orders-dlx", "", cancellationToken: cancellationToken);
    }

    private async Task HandleRetryAsync(BasicDeliverEventArgs ea, Exception ex, CancellationToken cancellationToken)
    {
        var retryCount = 0;

        if (ea.BasicProperties?.Headers != null &&
            ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var value) &&
            value != null)
        {
            retryCount = Convert.ToInt32(value);
        }

        if (retryCount >= 3)
        {
            Console.WriteLine($"❌ Falha definitiva → DLQ | {ex.Message}");

            await _channel!.BasicPublishAsync(
                exchange: "orders-dlx",
                routingKey: "",
                mandatory: false,
                basicProperties: new BasicProperties { Persistent = true },
                body: ea.Body,
                cancellationToken: cancellationToken);

            await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
            return;
        }

        Console.WriteLine($"🔁 Retry {retryCount + 1} | {ex.Message}");

        var props = new BasicProperties
        {
            Headers = new Dictionary<string, object?>
            {
                { "x-retry-count", retryCount + 1 }
            },
            Persistent = true
        };

        await _channel!.BasicPublishAsync(
            exchange: "orders-retry-exchange",
            routingKey: "order.created",
            mandatory: false,
            basicProperties: props,
            body: ea.Body,
            cancellationToken: cancellationToken);

        await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}