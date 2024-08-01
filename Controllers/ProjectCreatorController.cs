using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ProjectCreatorApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectCreatorController : ControllerBase
    {
        private static readonly string BaseDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "AutomaticCreatedProjects"));
        private static readonly string DownloadsDirectory = Path.Combine("/root/Downloads");

        [HttpPost("CreateProject")]
        public IActionResult CreateNewProject([FromQuery] string? projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                projectName = "NewConsoleApp";
            }

            if (Directory.Exists(BaseDirectory))
            {
                Directory.Delete(BaseDirectory, true);
            }

            Directory.CreateDirectory(BaseDirectory);

            string projectDirectory = Path.Combine(BaseDirectory, projectName);

            Directory.CreateDirectory(projectDirectory);

            var result = ExecuteCommand("dotnet", $"new console --output {projectDirectory}");

            if (result.ExitCode != 0)
            {
                return StatusCode(500, "Error creating project: " + result.Error);
            }

            result = ExecuteCommand("dotnet", $"restore {projectDirectory}");

            if (result.ExitCode != 0)
            {
                return StatusCode(500, "Error restoring project: " + result.Error);
            }

            return Ok($"Project '{projectName}' created in '{projectDirectory}'.");
        }

        [HttpGet("CopyProject")]
        public IActionResult CopyProject()
        {
            try
            {
                // Ensure the base directory exists
                if (!Directory.Exists(BaseDirectory))
                {
                    return NotFound("No projects found. The directory 'AutomaticCreatedProjects' does not exist.");
                }

                // Get the first project directory
                var directories = Directory.GetDirectories(BaseDirectory);
                var projectDirectory = directories.FirstOrDefault();

                if (string.IsNullOrEmpty(projectDirectory))
                {
                    return NotFound("No project directories found within 'AutomaticCreatedProjects'.");
                }

                // Prepare paths
                var projectDirectoryName = Path.GetFileName(projectDirectory);
                var destinationPath = Path.Combine(DownloadsDirectory, projectDirectoryName);

                // Ensure the parent directory exists
                var parentDirectory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(parentDirectory))
                {
                    // Create the parent directory if it does not exist
                    Directory.CreateDirectory(parentDirectory);
                }

                // Construct the cp command to copy the entire projectDirectory to the destinationPath
                var command = $"cp -r \"{projectDirectory}\" \"{destinationPath}\"";

                // Execute the command
                var (ExitCode, Output, Error) = ExecuteCommand("sh", $"-c \"{command}\"");
                if (ExitCode != 0)
                {
                    return StatusCode(500, "Error copying project: " + Error);
                }

                return Ok($"Project folder copied to '{destinationPath}'.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }


        private static (int ExitCode, string Output, string Error) ExecuteCommand(string command, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                return (1, string.Empty, "Failed to start process.");
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            return (process.ExitCode, output, error);
        }

        [HttpGet("TestApi")]
        public IActionResult TestApi()
        {
            return Ok($"Basedirection: {BaseDirectory}, DownloadsDirectory: {DownloadsDirectory}");
        }
    }
}
