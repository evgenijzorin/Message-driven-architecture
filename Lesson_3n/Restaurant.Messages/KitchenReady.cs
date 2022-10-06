using System;

namespace Restaurant.Messages
{
    public interface IKitchenReady
    {
        public Guid OrderId { get; }
        
        public bool Ready { get; }
        public string BooketTablesLineString { get; }
    }

    public class KitchenReady : IKitchenReady
    {
        public KitchenReady(Guid orderId, bool ready, string booketTablesLineString)
        {
            OrderId = orderId;
            Ready = ready;
            BooketTablesLineString = booketTablesLineString;  
        }

        public Guid OrderId { get; }
        public bool Ready { get; }
        public string BooketTablesLineString { get; }
    }
}