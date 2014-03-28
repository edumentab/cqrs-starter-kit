using System;
using System.Collections.Generic;

namespace CafeReadModels
{
    public interface IOpenTabQueries
    {
        List<int> ActiveTableNumbers();
        Guid TabIdForTable(int table);
        Dictionary<int, List<OpenTabs.TabItem>> TodoListForWaiter(string waiter);
    }
}
