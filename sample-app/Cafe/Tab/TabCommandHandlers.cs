using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edument.CQRS;
using System.Collections;
using Events.Cafe;

namespace Cafe.Tab
{
    public class TabCommandHandlers :
        IHandleCommand<OpenTab, TabAggregate>,
        IHandleCommand<PlaceOrder, TabAggregate>,
        IHandleCommand<MarkDrinksServed, TabAggregate>,
        IHandleCommand<MarkFoodPrepared, TabAggregate>,
        IHandleCommand<MarkFoodServed, TabAggregate>,
        IHandleCommand<CloseTab, TabAggregate>
    {
        public IEnumerable Handle(Func<Guid, TabAggregate> al, OpenTab c)
        {
            yield return new TabOpened
            {
                Id = c.Id,
                TableNumber = c.TableNumber,
                Waiter = c.Waiter
            };
        }

        public IEnumerable Handle(Func<Guid, TabAggregate> al, PlaceOrder c)
        {
            var tab = al(c.Id);

            if (!tab.Open)
                throw new TabNotOpen();

            var drink = c.Items.Where(i => i.IsDrink).ToList();
            if (drink.Any())
                yield return new DrinksOrdered
                {
                    Id = c.Id,
                    Items = drink
                };

            var food = c.Items.Where(i => !i.IsDrink).ToList();
            if (food.Any())
                yield return new FoodOrdered
                {
                    Id = c.Id,
                    Items = food
                };
        }

        public IEnumerable Handle(Func<Guid, TabAggregate> al, MarkDrinksServed c)
        {
            var tab = al(c.Id);

            if (!tab.AreDrinksOutstanding(c.MenuNumbers))
                throw new DrinksNotOutstanding();

            yield return new DrinksServed
            {
                Id = c.Id,
                MenuNumbers = c.MenuNumbers
            };
        }

        public IEnumerable Handle(Func<Guid, TabAggregate> al, MarkFoodPrepared c)
        {
            var tab = al(c.Id);

            if (!tab.IsFoodOutstanding(c.MenuNumbers))
                throw new FoodNotOutstanding();

            yield return new FoodPrepared
            {
                Id = c.Id,
                MenuNumbers = c.MenuNumbers
            };
        }

        public IEnumerable Handle(Func<Guid, TabAggregate> al, MarkFoodServed c)
        {
            var tab = al(c.Id);

            if (!tab.IsFoodPrepared(c.MenuNumbers))
                throw new FoodNotPrepared();

            yield return new FoodServed
            {
                Id = c.Id,
                MenuNumbers = c.MenuNumbers
            };
        }

        public IEnumerable Handle(Func<Guid, TabAggregate> al, CloseTab c)
        {
            var tab = al(c.Id);

            if (!tab.Open)
                throw new TabNotOpen();
            if (tab.HasUnservedItems())
                throw new TabHasUnservedItems();
            if (c.AmountPaid < tab.ServedItemsValue)
                throw new MustPayEnough();

            yield return new TabClosed
            {
                Id = c.Id,
                AmountPaid = c.AmountPaid,
                OrderValue = tab.ServedItemsValue,
                TipValue = c.AmountPaid - tab.ServedItemsValue
            };
        }
    }
}
