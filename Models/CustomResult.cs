namespace ProjectCreatorApplication.Models
{
    public class CustomResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? OptionalMessage { get; set; }
        public Byte[]? Data { get; set; }

        public CustomResult(bool success, string message, string? optionalMessage)
        {
            Success = success;
            Message = message;
            OptionalMessage = optionalMessage;
        }

        public CustomResult(bool success, string message, string? optionalMessage, Byte[]? data)
        {
            Success = success;
            Message = message;
            OptionalMessage = optionalMessage;
            Data = data;
        }
    }
}
