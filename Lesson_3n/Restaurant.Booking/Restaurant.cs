using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Restaraunt.Booking.Services;
using Restaurant.Booking.Models;
using Restaurant.Messages;

namespace Restaurant.Booking
{
    public class Restaurant
    {
        private ConsoleMessager _messanger = new();
        //private readonly List<Table> _tables = new ();
        public static readonly ConcurrentDictionary<int, Table> _tables = new();
        public int numberLastBookedTable = -1;

        public Restaurant()
        {
            // adding tables. Restaurant has 10 tables by default
            for (ushort i = 0; i <= 10; i++)
            {
                _tables.TryAdd(i, new Table(i));
            }
        }
                
        #region Booking async Methods
        public async Task<TableBooked?> BookFreeTableAsync(int countOfPersons)
        {
            _messanger.WriteMessage("Спасибо за Ваше обращение, я подберу столик и подтвержу вашу бронь," +
                              "Вам придет уведомление");

            //var table = _tables.FirstOrDefault(t => t.Value.SeatsCount > countOfPersons
            //                                            && t.Value.State == State.Free);
            await Task.Delay(1000 * 3); //у нас нерасторопные менеджеры, 3 секунд они находятся в поисках стола
            BookingResult bookingResult = new();
            await Task.Run(async () =>
            {
                bookingResult = SerchingTables(countOfPersons);
                             
            });
            return new TableBooked(NewId.NextGuid(), NewId.NextGuid(), bookingResult.Success, bookingResult.BooketTablesLineString);
        }
        #endregion

        #region Show methods
        public void ShowTablesStatus()
        {
            var tables = _tables.ToList();

            for (int i = 0; i < tables.Count; i++)
            {
                var table = tables[i].Value;
                Console.WriteLine($"Столик №{table.Id}, Количество мест: {table.SeatsCount}, Статус брони: " +
                    (table.State == State.Free ? "Свободен\n" : "Занят\n"));
            }
        }
        #endregion

        #region Support private methods
        /// <summary>
        /// Поиск столов для бронирования, с подбором нескольких столов, еслси нехватает мест
        /// </summary>
        /// <param name="countOfPersons"> колличество посетителей </param>
        /// <returns>(текст с перечислением столов, список забронированных в результате столов)</returns>
        private BookingResult SerchingTables(int countOfPersons)
        {
            BookingResult bookingResult = new BookingResult();
            List<Table> tablesForBooking1 = new List<Table>();
            IEnumerable<Table> tablesForBooking2 = new List<Table>();
            // Выбор коллекции подходящих столиков
            List<Table> tablesForBooking = (from t in _tables.ToList()
                                                               where
                                                               t.Value.State == State.Free
                                                               // && t.Value.SeatCount >= countOfPersons
                                                               select t.Value).ToList<Table>();
            // Подбор столика согласно колличества гостей            
            if (tablesForBooking.Count > 0)
                tablesForBooking2 = tablesForBooking.OrderBy(x => x.SeatsCount); // сортировка по возврастанию
            int countSeatsForBooking = 0;
            for (int i = 0; i < tablesForBooking2.Count(); i++)
            {                
                Table table1 = tablesForBooking2.ElementAt(i);                   
                countSeatsForBooking = countSeatsForBooking + table1.SeatsCount;                   
                tablesForBooking1.Add(table1);
                if (countSeatsForBooking >= countOfPersons)
                    break;
            }
            if (countSeatsForBooking < countOfPersons)
            {
                _messanger.WriteMessage($"К сожалению, необходимого колличесва свободных столиков на данный момент нет\n");
                bookingResult.Success = false;
                return bookingResult;
            }
            // Бронирование столиков и ответ клиенту
            string numbersOfTables = "";
            foreach (Table table in tablesForBooking1)
            {
                table?.SetState(State.Booked);
                numbersOfTables = (numbersOfTables == ""
                    ? table.Id.ToString()
                    : numbersOfTables + ", " + table.Id);
            }
            // Подготовка результата бронирования
            bookingResult.BooketTablesLineString = numbersOfTables;
            bookingResult.Success = true;
            bookingResult.BooketTables = tablesForBooking1;
            return bookingResult;
        }
        #endregion

    }
}