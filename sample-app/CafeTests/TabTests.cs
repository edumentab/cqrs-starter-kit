using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edument.CQRS;
using NUnit.Framework;
using Cafe.Tab;
using Events.Cafe;

namespace CafeTests
{
    [TestFixture]
    public class TabTests : BDDTest<TabCommandHandlers, TabAggregate>
    {
        private Guid testId;
        private int testTable;
        private string testWaiter;
        private OrderedItem testDrink1;
        private OrderedItem testDrink2;
        private OrderedItem testFood1;
        private OrderedItem testFood2;

        [SetUp]
        public void Setup()
        {
            testId = Guid.NewGuid();
            testTable = 42;
            testWaiter = "Derek";

            testDrink1 = new OrderedItem
            {
                MenuNumber = 4,
                Description = "Sprite",
                Price = 1.50M,
                IsDrink = true
            };
            testDrink2 = new OrderedItem
            {
                MenuNumber = 10,
                Description = "Beer",
                Price = 2.50M,
                IsDrink = true
            };

            testFood1 = new OrderedItem
            {
                MenuNumber = 16,
                Description = "Beef Noodles",
                Price = 7.50M,
                IsDrink = false
            };
            testFood2 = new OrderedItem
            {
                MenuNumber = 25,
                Description = "Vegetable Curry",
                Price = 6.00M,
                IsDrink = false
            };
        }

        [Test]
        public void CanOpenANewTab()
        {
            Test(
                Given(),
                When(new OpenTab
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                Then(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                }));
        }

