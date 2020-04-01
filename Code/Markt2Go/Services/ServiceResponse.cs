namespace Markt2Go.Services
{
    public class ServiceResponse
    {
        public bool Success { get; set; } = true;

        public string Message { get; set; } = null;
        public string Exception { get; set; } = null;
    }
    public class ServiceResponse<T>
    {
        public T Data { get; set; }

        public bool Success { get; set; } = true;

        public string Message { get; set; } = null;
        public string Exception { get; set; } = null;
    }
}