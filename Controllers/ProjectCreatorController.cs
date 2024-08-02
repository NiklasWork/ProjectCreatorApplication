using Microsoft.AspNetCore.Mvc;
using ProjectCreatorApplication.Interfaces;
using ProjectCreatorApplication.Repositorys;

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
        public IActionResult CreateNewProject([FromQuery] string? projectName)
        {
            var result = _cpRepo.CreateNewProject(projectName);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return StatusCode(500, result.Message);
        }

        [HttpGet("CopyProject")]
        public IActionResult CopyProject()
        {
            var result = _cpRepo.CopyProject();
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return StatusCode(500, result.Message);
        }

        [HttpPost("CreateAndDownloadNewProject")]
        public IActionResult CreateAndDownloadNewProject([FromQuery] string? projectName)
        {
            var createNewProjectResponse = _cpRepo.CreateNewProject(projectName);
            if (!createNewProjectResponse.Success)
            {
                return StatusCode(500, createNewProjectResponse.Message);
            }

            var copyProjectResponse = _cpRepo.CopyProject();
            if (!copyProjectResponse.Success)
            {
                return StatusCode(500, copyProjectResponse.Message);
            }

            return Ok($"{createNewProjectResponse.Message}, {copyProjectResponse.Message}");
        }

        [HttpGet("TestApi")]
        public IActionResult TestApi()
        {
            return Ok("Hello I'm here");
        }
    }
}
