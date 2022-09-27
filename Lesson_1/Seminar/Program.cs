using Seminar.Services;
using Seminar.Services.impl;
using System;
using System.Diagnostics;

namespace Seminar
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Подключить кодирование текста UTF8
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            IMessenger _mesenger = new ConsoleMessager();
            var rest = new Restaurant();
            while(true)
            {
                // Request an action from the user
                _mesenger.WriteMessage("Привет, желаете забронировать столик?\n1 - мы уведомим вас по смс (асинхронно)" +
                    "\n2 - подождите на линии, мы вас оповестим (синхронно)" + "\n3 - Вывод информации по столикам\n");
                if (!int.TryParse(Console.ReadLine(), out var choice)
                    && choice is not (1 or 2 or 3))
                {
                    _mesenger.WriteMessage("Неверный ввод. Введите пожалуйста 1, 2 или 3\n");
                    continue;
                }       
                var stopWatch = new Stopwatch();
                stopWatch.Start(); // start timer                

                // Обработка ответа пользователя:
                switch (choice)
                {
                    case 1:
                        _mesenger.WriteMessage("Введите колличество гостей");
                        int.TryParse(Console.ReadLine(), out int countOfPersons);
                        if(countOfPersons > 0)
                        {
                            rest.BookFreeTableAsync(countOfPersons);                            
                        }
                        break;
                    case 2:
                        _mesenger.WriteMessage("Введите колличество гостей");
                        int.TryParse(Console.ReadLine(), out int countOfPersons1);
                        if (countOfPersons1 > 0)
                        {
                            rest.BookFreeTable(countOfPersons1);
                        }
                        break;
                    case 3:
                        rest.ShowTablesStatus();
                        break;
                }
                _mesenger.WriteMessage("Спасибо за ваше обращение!\n");
                stopWatch.Stop();
                var ts = stopWatch.Elapsed;
                _mesenger.WriteMessage($"{ts.Seconds:00} : {ts.Milliseconds:00}"+ "\n"); // Show elapsed time
            }  
        }
    }
}
