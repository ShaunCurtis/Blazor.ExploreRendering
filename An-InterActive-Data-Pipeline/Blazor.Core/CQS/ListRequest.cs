namespace Blazor.Core.CQS;

public record ListRequest
{
    public int StartIndex { get; init; }
    public int PageSize { get; init; }

    public ListRequest()
    {
        StartIndex = 0;
        PageSize = 1000;
    }

    public static ListRequest Create(int startIndex, int pageSize)
        => new() { StartIndex = startIndex, PageSize= pageSize };
}
