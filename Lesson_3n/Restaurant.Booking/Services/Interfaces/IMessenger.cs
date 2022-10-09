using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaraunt.Booking.Services.Inteerfaces
{
    internal interface IMessenger
    {
        public void WriteMessage(string message);
        public void WriteMessage(string message, int seconds);
        public Task WriteMessageAsync(string message, int seconds);
    }
}
