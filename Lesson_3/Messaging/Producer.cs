using System.Collections;
using System.Text;
using RabbitMQ.Client;

namespace Messaging
{
    /// <summary>
    /// Отправитель
    /// </summary>
    public class Producer
    {
        
        private readonly string _queueName; // имя очереди
        private readonly string _hostName; // имя хоста

        #region Constructors
        public Producer(string queueName, string hostName)
        {
            _queueName = queueName;
            _hostName = hostName; 
        }
        #endregion

        #region Methods
        public void Send(string message)
        {
            // конфигурировать соединение
            var factory = new ConnectionFactory()
            {
                HostName = _hostName,
                Port = 5672,
                UserName = "shaizeye",
                Password = "PuMn5K_5msxcIyyPhFqQRw-e6A3hRLX3",
                VirtualHost = "shaizeye"

            };

            // создать соединение
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            // Задать точку обмена
            channel.ExchangeDeclare(
                "direct_exchange",
                "direct",
                false,
                false,
                null
            );

            // формируем тело сообщения для отправки
            var body = Encoding.UTF8.GetBytes(message); 

            // Опубликовать сообщение
            channel.BasicPublish(exchange: "direct_exchange",
                routingKey: _queueName,
                basicProperties: null,
                body: body); //отправляем сообщение
        }
        #endregion
    }
}