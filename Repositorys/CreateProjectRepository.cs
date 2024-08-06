using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ProjectCreatorApplication.Interfaces;
using ProjectCreatorApplication.Models;

namespace ProjectCreatorApplication.Repository
{
    public class CreateProjectRepository : ICreateProjectRepository
    {
        private static readonly string basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "AutomaticCreatedProject"));

        public CustomResult CreateProject(CreateProjectConfig projectConfig)
        {
            if (string.IsNullOrWhiteSpace(projectConfig.Type))
            {
                return new CustomResult(false, "Error: No project type was given.", "");
            }

            if (string.IsNullOrWhiteSpace(projectConfig.Name))
            {
                projectConfig.Name = "Project-Type-Of-" + projectConfig.Type;
            }

            if (Directory.Exists(basePath))
            {
                Directory.Delete(basePath, true);
            }

            Directory.CreateDirectory(basePath);
            string projectDirectory = Path.Combine(basePath, projectConfig.Name);
            Directory.CreateDirectory(projectDirectory);

            if (projectConfig.Type.Equals("func", StringComparison.OrdinalIgnoreCase))
            {
                return CreateFunctionApp(projectConfig, projectDirectory);
            }
            else
            {
                return CreateDotNetProject(projectConfig, projectDirectory);
            }
        }

        private static CustomResult CreateDotNetProject(CreateProjectConfig projectConfig, string projectDirectory)
        {
            var dotnetNewArguments = $"new {projectConfig.Type} --output {projectDirectory}";
            var result = ExecuteCommand("dotnet", dotnetNewArguments);
            if (result.ExitCode != 0)
            {
                return new CustomResult(false, "Error creating project: " + result.Error, "");
            }

            var csprojFilePath = Directory.GetFiles(projectDirectory, "*.csproj").FirstOrDefault();
            if (csprojFilePath == null)
            {
                return new CustomResult(false, "Error: .csproj file not found.", "");
            }

            var framework = !string.IsNullOrWhiteSpace(projectConfig.Framework) ? projectConfig.Framework : "net6.0";
            var csprojContent = File.ReadAllText(csprojFilePath);

            csprojContent = csprojContent.Replace("<TargetFramework>netcoreapp3.1</TargetFramework>", $"<TargetFramework>{framework}</TargetFramework>");
            File.WriteAllText(csprojFilePath, csprojContent);

            var restoreResult = ExecuteCommand("dotnet", $"restore {csprojFilePath}");
            if (restoreResult.ExitCode != 0)
            {
                return new CustomResult(false, "Error restoring project: " + restoreResult.Error, "");
            }

            return new CustomResult(true, $"Project '{projectConfig.Name}' created in '{projectDirectory}'.", "");
        }

        private static CustomResult CreateFunctionApp(CreateProjectConfig projectConfig, string projectDirectory)
        {
            var (exitCode, output, error) = ExecuteCommand("which", "func");
            if (exitCode != 0)
            {
                return new CustomResult(false, $"Azure Functions Core Tools 'func' not found: {error}", "");
            }

            if (!Directory.Exists(projectDirectory))
            {
                Directory.CreateDirectory(projectDirectory);
            }

            var oldWorkingDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(projectDirectory);

            try
            {
                string initArguments = "--dotnet";
                if (!string.IsNullOrWhiteSpace(projectConfig.Version))
                {
                    initArguments += $" --version {projectConfig.Version}";
                }
                if (!string.IsNullOrWhiteSpace(projectConfig.Authorization))
                {
                    initArguments += $" --auth {projectConfig.Authorization}";
                }

                var initResult = ExecuteCommand("func", $"init {initArguments}", 120000);
                if (initResult.ExitCode != 0)
                {
                    return new CustomResult(false, $"Error initializing Function App: {initResult.Error}", "");
                }

                var functionResult = ExecuteCommand("func", $"new --template \"{projectConfig.Template}\" --name {projectConfig.Name} --verbose", 120000);
                if (functionResult.ExitCode != 0)
                {
                    return new CustomResult(false, $"Error creating function: {functionResult.Error}", "");
                }

                return new CustomResult(true, $"Function '{projectConfig.Name}' created in '{projectDirectory}'.", $"Output: {functionResult.Output}\nError: {functionResult.Error}");
            }
            finally
            {
                Directory.SetCurrentDirectory(oldWorkingDirectory);
            }
        }

        private static (int ExitCode, string Output, string Error) ExecuteCommand(string command, string arguments, int timeout = 60000)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = command;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    if (!process.WaitForExit(timeout))
                    {
                        process.Kill();
                        process.WaitForExit();
                        return (-1, string.Empty, $"Command '{command} {arguments}' timed out.");
                    }

                    string output = outputTask.Result;
                    string error = errorTask.Result;

                    return (process.ExitCode, output, error);
                }
            }
            catch (Exception ex)
            {
                return (-1, string.Empty, $"Exception: {ex.Message}");
            }
        }

        public CustomResult CreateZipFile()
        {
            try
            {
                var projectPath = Directory.GetDirectories(basePath).FirstOrDefault(x => !x.EndsWith(".zip"));
                if (projectPath == null)
                {
                    return new CustomResult(false, "", "There is no project to download.");
                }
                var projectName = Path.GetFileName(projectPath);

                using var memoryStream = new MemoryStream();
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    AddDirectoryToZip(zipArchive, projectPath, projectPath);
                }

                Directory.Delete(projectPath, true);

                memoryStream.Position = 0;
                var byteArray = memoryStream.ToArray();

                return new CustomResult(true, "Zip file created successfully", projectName, byteArray);
            }
            catch (Exception ex)
            {
                return new CustomResult(false, $"Error creating ZIP file: {ex.Message}", null);
            }
        }

        private static void AddDirectoryToZip(ZipArchive zipArchive, string sourceDir, string baseDir)
        {
            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var entryName = Path.GetRelativePath(baseDir, file);
                var entry = zipArchive.CreateEntry(entryName);

                using var entryStream = entry.Open();
                using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                fileStream.CopyTo(entryStream);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                var entryName = Path.GetRelativePath(baseDir, directory) + Path.DirectorySeparatorChar;
                zipArchive.CreateEntry(entryName);
            }
        }
    }
}