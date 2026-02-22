using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "admin",
    Password = "admin"
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync("demo-exchange", ExchangeType.Direct);
await channel.QueueDeclareAsync("demo-queue", durable: false, exclusive: false, autoDelete: false);
await channel.QueueBindAsync("demo-queue", "demo-exchange", "demo-key");

var message = $"Teste mensagem enviada em {DateTime.Now}";
var body = Encoding.UTF8.GetBytes(message);

await channel.BasicPublishAsync(
    exchange: "demo-exchange",
    routingKey: "demo-key",
    body: body);

Console.WriteLine($"[x] Enviado: {message}");