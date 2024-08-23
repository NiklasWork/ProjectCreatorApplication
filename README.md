# ProjectCreatorApplication

## Overview

`ProjectCreatorApplication` is a .NET-based application that provides endpoints for creating and downloading .NET projects and Azure Function Apps. This application exposes APIs that allow users to specify configuration settings for project creation and handle the creation and downloading processes. It supports creating .NET projects with custom configurations and Azure Function Apps with various settings.

## Endpoints

### 1. `CreateAndDownloadProject`
- **Method:** `POST`
- **Route:** `/ProjectCreator/CreateAndDownloadProject`
- **Description:** Creates a project based on the provided configuration and downloads it as a ZIP file.
- **Request Body:** `CreateProjectConfig`
- **Responses:**
  - `200 OK`: Project created successfully and ZIP file downloaded.
  - `500 Internal Server Error`: Error during project creation or ZIP file creation.

### 2. `CreateProject`
- **Method:** `POST`
- **Route:** `/ProjectCreator/CreateProject`
- **Description:** Creates a project based on the provided configuration.
- **Request Body:** `CreateProjectConfig`
- **Responses:**
  - `200 OK`: Project created successfully.
  - `500 Internal Server Error`: Error during project creation.

### 3. `DownloadProject`
- **Method:** `GET`
- **Route:** `/ProjectCreator/DownloadProject`
- **Description:** Downloads the created project as a ZIP file.
- **Responses:**
  - `200 OK`: ZIP file of the project.
  - `500 Internal Server Error`: Error in downloading the project. Ensure the project is created first.

## Configuration Model

### `CreateProjectConfig`
```csharp
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
```

- **`Type`**: The type of project to create. For .NET projects, use types like `console`, `web`, etc. For Azure Functions, use `func`.
- **`ProjectName`**: The name of the project to create. Defaults to a name derived from the project type if not provided.
- **`FunctionName`**: The name of the function in the Azure Function App. Defaults to `NewFunc`.
- **`Template`**: The template for the Azure Function. Defaults to `HTTP trigger`.
- **`Authorization`**: Authorization level for the Azure Function. Defaults to `function`.
- **`Framework`**: The target framework for the .NET project (e.g., `net6.0`). Defaults to `net6.0`.
- **`WorkerRuntime`**: The worker runtime for Azure Functions (e.g., `dotnet`, `node`). Defaults to `dotnet`.
- **`Language`**: The programming language for the Azure Function (e.g., `c#`, `javascript`). Defaults to `c#`.

## Usage

### Creating a .NET Project

1. **Send a POST request** to `/ProjectCreator/CreateProject` with a JSON body representing `CreateProjectConfig`.
2. **Example Request:**
   ```json
   {
       "Type": "console",
       "ProjectName": "MyConsoleApp",
       "Framework": "net7.0"
   }
   ```
3. **Response:**
   - `200 OK` with a success message if the project is created successfully.
   - `500 Internal Server Error` with error details if there is an issue.

### Creating and Downloading an Azure Function App

1. **Send a POST request** to `/ProjectCreator/CreateAndDownloadProject` with a JSON body representing `CreateProjectConfig`.
2. **Example Request:**
   ```json
   {
       "Type": "func",
       "ProjectName": "MyFunctionApp",
       "FunctionName": "MyFunction",
       "Template": "HTTP trigger",
       "Authorization": "function",
       "Framework": "net6.0",
       "WorkerRuntime": "dotnet",
       "Language": "c#"
   }
   ```
3. **Response:**
   - `200 OK` with the ZIP file of the project if successful.
   - `500 Internal Server Error` if there is an issue with project creation or downloading.

### Downloading a Project

1. **Send a GET request** to `/ProjectCreator/DownloadProject`.
2. **Response:**
   - `200 OK` with the ZIP file of the project if the project was created and is available.
   - `500 Internal Server Error` if there is no project to download or if the ZIP file creation fails.

## Notes

- Ensure the necessary tools (e.g., .NET SDK, Azure Functions Core Tools) are installed and accessible in the environment where this application is running.
- Error messages in responses will help diagnose issues during project creation or downloading.
