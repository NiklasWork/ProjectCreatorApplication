namespace ProjectCreatorApplication.Repositorys
{
    public class CustomResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public CustomResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}

