namespace Mechanics.Application.Generic;

public class ListResponse<T>(IEnumerable<T> items)
{
    public IEnumerable<T> Items { get; } = items;
}
