using System.Collections.Generic;

namespace OrderManagement.Application.Queries
{
    public class ListOrdersQuery
    {
        public int Page { get; }
        public int PageSize { get; }
        public string? SortBy { get; }
        public bool Desc { get; }

        public ListOrdersQuery(int page, int pageSize, string? sortBy = null, bool desc = true)
        {
            Page = page;
            PageSize = pageSize;
            SortBy = sortBy;
            Desc = desc;
        }
    }

    public class PagedOrdersResult
    {
        public IEnumerable<object> Orders { get; set; } = new List<object>();
        public int TotalCount { get; set; }
    }
}
