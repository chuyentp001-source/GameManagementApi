
namespace GameManagement.Application.DTOs;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public long TotalRecords { get; set; }

    public long TotalPages { get; set; }
}
