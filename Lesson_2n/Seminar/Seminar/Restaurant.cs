using Restaraunt.Booking.Services.Inteerfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Concurrent;
using Restaraunt.Booking.Services;
using Messaging;

namespace Restaraunt.Booking
{
    internal class Restaurant
    {             
        private readonly Producer _producer = new("BookingNotification", "puffin-01.rmq2.cloudamqp.com");
        public static readonly ConcurrentDictionary<int, Table> _tables = new();
        private static System.Threading.Timer stateTimer = new System.Threading.Timer(
            Restaurant.OnTimedEvent,
            null, // An object containing information to be used by the callback method, or null.
            0,// Задержка перед стартом таймера
            30000); // период

        #region Constructors
        public Restaurant()
        {
            // adding tables. Restaurant has 10 tables by default
            for (ushort i = 0; i <= 10; i++)
            {
                _tables.TryAdd(i, new Table(i));
            }
        }

        #endregion

        #region Booking async Methods
        public async void BookFreeTableAsync(int countOfPersons)
        {
            _producer.Send("Добрый день! Подождите секунду, я подберу столик и подтвержу вашу бронь, оставайтесь на линии.\n");
            await Task.Run(async () =>
            {
                var s = SerchingTables(countOfPersons);
                var numbersOfTables = s.Item1;
                var tablesForBooking1 = s.Item2;
                await Task.Delay(1000 * 5);// 5 seconds for the searching table by the waiter 
                if (!(numbersOfTables == null || tablesForBooking1 == null))
                {
                    _producer.Send(tablesForBooking1.Count() > 1                        
                        ? $"Для вас забранированны столики № {numbersOfTables}.\n"
                        : $"Готово! Ваш столик номер{tablesForBooking1[0].Id}\n");
                }
            }); 
        }

        public void CancelBookTableAsync(int id)
        {
            Task.Run(async () =>
            {
                // await Task.Delay(1000 * 5);
                var table = _tables.FirstOrDefault(t => t.Value.Id == id).Value;
                if (table != null)
                {
                    if (table.State == State.Booked)
                    {
                        table.SetState(State.Free);
                        _producer.Send($"Бронь снята со столика номер {table.Id}\n");
                    }
                    else
                    {
                        _producer.Send("Столик небыл забронирован.\n");
                    }
                }
                else
                {
                    _producer.Send($"Столик номер{id} не найден\n");
                }
            });
        }
        #endregion

        #region Support private methods
        /// <summary>
        /// Поиск столов для бронирования, с подбором нескольких столов, еслси нехватает мест
        /// </summary>
        /// <param name="countOfPersons"> колличество посетителей </param>
        /// <returns>(текст с перечислением столов, список забронированных в результате столов)</returns>
        private (string, List<Table>) SerchingTables(int countOfPersons)
        {
            List<Restaraunt.Booking.Table> tablesForBooking1 = new List<Table>();
            IEnumerable<Table> tablesForBooking2 = new List<Table>();
            // Выбор коллекции подходящих столиков
            List<Restaraunt.Booking.Table> tablesForBooking = (from t in _tables.ToList()
                                                               where
                                                               t.Value.State == State.Free
                                                               // && t.Value.SeatCount >= countOfPersons
                                                               select t.Value).ToList<Restaraunt.Booking.Table>();
            // Подбор столика согласно колличества гостей            
            if (tablesForBooking.Count > 0)
                tablesForBooking2 = tablesForBooking.OrderBy(x => x.SeatCount); // сортировка по возврастанию
            int countSeatsForBooking = 0;
            for (int i = 0; i < tablesForBooking2.Count(); i++)
            {
                Table table1 = tablesForBooking2.ElementAt(i);
                countSeatsForBooking = countSeatsForBooking + table1.SeatCount;
                tablesForBooking1.Add(table1);
                if (countSeatsForBooking >= countOfPersons)
                    break;
            }
            if (countSeatsForBooking < countOfPersons)
            {
                _producer.Send($"К сожалению, необходимого колличесва свободных столиков на данный момент нет\n");
                return (null, null);
            }
            // Бронирование столиков и ответ клиенту
            string numbersOfTables = "";

            //for (int i = 0; i < tablesForBooking1.Count(); i++)
            //{
            foreach (Table table in tablesForBooking1)
            {
                table?.SetState(State.Booked);
                numbersOfTables = (numbersOfTables == ""
                    ? table.Id.ToString()
                    : numbersOfTables + ", " + table.Id);
            }
            return (numbersOfTables, tablesForBooking1);
        }
        #endregion

        #region Show methods
        public void ShowTablesStatus()
        {
            var tables = _tables.ToList();

            for (int i = 0; i < tables.Count; i++)
            {
                var table = tables[i].Value;
                _producer.Send($"Столик №{table.Id}, Количество мест: {table.SeatCount}, Статус брони: " +
                    (table.State == State.Free ? "Свободен\n" : "Занят\n"));
            }
        }

        #endregion

        #region Timer service

        private static void OnTimedEvent(object obj)
        {                    
            Producer _producer = new("BookingNotification", "puffin-01.rmq2.cloudamqp.com");
        
            for (int i = 0; i < _tables.Count; i++)
            {
                Restaraunt.Booking.Table table;

                _tables.TryGetValue(i, out table);
                if (table != null)
                    table.SetState(State.Free);
            }
            _producer.Send("Атоматическое снятие брони\n");
        }

        #endregion

    }
}
