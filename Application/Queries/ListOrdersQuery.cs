using System.Collections.Generic;

namespace OrderManagement.Application.Queries
{
    public class ListOrdersQuery
    {
        public int Page { get; }
        public int PageSize { get; }

        public ListOrdersQuery(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
    }

    public class PagedOrdersResult
    {
        public IEnumerable<object> Orders { get; set; } = new List<object>();
        public int TotalCount { get; set; }
    }
}
