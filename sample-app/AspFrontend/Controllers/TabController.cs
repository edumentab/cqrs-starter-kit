using Cafe.Tab;
using System.Text.RegularExpressions;
using AspFrontend.ActionFilters;
using AspFrontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspFrontend.Controllers
{
    [IncludeLayoutData]
    public class TabController : Controller
    {
        public ActionResult Open()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Open(OpenTab cmd)
        {
            cmd.Id = Guid.NewGuid();
            Domain.Dispatcher!.SendCommand(cmd);
            return RedirectToAction("Order", new { id = cmd.TableNumber });
        }
        
        public ActionResult Close(int id) {
            return View(new CloseModel 
            {
                Id = id
            });
        }

        [HttpPost]
        public ActionResult Close(CloseModel model) {
            Domain.Dispatcher!.SendCommand(new CloseTab {
                 Id = Domain.OpenTabQueries!.TabIdForTable(model.Id),
                 AmountPaid = model.AmountPaid
            });
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Status(int id)
        {
            return View(Domain.OpenTabQueries!.TabForTable(id));
        }

        public ActionResult Order(int id)
        {
            return View(new OrderModel
            {
                Items = (from item in StaticData.Menu
                         select new OrderModel.OrderItem
                         {
                             MenuNumber = item.MenuNumber,
                             Description = item.Description,
                             NumberToOrder = 0
                         }).ToList(),
            });
        }

        [HttpPost]
        public ActionResult Order(int id, OrderModel order)
        {
            var items = new List<Events.Cafe.OrderedItem>();
            var menuLookup = StaticData.Menu.ToDictionary(k => k.MenuNumber, v => v);
            foreach (var item in order.Items)
                for (int i = 0; i < item.NumberToOrder; i++)
                    items.Add(new Events.Cafe.OrderedItem
                    {
                        MenuNumber = item.MenuNumber,
                        Description = menuLookup[item.MenuNumber].Description,
                        Price = menuLookup[item.MenuNumber].Price,
                        IsDrink = menuLookup[item.MenuNumber].IsDrink
                    });
            
            Domain.Dispatcher!.SendCommand(new PlaceOrder
            {
                Id = Domain.OpenTabQueries!.TabIdForTable(id),
                Items = items
            });

            return RedirectToAction("Status", new { id = id });
        }


        public ActionResult MarkServed(int id, IFormCollection form)
        {
            var tabId = Domain.OpenTabQueries!.TabIdForTable(id);
            var menuNumbers = (from entry in form.Keys.Cast<string>()
                               where form[entry] != "false"
                               let m = Regex.Match(entry, @"served_\d+_(\d+)")
                               where m.Success
                               select int.Parse(m.Groups[1].Value)
                              ).ToList();

            var menuLookup = StaticData.Menu.ToDictionary(k => k.MenuNumber, v => v);
            
            var drinks = menuNumbers.Where(n => menuLookup[n].IsDrink).ToList();
            if (drinks.Any())
                Domain.Dispatcher!.SendCommand(new MarkDrinksServed
                {
                    Id = tabId,
                    MenuNumbers = drinks
                });

            var food = menuNumbers.Where(n => !menuLookup[n].IsDrink).ToList();
            if (food.Any())
                Domain.Dispatcher!.SendCommand(new MarkFoodServed
                {
                    Id = tabId,
                    MenuNumbers = food
                });

            return RedirectToAction("Status", new { id = id });
        }

    }
}
