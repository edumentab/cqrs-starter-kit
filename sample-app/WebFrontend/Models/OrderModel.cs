using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebFrontend.Models
{
    public class OrderModel
    {
        public class OrderItem
        {
            public int MenuNumber { get; set; }
            public string Description { get; set; }
            public int NumberToOrder { get; set; }
        }

        public List<OrderItem> Items { get; set; }
    }
}