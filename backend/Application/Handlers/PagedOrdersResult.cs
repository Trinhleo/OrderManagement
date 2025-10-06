namespace OrderManagement.Application.Handlers
{
    public class PagedOrdersResult
    {
        public IEnumerable<object> Orders { get; set; } = Enumerable.Empty<object>();
        public int TotalCount { get; set; }
    }
}
