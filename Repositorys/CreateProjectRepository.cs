using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectCreatorApplication.Interfaces;
using ProjectCreatorApplication.Models;

namespace ProjectCreatorApplication.Repository
{
    public class CreateProjectRepository : ICreateProjectRepository
    {
        private static readonly string basePath = "/app/AutomaticCreatedProject";
        private static readonly string[] sourceArrayWorkerRuntime = ["dotnet", "node", "python", "powershell", "custom"];
        private static readonly string[] sourceArrayAuth = ["function", "anonymous", "admin"];

        public CustomResult CreateProject(CreateProjectConfig projectConfig)
        {
            Directory.SetCurrentDirectory("/app");
            if (Directory.Exists(basePath))
            {
                Directory.Delete(basePath, true);
            }
            Directory.CreateDirectory(basePath);

            if (string.IsNullOrWhiteSpace(projectConfig.Type))
            {
                return new CustomResult(false, "Error: No project type was given.", "");
            }

            if (string.IsNullOrWhiteSpace(projectConfig.ProjectName))
            {
                projectConfig.ProjectName = "Project-Type-Of-" + projectConfig.Type;
            }

            try
            {
                if (projectConfig.Type.Equals("func", StringComparison.OrdinalIgnoreCase))
                {
                    return CreateFunctionApp(projectConfig, basePath);
                }
                else
                {
                    string projectDirectory = Path.Combine(basePath, projectConfig.ProjectName);
                    Directory.CreateDirectory(projectDirectory);
                    return CreateDotNetProject(projectConfig, projectDirectory);
                }
            }
            catch (Exception ex)
            {
                return new CustomResult(false, $"Exception during project creation: {ex.Message}", "");
            }
        }

        private static CustomResult CreateDotNetProject(CreateProjectConfig projectConfig, string projectDirectory)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectDirectory))
                {
                    return new CustomResult(false, "Error: Invalid project directory path.", "");
                }

                var createArguments = $"new {projectConfig.Type} --output {projectDirectory}";
                var (createExitCode, createOutput, createError) = ExecuteCommand("dotnet", createArguments);

                if (createExitCode != 0)
                {
                    return new CustomResult(false, "Error creating project: " + createError, "");
                }

                if (!string.IsNullOrWhiteSpace(projectConfig.Framework))
                {
                    var frameworkVersion = ExtractVersionNumber(projectConfig.Framework);

                    if (frameworkVersion < 8)
                    {
                        var csprojFilePath = Directory.GetFiles(projectDirectory, "*.csproj").FirstOrDefault();
                        if (csprojFilePath == null)
                        {
                            return new CustomResult(false, "Error: .csproj file not found.", "");
                        }

                        var updateResult = UpdateCsprojFramework(csprojFilePath, projectConfig.Framework);
                        if (!updateResult.Success)
                        {
                            return new CustomResult(false, "Error updating .csproj file: " + updateResult.Message, "");
                        }
                    }
                    else
                    {
                        createArguments = $"new {projectConfig.Type} --framework {projectConfig.Framework} --output {projectDirectory}";
                        var (updateExitCode, updateOutput, updateError) = ExecuteCommand("dotnet", createArguments);
                        if (updateExitCode != 0)
                        {
                            return new CustomResult(false, "Error updating project with framework: " + updateError, "");
                        }
                    }
                }

                var restoreResult = ExecuteCommand("dotnet", $"restore {projectDirectory}");
                if (restoreResult.ExitCode != 0)
                {
                    return new CustomResult(false, "Error restoring project: " + restoreResult.Error, "");
                }

                return new CustomResult(true, $"Project '{projectConfig.ProjectName}' created in '{projectDirectory}'.", "");
            }
            catch (Exception ex)
            {
                return new CustomResult(false, $"Exception occurred: {ex.Message}", "");
            }
        }

        private static int ExtractVersionNumber(string framework)
        {
            var match = Regex.Match(framework, @"net(\d+)\.0");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var versionNumber))
            {
                return versionNumber;
            }
            return 0;
        }

        private static CustomResult UpdateCsprojFramework(string csprojFilePath, string targetFramework)
        {
            try
            {
                var csprojContent = File.ReadAllText(csprojFilePath);
                var updatedContent = Regex.Replace(
                    csprojContent,
                    @"<TargetFramework>.*</TargetFramework>",
                    $"<TargetFramework>{targetFramework}</TargetFramework>",
                    RegexOptions.IgnoreCase
                );

                File.WriteAllText(csprojFilePath, updatedContent);
                return new CustomResult(true, "Successfully updated .csproj file.", "");
            }
            catch (Exception ex)
            {
                return new CustomResult(false, $"Exception occurred while updating .csproj file: {ex.Message}", "");
            }
        }

        private static CustomResult CreateFunctionApp(CreateProjectConfig projectConfig, string projectDirectory)
        {
            var tempDirectory = Path.Combine(projectDirectory, projectConfig.ProjectName);
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }

            Directory.CreateDirectory(tempDirectory);

            Directory.SetCurrentDirectory(tempDirectory);

            if (!sourceArrayWorkerRuntime.Contains(projectConfig.WorkerRuntime))
            {
                return new CustomResult(false, $"Invalid worker runtime: {projectConfig.WorkerRuntime}", "");
            }

            if (projectConfig.WorkerRuntime == "dotnet" && projectConfig.Language != "c#")
            {
                return new CustomResult(false, $"Invalid language for dotnet runtime: {projectConfig.Language}", "");
            }

            if (!sourceArrayAuth.Contains(projectConfig.Authorization))
            {
                return new CustomResult(false, $"Invalid authorization level: {projectConfig.Authorization}", "");
            }

            var (funcCheckExitCode, funcCheckOutput, funcCheckError) = ExecuteCommand("which", "func");
            if (funcCheckExitCode != 0)
            {
                return new CustomResult(false, $"Azure Functions Core Tools 'func' not found: {funcCheckError}", "");
            }

            try
            {
                var initArguments = $"--worker-runtime {projectConfig.WorkerRuntime} --language {projectConfig.Language}";
                if (!string.IsNullOrWhiteSpace(projectConfig.Framework))
                {
                    var frameworkVersion = ExtractVersionNumber(projectConfig.Framework);
                    if (frameworkVersion >= 8)
                    {
                        initArguments += $" --target-framework {projectConfig.Framework}";
                    }
                }

                var initResult = ExecuteCommand("func", $"init {projectConfig.ProjectName} {initArguments}", 120000);
                if (initResult.ExitCode != 0)
                {
                    return new CustomResult(false, $"Error initializing Function App: {initResult.Error}", "");
                }

                Directory.SetCurrentDirectory(projectConfig.ProjectName);

                var createFunctionArguments = $"--name {projectConfig.FunctionName} --template \"{projectConfig.Template}\" --language {projectConfig.Language} --authlevel {projectConfig.Authorization}";
                var createResult = ExecuteCommand("func", $"new {createFunctionArguments}", 120000);
                if (createResult.ExitCode != 0)
                {
                    return new CustomResult(false, $"Error creating function: {createResult.Error}", "");
                }

                return new CustomResult(true, $"Function '{projectConfig.FunctionName}' created in '{projectConfig.ProjectName}'.", $"Output: {createResult.Output}\nError: {createResult.Error}");
            }
            catch (Exception ex)
            {
                return new CustomResult(false, $"Exception occurred: {ex.Message}", "");
            }
            finally
            {
                Directory.SetCurrentDirectory(projectDirectory);
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
