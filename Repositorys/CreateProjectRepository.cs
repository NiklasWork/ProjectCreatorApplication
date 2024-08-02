using ProjectCreatorApplication.Interfaces;
using System.Diagnostics;
using System.IO.Compression;

namespace ProjectCreatorApplication.Repositorys
{
    public class CreateProjectRepository : ICreateProjectRepository
    {
        private static readonly string BaseDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "AutomaticCreatedProjects"));
        //private static readonly string DownloadsDirectory = Path.Combine("/root/Downloads");

        public CustomResult CreateProject(string? projectName)
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
                return new CustomResult(false, "Error creating project: " + result.Error, "");
            }

            result = ExecuteCommand("dotnet", $"restore {projectDirectory}");

            if (result.ExitCode != 0)
            {
                return new CustomResult(false, "Error restoring project: " + result.Error, "");
            }
            
            return new CustomResult(true, $"Project '{projectName}' created in '{projectDirectory}'.", "");
        }

        public CustomResult CreateZipFile()
        {
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "AutomaticCreatedProjects"));
            var projectPath = Directory.GetDirectories(basePath).FirstOrDefault(x => !x.EndsWith(".zip"));
            var projectName = Path.GetFileName(projectPath);

            if (projectPath == null)
            {
                return new CustomResult(false, "There is no project to Download.", "");
            }

            var zipFilePath = Path.Combine(basePath, $"{projectName}.zip");

            if (System.IO.File.Exists(zipFilePath))
            {
                System.IO.File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(projectPath, zipFilePath);
            
            return new CustomResult(true, zipFilePath, projectName);
        }

        //public CustomResult CopyProject()
        //{
        //    try
        //    {
        //        if (!Directory.Exists(BaseDirectory))
        //        {
        //            return new CustomResult(false, "No projects found. The directory 'AutomaticCreatedProjects' does not exist.");
        //        }

        //        var directories = Directory.GetDirectories(BaseDirectory);
        //        var projectDirectory = directories.FirstOrDefault();

        //        if (string.IsNullOrEmpty(projectDirectory))
        //        {
        //            return new CustomResult(false, "No project directories found within 'AutomaticCreatedProjects'.");
        //        }

        //        var projectDirectoryName = Path.GetFileName(projectDirectory);
        //        var destinationPath = Path.Combine(DownloadsDirectory, projectDirectoryName);

        //        var parentDirectory = Path.GetDirectoryName(destinationPath);
        //        if (!Directory.Exists(parentDirectory))
        //        {
        //            Directory.CreateDirectory(parentDirectory);
        //        }

        //        var command = $"cp -r \"{projectDirectory}\" \"{destinationPath}\"";

        //        var (ExitCode, Output, Error) = ExecuteCommand("sh", $"-c \"{command}\"");
        //        if (ExitCode != 0)
        //        {
        //            return new CustomResult(false, "Error copying project: " + Error);
        //        }

        //        return new CustomResult(true, $"Project folder copied to '{destinationPath}'.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return new CustomResult(false, "An error occurred: " + ex.Message);
        //    }
        //}

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
    }
}