        [Test]
        public void CanNotOrderWithUnopenedTab()
        {
            Test(
                Given(),
                When(new PlaceOrder
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink1 }
                }),
                ThenFailWith<TabNotOpen>());
        }

        [Test]
        public void CanPlaceDrinksOrder()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                When(new PlaceOrder
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink1, testDrink2 }
                }),
                Then(new DrinksOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink1, testDrink2 }
                }));
        }

        [Test]
        public void CanPlaceFoodOrder()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                When(new PlaceOrder
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1, testFood1 }
                }),
                Then(new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1, testFood1 }
                }));
        }

        [Test]
        public void CanPlaceFoodAndDrinkOrder()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                When(new PlaceOrder
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1, testDrink2 }
                }),
                Then(new DrinksOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink2 }
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1 }
                }));
        }

        [Test]
        public void OrderedDrinksCanBeServed()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new DrinksOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink1, testDrink2 }
                }),
                When(new MarkDrinksServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testDrink1.MenuNumber, testDrink2.MenuNumber }
                }),
                Then(new DrinksServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testDrink1.MenuNumber, testDrink2.MenuNumber }
                }));
        }

        [Test]
        public void CanNotServeAnUnorderedDrink()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new DrinksOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink1 }
                }),
                When(new MarkDrinksServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testDrink2.MenuNumber }
                }),
                ThenFailWith<DrinksNotOutstanding>());
        }

        [Test]
        public void CanNotServeAnOrderedDrinkTwice()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new DrinksOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink1 }
                },
                new DrinksServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testDrink1.MenuNumber }
                }),
                When(new MarkDrinksServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testDrink1.MenuNumber }
                }),
                ThenFailWith<DrinksNotOutstanding>());
        }

        [Test]
        public void OrderedFoodCanBeMarkedPrepared()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1, testFood1 }
                }),
                When(new MarkFoodPrepared
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood1.MenuNumber, testFood1.MenuNumber }
                }),
                Then(new FoodPrepared
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood1.MenuNumber, testFood1.MenuNumber }
                }));
        }

        [Test]
        public void FoodNotOrderedCanNotBeMarkedPrepared()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                When(new MarkFoodPrepared
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood2.MenuNumber }
                }),
                ThenFailWith<FoodNotOutstanding>());
        }

        [Test]
        public void CanNotMarkFoodAsPreparedTwice()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1, testFood1 }
                },
                new FoodPrepared
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood1.MenuNumber, testFood1.MenuNumber }
                }),
                When(new MarkFoodPrepared
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood1.MenuNumber }
                }),
                ThenFailWith<FoodNotOutstanding>());
        }

        [Test]
        public void CanServePreparedFood()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1, testFood2 }
                },
                new FoodPrepared
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood1.MenuNumber, testFood2.MenuNumber }
                }),
                When(new MarkFoodServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood2.MenuNumber, testFood1.MenuNumber }
                }),
                Then(new FoodServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood2.MenuNumber, testFood1.MenuNumber }
                }));
        }

        [Test]
        public void CanNotServePreparedFoodTwice()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1, testFood2 }
                },
                new FoodPrepared
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood1.MenuNumber, testFood2.MenuNumber }
                },
                new FoodServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood2.MenuNumber, testFood1.MenuNumber }
                }),
                When(new MarkFoodServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood2.MenuNumber, testFood1.MenuNumber }
                }),
                ThenFailWith<FoodNotPrepared>());
        }

        [Test]
        public void CanNotServeUnorderedFood()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1 }
                }),
                When(new MarkFoodServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood2.MenuNumber }
                }),
                ThenFailWith<FoodNotPrepared>());
        }

        [Test]
        public void CanNotServeOrderedButUnpreparedFood()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1 }
                }),
                When(new MarkFoodServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood1.MenuNumber }
                }),
                ThenFailWith<FoodNotPrepared>());
        }

        [Test]
        public void CanCloseTabByPayingExactAmount()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1, testFood2 }
                },
                new FoodPrepared
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood1.MenuNumber, testFood2.MenuNumber }
                },
                new FoodServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood2.MenuNumber, testFood1.MenuNumber }
                }),
                When(new CloseTab
                {
                    Id = testId,
                    AmountPaid = testFood1.Price + testFood2.Price
                }),
                Then(new TabClosed
                {
                    Id = testId,
                    AmountPaid = testFood1.Price + testFood2.Price,
                    OrderValue = testFood1.Price + testFood2.Price,
                    TipValue = 0.00M
                }));
        }

        [Test]
        public void CanCloseTabWithTip()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new DrinksOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink2 }
                },
                new DrinksServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testDrink2.MenuNumber }
                }),
                When(new CloseTab
                {
                    Id = testId,
                    AmountPaid = testDrink2.Price + 0.50M
                }),
                Then(new TabClosed
                {
                    Id = testId,
                    AmountPaid = testDrink2.Price + 0.50M,
                    OrderValue = testDrink2.Price,
                    TipValue = 0.50M
                }));
        }

        [Test]
        public void MustPayEnoughToCloseTab()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new DrinksOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink2 }
                },
                new DrinksServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testDrink2.MenuNumber }
                }),
                When(new CloseTab
                {
                    Id = testId,
                    AmountPaid = testDrink2.Price - 0.50M
                }),
                ThenFailWith<MustPayEnough>());
        }

        [Test]
        public void CanNotCloseTabTwice()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new DrinksOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink2 }
                },
                new DrinksServed
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testDrink2.MenuNumber }
                },
                new TabClosed
                {
                    Id = testId,
                    AmountPaid = testDrink2.Price + 0.50M,
                    OrderValue = testDrink2.Price,
                    TipValue = 0.50M
                }),
                When(new CloseTab
                {
                    Id = testId,
                    AmountPaid = testDrink2.Price
                }),
                ThenFailWith<TabNotOpen>());
        }

        [Test]
        public void CanNotCloseTabWithUnservedDrinksItems()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new DrinksOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testDrink2 }
                }),
                When(new CloseTab
                {
                    Id = testId,
                    AmountPaid = testDrink2.Price
                }),
                ThenFailWith<TabHasUnservedItems>());
        }

        [Test]
        public void CanNotCloseTabWithUnpreparedFoodItems()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1 }
                }),
                When(new CloseTab
                {
                    Id = testId,
                    AmountPaid = testFood1.Price
                }),
                ThenFailWith<TabHasUnservedItems>());
        }

        [Test]
        public void CanNotCloseTabWithUnservedFoodItems()
        {
            Test(
                Given(new TabOpened
                {
                    Id = testId,
                    TableNumber = testTable,
                    Waiter = testWaiter
                },
                new FoodOrdered
                {
                    Id = testId,
                    Items = new List<OrderedItem> { testFood1 }
                },
                new FoodPrepared
                {
                    Id = testId,
                    MenuNumbers = new List<int> { testFood1.MenuNumber }
                }),
                When(new CloseTab
                {
                    Id = testId,
                    AmountPaid = testFood1.Price
                }),
                ThenFailWith<TabHasUnservedItems>());
        }
    }
}
