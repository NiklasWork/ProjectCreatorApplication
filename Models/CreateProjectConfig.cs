namespace ProjectCreatorApplication.Models
{
    public class CreateProjectConfig
    {
        public string? Type { get; set; } = "console";
        public string? Name { get; set; }
        public string? Template { get; set; } = "HTTP trigger";
        public string? Version { get; set; }
        public string? Authorization { get; set; } = "Function";
        public string? Framework { get; set; } = "net6.0";
    }
}
