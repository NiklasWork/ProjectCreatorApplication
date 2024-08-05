using Microsoft.AspNetCore.Mvc;
using ProjectCreatorApplication.Interfaces;
using ProjectCreatorApplication.Models;
using ProjectCreatorApplication.Repository;

namespace ProjectCreatorApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectCreatorController : ControllerBase
    {
        private readonly ICreateProjectRepository _cpRepo;

        public ProjectCreatorController()
        {
            _cpRepo = new CreateProjectRepository();
        }

        [HttpPost("CreateProject")]
        public IActionResult CreateProject([FromBody] CreateProjectConfig projectConfig)
        {
            var result = _cpRepo.CreateProject(projectConfig);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return StatusCode(500, result.Message);
        }

        [HttpPost("CreateAndDownloadProject")]
        public IActionResult CreateAndDownloadProject([FromBody] CreateProjectConfig projectConfig)
        {
            var createNewProjectResult = _cpRepo.CreateProject(projectConfig);
            if (createNewProjectResult.Success)
            {
                return DownloadProject();
            }
            return StatusCode(500, createNewProjectResult.Message);
        }

        [HttpGet("DownloadProject")]
        public IActionResult DownloadProject()
        {
            var response = _cpRepo.CreateZipFile();

            if (!response.Success)
            {
                return StatusCode(500, response.Message);
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(response.Message);

            return File(fileBytes, "application/zip", $"{response.OptinalMessage}.zip");
        }

        [HttpGet("TestApi")]
        public IActionResult TestApi()
        {
            return Ok("Hello I'm here");
        }
    }
}
