namespace DriveCore.Services
{
    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public List<string> Errors { get; private set; } = new();
        public T? Data { get; private set; }

        public static ServiceResult<T> Ok(T data, string message = "")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ServiceResult<T> Fail(string message, IEnumerable<string>? errors = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Errors = errors?.ToList() ?? new List<string>()
            };
        }
    }
}
