using Restaurant.Booking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Booking
{
    /// <summary>
    /// Результат бронирования столиков
    /// </summary>
    internal class BookingResult
    {
        internal bool Success { get; set;} // успешность бронирования
        internal List<Table> BooketTables { get; set; } = new List<Table>(); // список забронированных столов
        internal string BooketTablesLineString { get; set; }
    }
}
