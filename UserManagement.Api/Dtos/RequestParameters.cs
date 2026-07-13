namespace UserManagement.Api.Dtos
{
    public abstract class RequestParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
        /// <summary>
        /// Comma-separated dynamic filters (e.g. "Email==test@example.com,Id>5").
        /// </summary>
        public string? Filters { get; set; }
    }
}
