using System;
using System.Collections.Generic;

namespace CafeReadModels
{
    public interface IChefTodoListQueries
    {
        List<ChefTodoList.TodoListGroup> GetTodoList();
    }
}
