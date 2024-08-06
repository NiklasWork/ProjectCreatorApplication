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

        [HttpPost("CreateProject")] //Debug function
        public IActionResult CreateProject([FromBody] CreateProjectConfig projectConfig)
        {
            var result = _cpRepo.CreateProject(projectConfig);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return StatusCode(500, result.Message);
        }

        [HttpGet("DownloadProject")] //Debug function
        public IActionResult DownloadProject() 
        {
            var response = _cpRepo.CreateZipFile();

            if (!response.Success)
            {
                return StatusCode(500, "Error: No Project to download. Create one first.");
            }
            
            if (response.Data == null)
            {
                return StatusCode(500, "Error: Failed to create zip file. Date == null");
            }

            return File(response.Data, "application/zip", $"{response.OptionalMessage}.zip");;
        }

        [HttpGet("TestApi")] //Debug function
        public IActionResult TestApi()
        {
            return Ok("Hello I'm here");
        }
    }
}
