namespace Blazor.Core.CQS;

public record ListResult<TItem>
{
    public IEnumerable<TItem>? Items { get; init; }
    public bool Successful { get; init; }
    public string? Message { get; init; }
    public int TotalCount { get; init; }

    public ListResult()
    {
        Successful = false;
    }

    public static ListResult<TItem> Success(IEnumerable<TItem> items, int totalCount)
        => new() { Items = items, Successful = true, TotalCount = totalCount };

    public static ListResult<TItem> Failure(string message)
        => new() { Message = message };
}
