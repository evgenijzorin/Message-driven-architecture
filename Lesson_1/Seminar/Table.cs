using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seminar
{
    public class Table
    {
        public State State { get; private set; }
        public int SeatCount { get; }
        public int Id { get; }
        public Table(int id)
        {
            Id = id;
            State = State.Free;
            Random rand = new Random();
            SeatCount = rand.Next(2,5); // number of seats at the table
        }

        public bool SetState(State state)
        {
            if (state == State)
                return false;
            State = state;
            return true;
        }
    }
}
