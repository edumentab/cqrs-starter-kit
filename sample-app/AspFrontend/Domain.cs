using Edument.CQRS;
using CafeReadModels;
using Cafe.Tab;

namespace AspFrontend
{
    public static class Domain
    {
        public static MessageDispatcher? Dispatcher;
        public static IOpenTabQueries? OpenTabQueries;
        public static IChefTodoListQueries? ChefTodoListQueries;

        public static void Setup()
        {
            if (Dispatcher != null) return;
            Dispatcher = new MessageDispatcher(new InMemoryEventStore());
            
            Dispatcher.ScanInstance(new TabAggregate());

            OpenTabQueries = new OpenTabs();
            Dispatcher.ScanInstance(OpenTabQueries);

            ChefTodoListQueries = new ChefTodoList();
            Dispatcher.ScanInstance(ChefTodoListQueries);
        }
    }
}