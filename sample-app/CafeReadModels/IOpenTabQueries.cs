using System;
using System.Collections.Generic;

namespace CafeReadModels
{
    public interface IOpenTabQueries
    {
        List<int> ActiveTableNumbers();
        OpenTabs.TabInvoice InvoiceForTable(int table);
        Guid TabIdForTable(int table);
        OpenTabs.TabStatus TabForTable(int table);
        Dictionary<int, List<OpenTabs.TabItem>> TodoListForWaiter(string waiter);
    }
}
