﻿using System;
using System.Runtime.Serialization;
using Ses.Abstracts;

namespace Ses.Samples.Cart
{
    [DataContract(Name = "ShoppingCartCreated")]
    public class ShoppingCartCreated : IEvent
    {
        protected ShoppingCartCreated() { }

        public Guid CartId { get; private set; }
        public Guid CustomerId { get; private set; }

        public ShoppingCartCreated(Guid cartId, Guid customerId)
        {
            CartId = cartId;
            CustomerId = customerId;
        }
    }
}