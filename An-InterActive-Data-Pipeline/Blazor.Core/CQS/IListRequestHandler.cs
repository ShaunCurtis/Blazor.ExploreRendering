namespace Blazor.Core.CQS;

public interface IListRequestHandler<TItem>
{
    public ValueTask<ListResult<TItem>> Execute(ListRequest request);
}
