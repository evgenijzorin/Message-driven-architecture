using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Restaraunt.Booking.Services;
using Restaurant.Booking.Models;
using Restaurant.Messages;

namespace Restaurant.Booking
{
    // Работник ресторана принимающий заказы 
    public class Worker :
        BackgroundService // Определяет методы для объектов, управляемых узлом.
    {
        private ConsoleMessager _messenger = new();
        private readonly IBus _bus;
        private readonly Restaurant _restaurant;

        #region Constructors
        public Worker(IBus bus, Restaurant restaurant)
        {
            _bus = bus;
            _restaurant = restaurant;
        }
        #endregion

        #region Методы базового класса BackgroundService интерфейса IHostedService 
        /// <summary>
        /// Этот метод вызывается при запуске IHostedService. Реализация должна возвращать задачу, представляющую время существования длительно выполняемых операций.
        /// </summary>
        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken) // токен отмены. Распространяет уведомление о том, что операции следует отменить.
        {                
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            //Выполнение циклического действия обращения к клиенту и запроса команд
            while (!stoppingToken.IsCancellationRequested)
            {
                TableBooked tableBooked = null;
                // Request an action from the user
                _messenger.WriteMessage("Привет, желаете забронировать столик?\n1 - мы уведомим вас по смс (асинхронно)" 
                    + "\n2 - Вывод информации по столикам\n");
                if (!int.TryParse(Console.ReadLine(), out var choice)
                    && choice is not (1 or 2 or 3))
                {
                    _messenger.WriteMessage("Неверный ввод. Введите пожалуйста 1, 2 \n");
                    continue;
                }
                var stopWatch = new Stopwatch();
                stopWatch.Start(); // start timer                

                // Обработка ответа пользователя:
                switch (choice)
                {
                    case 1:
                        _messenger.WriteMessage("Введите колличество гостей");
                        int.TryParse(Console.ReadLine(), out int countOfPersons);
                        if (countOfPersons > 0)
                        {
                            tableBooked = await _restaurant.BookFreeTableAsync(countOfPersons);                            
                        }
                        break;
                    case 2:
                        _restaurant.ShowTablesStatus();
                        break;
                }
                _messenger.WriteMessage("Спасибо за ваше обращение!\n");
                stopWatch.Stop();
                var ts = stopWatch.Elapsed;
                _messenger.WriteMessage($"{ts.Seconds:00} : {ts.Milliseconds:00}" + "\n"); // Show elapsed time
                                
                // Опубликовать сообщение 
                if(tableBooked != null)
                {
                    await _bus.Publish(tableBooked,
                    context => context.Durable = false, stoppingToken);
                }

            }
        }
        #endregion
    }
}