using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Shared;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "admin",
    Password = "admin"
};

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

// Apenas garante que o exchange existe
await channel.ExchangeDeclareAsync(
    exchange: "orders-exchange",
    type: ExchangeType.Direct,
    durable: true);

var order = new OrderCreatedEvent
{
    OrderId = Guid.NewGuid(),
    Amount = 150, // >100 vai gerar retry
    CreatedAt = DateTime.UtcNow
};

var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));

var props = new BasicProperties
{
    Persistent = true
};

await channel.BasicPublishAsync(
    exchange: "orders-exchange",
    routingKey: "order.created",
    mandatory: false,
    basicProperties: props,
    body: body);

Console.WriteLine("✅ Evento publicado!");