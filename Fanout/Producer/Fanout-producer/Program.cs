using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fanout_producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            var manualResetEvent = new ManualResetEvent(initialState: false);

            manualResetEvent.Reset();

            using (var connection = factory.CreateConnection())
            {
                var queueName = "order";

                var channel = SetupChannel(connection);

                BuildAndRunPublishers(channel, queueName, "Produtor A", manualResetEvent);

                manualResetEvent.WaitOne();
            }

        }

        public static IModel SetupChannel(IConnection connection)
        {
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "order", durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: "logs", durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: "finance_orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

            channel.ExchangeDeclare("order", type: "fanout");

            channel.QueueBind("order", "order", ""); //Exchange do tipo Fanout ignora o RoutingKey, mais usado no tipo Direct
            channel.QueueBind("logs", "order", "");
            channel.QueueBind("finance_orders", "order", "");

            return channel;
        }

        public static void BuildAndRunPublishers(IModel chanel, string queueName, string publisherName, ManualResetEvent manualResetEvent)
        {
            Task.Run(() =>
            {
                int count = 0;
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Pressione qualquer tecla para produzir 10 mensagens.");
                        Console.ReadLine();

                        for (int i = 0; i < 10; i++)
                        {
                            var message = $"OrderNumber: {count++} from {publisherName})";
                            var body = Encoding.UTF8.GetBytes(message);

                            chanel.BasicPublish(exchange: "order", "", null, body);

                            Console.WriteLine($"{publisherName} - [x] Sent {count}", message);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                        manualResetEvent.Set();
                    }
                }
            });
        }

    }
}
