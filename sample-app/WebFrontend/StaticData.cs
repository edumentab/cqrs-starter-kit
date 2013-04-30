using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebFrontend
{
    public static class StaticData
    {
        public class MenuItem
        {
            public int MenuNumber;
            public string Description;
            public decimal Price;
            public bool IsDrink;
        }

        public static List<MenuItem> Menu = new List<MenuItem>
        {
            new MenuItem
            {
                MenuNumber = 1, Description = "Coke", Price = 1.50M, IsDrink = true
            },
            new MenuItem
            {
                MenuNumber = 2, Description = "Green Tea", Price = 1.90M, IsDrink = true
            },
            new MenuItem
            {
                MenuNumber = 3, Description = "Freshly Ground Coffee", Price = 2.00M, IsDrink = true
            },
            new MenuItem
            {
                MenuNumber = 4, Description = "Czech Pilsner", Price = 3.50M, IsDrink = true
            },
            new MenuItem
            {
                MenuNumber = 5, Description = "Yeti Stout", Price = 4.50M, IsDrink = true
            },
            new MenuItem
            {
                MenuNumber = 10, Description = "Mushroom & Bacon Pasta", Price = 6.00M
            },
            new MenuItem
            {
                MenuNumber = 11, Description = "Chili Con Carne", Price = 7.50M
            },
            new MenuItem
            {
                MenuNumber = 12, Description = "Borsch With Smetana", Price = 4.50M
            },
            new MenuItem
            {
                MenuNumber = 13, Description = "Lamb Skewers with Tatziki", Price = 8.00M
            },
            new MenuItem
            {
                MenuNumber = 14, Description = "Beef Stroganoff", Price = 8.50M
            },
        };

        public static List<string> WaitStaff = new List<string>
        {
            "Jack", "Lena", "Pedro", "Anastasia"
        };
    }
}