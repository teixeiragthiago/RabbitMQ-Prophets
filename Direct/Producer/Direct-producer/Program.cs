using Direct_producer.Models;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Direct_producer
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
            channel.QueueDeclare(queue: "finance_orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

            channel.ExchangeDeclare("order", type: "direct");

            channel.QueueBind("order", "order", "order_new"); 
            channel.QueueBind("order", "order", "order_upd");
            channel.QueueBind("finance_orders", "order", "order_new");

            return channel;
        }

        public static void BuildAndRunPublishers(IModel chanel, string queueName, string publisherName, ManualResetEvent manualResetEvent)
        {
            Task.Run(() =>
            {
                var idIndex = 1;
                var random = new Random(DateTime.UtcNow.Millisecond * DateTime.UtcNow.Second);
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Pressione qualquer tecla para produzir 10 mensagens.");
                        Console.ReadLine();

                        var order = new Order(idIndex++, random.Next(1000, 9999));
                        var message1 = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));

                        chanel.BasicPublish("order", "order_new", basicProperties: null, message1);
                        Console.WriteLine($"New order_ Id: {order.Id}, Amount: {order.Amount} | Created: {order.CreateDate}");


                        order.UpdateOrder(random.Next(100, 999));
                        var message2 = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));

                        chanel.BasicPublish("order", "order_new", basicProperties: null, message2);
                        Console.WriteLine($"New order_ Id: {order.Id}, Amount: {order.Amount} | LastUpdated: {order.LastUpdated}");


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
