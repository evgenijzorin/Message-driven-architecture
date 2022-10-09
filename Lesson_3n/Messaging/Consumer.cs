using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging
{
    /// <summary>
    /// Получатель
    /// </summary>
    public class Consumer : IDisposable
    {
        private readonly string _queueName; //название очереди
        private readonly string _hostName; //хостнейм
        private readonly IConnection _connection;
        private readonly IModel _channel;

        #region Constructors
        public Consumer(string queueName, string hostName)
        {
            _queueName = queueName;
            _hostName = hostName; //hostName;
            var factory = new ConnectionFactory()
            {
                HostName = _hostName,
                Port = 5672,
                UserName = "shaizeye",
                Password = "PuMn5K_5msxcIyyPhFqQRw-e6A3hRLX3",
                VirtualHost = "shaizeye"

            };
            _connection = factory.CreateConnection(); //создаем подключение
            _channel = _connection.CreateModel();
        }
        #endregion
    
        #region public methods
        public void Receive (EventHandler<BasicDeliverEventArgs> receiveCallback)
        {
            // Объявить точку обмена
            _channel.ExchangeDeclare(exchange: "direct_exchange",
                type: "direct"); 
            
            // объявить очередь
            _channel.QueueDeclare(queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null); 
            
            // Связываем точку обмена и очередь
            _channel.QueueBind(queue: _queueName,
                exchange: "direct_exchange",
                routingKey: _queueName);

            // создаем consumer для канала
            var consumer = new EventingBasicConsumer(_channel);

            // добавляем обработчик события приема сообщения
            consumer.Received += receiveCallback; 

            // Старт приема сообщения
            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }
        #endregion

        #region private Methods
        public void Dispose()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }
        #endregion
    }
}