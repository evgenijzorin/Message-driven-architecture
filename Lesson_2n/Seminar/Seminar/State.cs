using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaraunt.Booking
{
    public enum State
    {
        /// <summary>
        /// The table is free
        /// </summary>
        Free = 0,
        /// <summary>
        /// the table is occupied
        /// </summary>
        Booked = 1
    }
}
