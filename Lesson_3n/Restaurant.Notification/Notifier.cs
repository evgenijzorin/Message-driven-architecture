using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Restaurant.Notification
{
    public class Notifier
    {
        //импровизированный кэш для хранения статусов, номера заказа и клиента
        private readonly ConcurrentDictionary<Guid, Tuple<Guid?, Accepted>> _state = new ();
        
        // Метод который использует consumer Для сохранения статусов, номеров заказов, и вызова уведомления о бронировании
        public void Accept(Guid orderId, Accepted accepted,string numbersTable ,Guid? clientId = null)
        {
            _state.AddOrUpdate(orderId, new Tuple<Guid?, Accepted>(clientId, accepted),
                (guid, oldValue) => new Tuple<Guid?, Accepted>(
                    oldValue.Item1 ?? clientId, oldValue.Item2 | accepted));
            
            Notify(orderId, numbersTable);
        }

        private void Notify(Guid orderId, string numbersTable)
        {
            var booking = _state[orderId];
            
            switch (booking.Item2)
            {
                case Accepted.All:
                    Console.WriteLine($"Успешно забронирован столик {numbersTable} для клиента {booking.Item1}");
                    _state.Remove(orderId, out _);
                    break;
                case Accepted.Rejected:
                    Console.WriteLine($"Гость {booking.Item1}, к сожалению, все столики заняты");
                    _state.Remove(orderId, out _);
                    break;
                case Accepted.Kitchen:
                case Accepted.Booking:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}