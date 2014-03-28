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
        internal enum ItemStatus { NeedsPreparing, NeedsServing, Served }

        public class TabItem
        {
            public int MenuNumber;
            public string Description;
            internal ItemStatus Status;
        }

        private class Tab
        {
            public int TableNumber;
            public string Waiter;
            public List<TabItem> Items;
        }

        private Dictionary<Guid, Tab> todoByTab = new Dictionary<Guid, Tab>();

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
                        let toServe = tab.Value.Items
                            .Where(i => i.Status == ItemStatus.NeedsServing)
                            .ToList()
                        where toServe.Count > 0
                        select new
                        {
                            TableNumber = tab.Value.TableNumber,
                            ToServe = toServe
                        })
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

        public void Handle(TabOpened e)
        {
            lock (todoByTab)
                todoByTab.Add(e.Id, new Tab
                {
                    TableNumber = e.TableNumber,
                    Waiter = e.Waiter,
                    Items = new List<TabItem>()
                });
        }

        public void Handle(DrinksOrdered e)
        {
            AddItems(e.Id,
                e.Items.Select(drink => new TabItem
                    {
                        MenuNumber = drink.MenuNumber,
                        Description = drink.Description,
                        Status = ItemStatus.NeedsServing
                    }));
        }

        public void Handle(FoodOrdered e)
        {
            AddItems(e.Id,
                e.Items.Select(food => new TabItem
                {
                    MenuNumber = food.MenuNumber,
                    Description = food.Description,
                    Status = ItemStatus.NeedsPreparing
                }));
        }

        public void Handle(FoodPrepared e)
        {
            ChangeStatus(e.Id, e.MenuNumbers, ItemStatus.NeedsPreparing, ItemStatus.NeedsServing);
        }

        public void Handle(DrinksServed e)
        {
            ChangeStatus(e.Id, e.MenuNumbers, ItemStatus.NeedsServing, ItemStatus.Served);
        }

        public void Handle(FoodServed e)
        {
            ChangeStatus(e.Id, e.MenuNumbers, ItemStatus.NeedsServing, ItemStatus.Served);
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

        private void AddItems(Guid tabId, IEnumerable<TabItem> newItems)
        {
            var tab = getTab(tabId);
            lock (tab)
                tab.Items.AddRange(newItems);
        }

        private void ChangeStatus(Guid id, List<int> menuNumbers, ItemStatus origStatus, ItemStatus newStatus)
        {
            var tab = getTab(id);
            lock (tab)
                foreach (var mn in menuNumbers)
                {
                    tab.Items
                        .First(i => i.MenuNumber == mn && i.Status == origStatus)
                        .Status = ItemStatus.NeedsServing;
                }
        }
    }
}
