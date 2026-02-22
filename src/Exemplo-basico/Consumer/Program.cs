using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (sender, ea) =>
{
    try
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        Console.WriteLine($"[x] Recebido: {message}");

        // Confirma processamento
        await channel.BasicAckAsync(ea.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao processar: {ex.Message}");

        // Rejeita e reenvia para fila
        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
    }
};

await channel.BasicConsumeAsync(
    queue: "demo-queue",
    autoAck: false,
    consumer: consumer);

Console.WriteLine("Aguardando mensagens...");
Console.ReadLine();