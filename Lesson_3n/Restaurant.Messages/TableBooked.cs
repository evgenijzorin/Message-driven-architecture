
using Restaurant.Messages.Interfaces;
using System;
using System.Collections.Generic;

namespace Restaurant.Messages
{
    public class TableBooked : ITableBooked
    {
        public TableBooked(Guid orderId, Guid clientId, bool success, string booketTablesLineString, Dish? preOrder = null )
        {
            OrderId = orderId;
            ClientId = clientId;
            Success = success;
            PreOrder = preOrder;
            BooketTablesLineString = booketTablesLineString;   
        }

        public Guid OrderId { get; }
        public Guid ClientId { get; }
        public Dish? PreOrder { get; }
        public bool Success { get; }
        public string BooketTablesLineString { get; }
    }
}