using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cafe.Tab
{
    public class TabItem
    {
        public int MenuNumber;
        public string Description;
        public decimal Price;
    }

    public class TabStatus
    {
        public Guid TabId;
        public int TableNumber;
        public List<TabItem> ToServe;
        public List<TabItem> InPreparation;
        public List<TabItem> Served;
    }

    public class TabInvoice
    {
        public Guid TabId;
        public int TableNumber;
        public List<TabItem> Items;
        public decimal Total;
        public bool HasUnservedItems;
    }
}
