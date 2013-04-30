using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Events.Cafe;
using Edument.CQRS;

namespace CafeReadModels
{
    public class OpenTabs : IOpenTabQueries,
        ISubscribeTo<TabOpened>,
        ISubscribeTo<DrinksOrdered>,
        ISubscribeTo<FoodOrdered>,
        ISubscribeTo<FoodPrepared>,
        ISubscribeTo<DrinksServed>,
        ISubscribeTo<FoodServed>,
        ISubscribeTo<TabClosed>
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

        private class Tab
        {
            public int TableNumber;
            public string Waiter;
            public List<TabItem> ToServe;
            public List<TabItem> InPreparation;
            public List<TabItem> Served;
        }

        private Dictionary<Guid, Tab> todoByTab =
            new Dictionary<Guid,Tab>();

        public List<int> ActiveTableNumbers()
        {
            lock (todoByTab)
                return (from tab in todoByTab
                        select tab.Value.TableNumber
                       ).OrderBy(i => i).ToList();
        }

        public Dictionary<int, List<TabItem>> TodoListForWaiter(string waiter)
        {
            lock (todoByTab)
                return (from tab in todoByTab
                        where tab.Value.Waiter == waiter
                        select new
                        {
                            TableNumber = tab.Value.TableNumber,
                            ToServe = CopyItems(tab.Value, t => t.ToServe)
                        })
                        .Where(t => t.ToServe.Count > 0)
                        .ToDictionary(k => k.TableNumber, v => v.ToServe);
        }

        public Guid TabIdForTable(int table)
        {
            lock (todoByTab)
                return (from tab in todoByTab
                        where tab.Value.TableNumber == table
                        select tab.Key
                       ).First();
        }

        public TabStatus TabForTable(int table)
        {
            lock (todoByTab)
                return (from tab in todoByTab
                        where tab.Value.TableNumber == table
                        select new TabStatus
                        {
                            TabId = tab.Key,
                            TableNumber = tab.Value.TableNumber,
                            ToServe = CopyItems(tab.Value, t => t.ToServe),
                            InPreparation = CopyItems(tab.Value, t => t.InPreparation),
                            Served = CopyItems(tab.Value, t => t.Served)
                        })
                        .First();
        }

        public TabInvoice InvoiceForTable(int table)
        {
            KeyValuePair<Guid, Tab> tab;
            lock (todoByTab)
                tab = todoByTab.First(t => t.Value.TableNumber == table);

            lock (tab.Value)
                return new TabInvoice
                {
                    TabId = tab.Key,
                    TableNumber = tab.Value.TableNumber,
                    Items = new List<TabItem>(tab.Value.Served),
                    Total = tab.Value.Served.Sum(i => i.Price),
                    HasUnservedItems = tab.Value.InPreparation.Any() || tab.Value.ToServe.Any()
                };
        }

        private List<TabItem> CopyItems(Tab tableTodo, Func<Tab, List<TabItem>> selector)
        {
            lock (tableTodo)
                return new List<TabItem>(selector(tableTodo));
        }

        public void Handle(TabOpened e)
        {
            lock (todoByTab)
                todoByTab.Add(e.Id, new Tab
                {
                    TableNumber = e.TableNumber,
                    Waiter = e.Waiter,
                    ToServe = new List<TabItem>(),
                    InPreparation = new List<TabItem>(),
                    Served = new List<TabItem>()
                });
        }

        public void Handle(DrinksOrdered e)
        {
            AddItems(e.Id,
                e.Items.Select(drink => new TabItem
                    {
                        MenuNumber = drink.MenuNumber,
                        Description = drink.Description,
                        Price = drink.Price
                    }),
                t => t.ToServe);
        }

        public void Handle(FoodOrdered e)
        {
            AddItems(e.Id,
                e.Items.Select(drink => new TabItem
                {
                    MenuNumber = drink.MenuNumber,
                    Description = drink.Description,
                    Price = drink.Price
                }),
                t => t.InPreparation);
        }

        public void Handle(FoodPrepared e)
        {
            MoveItems(e.Id, e.MenuNumbers, t => t.InPreparation, t => t.ToServe);
        }

        public void Handle(DrinksServed e)
        {
            MoveItems(e.Id, e.MenuNumbers, t => t.ToServe, t => t.Served);
        }

        public void Handle(FoodServed e)
        {
            MoveItems(e.Id, e.MenuNumbers, t => t.ToServe, t => t.Served);
        }

        public void Handle(TabClosed e)
        {
            lock (todoByTab)
                todoByTab.Remove(e.Id);
        }

        private Tab getTab(Guid id)
        {
            lock (todoByTab)
                return todoByTab[id];
        }

        private void AddItems(Guid tabId, IEnumerable<TabItem> newItems, Func<Tab, List<TabItem>> to)
        {
            var tab = getTab(tabId);
            lock (tab)
                to(tab).AddRange(newItems);
        }

        private void MoveItems(Guid tabId, List<int> menuNumbers,
            Func<Tab, List<TabItem>> from, Func<Tab, List<TabItem>> to)
        {
            var tab = getTab(tabId);
            lock (tab)
            {
                var fromList = from(tab);
                var toList = to(tab);
                foreach (var num in menuNumbers)
                {
                    var serveItem = fromList.First(f => f.MenuNumber == num);
                    fromList.Remove(serveItem);
                    toList.Add(serveItem);
                }
            }
        }
    }
}
