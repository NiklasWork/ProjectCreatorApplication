namespace ProjectCreatorApplication.Repositorys
{
    public class CustomResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? OptinalMessage { get; set; }

        public CustomResult(bool success, string message, string? optionalMessage)
        {
            Success = success;
            Message = message;
            OptinalMessage = optionalMessage;
        }
    }
}

