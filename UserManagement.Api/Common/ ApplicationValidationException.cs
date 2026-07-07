namespace UserManagement.Api.Common
{
    public class ApplicationValidationException(
        string message,
        List<string>? errors = null) : Exception(message)
    {
        public List<string> Errors { get; } = errors ?? [];
    }
}