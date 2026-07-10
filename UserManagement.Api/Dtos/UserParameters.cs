namespace UserManagement.Api.Dtos
{
    public class UserParameters : RequestParameters
    {
        public string? SearchTerm { get; set; }
        public string? OrderBy { get; set; }
    }
}
