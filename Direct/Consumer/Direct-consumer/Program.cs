using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Direct_consumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                channel.QueueDeclare(string.Empty, durable: false, exclusive: false, autoDelete: false, arguments: null);

                BuildAndRunWorker(channel, "Worker A1", "order");
                BuildAndRunWorker(channel, "Worker A2", "order");
                BuildAndRunWorker(channel, "Worker A3", "order");
                BuildAndRunWorker(channel, "Worker B1", "finance_orders");
                BuildAndRunWorker(channel, "Worker B2", "finance_orders");
                BuildAndRunWorker(channel, "Worker B3", "finance_orders");

                Console.ReadLine();
            }
        }

        public static void BuildAndRunWorker(IModel channel, string workerName, string queueName)
        {
            channel.BasicQos(0, 7, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    Console.WriteLine($"{channel.ChannelNumber} - {workerName}: [x] Received: {message} from Queue: {queueName}");

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }
    }
}
