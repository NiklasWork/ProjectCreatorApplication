namespace ProjectCreatorApplication.Models
{
    public class CreateProjectConfig
    {
        public string? Type { get; set; }
        public string? ProjectName { get; set; }
        public string? FunctionName { get; set; } = "NewFunc";
        public string? Template { get; set; } = "HTTP trigger";
        public string? Authorization { get; set; } = "function";
        public string? Framework { get; set; } = "net6.0";
        public string? WorkerRuntime { get; set; } = "dotnet";
        public string? Language { get; set; } = "c#";
    }
}
