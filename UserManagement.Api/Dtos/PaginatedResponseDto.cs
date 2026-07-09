namespace UserManagement.Api.Dtos
{
    public class PaginatedResponseDto<T>
    {
        public IEnumerable<T> Items { get; set; } = null!;
        public MetaData MetaData { get; set; } = null!;
    }
}
