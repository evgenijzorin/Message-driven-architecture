using Seminar.Services;
using Seminar.Services.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Concurrent;

namespace Seminar
{
    internal class Restaurant
    {
        private IMessenger _messenger = new ConsoleMessager();
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

        #region Booking Methods
        public void BookFreeTable(int countOfPersons)
        {
            var s = SerchingTables(countOfPersons);
            var numbersOfTables = s.Item1;
            var tablesForBooking1 = s.Item2;
            Thread.Sleep(1000 * 5); // 5 seconds for the searching table by the waiter
            if (!(numbersOfTables == null || tablesForBooking1 == null))
            {
                _messenger.WriteMessageAsync(tablesForBooking1.Count() < 1
                    ? $"Для вас забранированны столики № {numbersOfTables}.\n"
                    : $"Готово! Ваш столик номер{tablesForBooking1[0].Id}\n", 5);
            }
        }
        public async void BookFreeTableAsync(int countOfPersons)
        {
            _messenger.WriteMessage("Добрый день! Подождите секунду, я подберу столик и подтвержу вашу бронь, оставайтесь на линии.\n");
            await Task.Run(async () =>
            {
                var s = SerchingTables(countOfPersons);
                var numbersOfTables = s.Item1;
                var tablesForBooking1 = s.Item2;
                await Task.Delay(1000 * 5);// 5 seconds for the searching table by the waiter 
                if (!(numbersOfTables == null || tablesForBooking1 == null))
                {
                    await _messenger.WriteMessageAsync(tablesForBooking1.Count() > 1
                        ? $"Для вас забранированны столики № {numbersOfTables}.\n"
                        : $"Готово! Ваш столик номер{tablesForBooking1[0].Id}\n", 5);
                }
            }); 
        }

        public void CancelBookTable(int id)
        {
            Thread.Sleep(1000 * 5);
            var table = _tables.FirstOrDefault(t => t.Value.Id == id).Value;
            if (table != null)
            {
                if (table.State == State.Booked)
                {
                    table.SetState(State.Free);
                    _messenger.WriteMessage($"Бронь снята со столика номер {table.Id}\n");
                }
                else
                {
                    _messenger.WriteMessage("Столик небыл забронирован.\n");
                }
            }
            else
            {
                _messenger.WriteMessage($"Столик номер{id} не найден\n");
            }
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
                        await _messenger.WriteMessageAsync($"Бронь снята со столика номер {table.Id}\n", 5);
                    }
                    else
                    {
                        _messenger.WriteMessage("Столик небыл забронирован.\n");
                    }
                }
                else
                {
                    _messenger.WriteMessage($"Столик номер{id} не найден\n");
                }
            });
        }

        private (string, List<Table>) SerchingTables(int countOfPersons)
        {
            List<Seminar.Table> tablesForBooking1 = new List<Table>();
            IEnumerable<Table> tablesForBooking2 = new List<Table>();
            // Выбор коллекции подходящих столиков
            List<Seminar.Table> tablesForBooking = (from t in _tables.ToList()
                                                    where
                                                    t.Value.State == State.Free
                                                    // && t.Value.SeatCount >= countOfPersons
                                                    select t.Value).ToList<Seminar.Table>();
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
                _messenger.WriteMessage($"К сожалению, необходимого колличесва свободных столиков на данный момент нет\n");
                return (null, null);
            }
            // Бронирование столиков и ответ клиенту
            string numbersOfTables = "";

            //for (int i = 0; i < tablesForBooking1.Count(); i++)
            //{
            foreach(Table table in tablesForBooking1)
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
                _messenger.WriteMessage($"Столик №{table.Id}, Количество мест: {table.SeatCount}, Статус брони: " +
                    (table.State == State.Free ? "Свободен\n" : "Занят\n"));
            }
        }

        #endregion

        #region Timer service

        private static void OnTimedEvent(object obj)
        {
            IMessenger _messenger = new ConsoleMessager();
            for (int i = 0; i < _tables.Count; i++)
            {
                Seminar.Table table;

                _tables.TryGetValue(i, out table);
                if (table != null)
                    table.SetState(State.Free);
            }
            _messenger.WriteMessage("Атоматическое снятие брони\n");
        }

        #endregion

    }
}
