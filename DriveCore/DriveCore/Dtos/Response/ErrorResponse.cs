namespace DriveCore.Dtos.Response
{
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();
    }
}
