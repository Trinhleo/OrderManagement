namespace OrderManagement.Application.Handlers
{
    public class PagedOrdersResult
    {
        public object Orders { get; set; }
        public int TotalCount { get; set; }
    }
}
