using Microsoft.AspNetCore.Mvc;
using ProjectCreatorApplication.Interfaces;
using ProjectCreatorApplication.Repositorys;
using System.IO.Compression;

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
        public IActionResult CreateProject([FromQuery] string? projectName)
        {
            var result = _cpRepo.CreateProject(projectName);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return StatusCode(500, result.Message);
        }

        //[HttpGet("CopyProject")]
        //public IActionResult CopyProject()
        //{
        //    var result = _cpRepo.CopyProject();
        //    if (result.Success)
        //    {
        //        return Ok(result.Message);
        //    }
        //    return StatusCode(500, result.Message);
        //}

        [HttpPost("CreateAndDownloadProject")]
        public IActionResult CreateAndDownloadProject([FromQuery] string? projectName)
        {
            var createNewProjectResponse = _cpRepo.CreateProject(projectName);
            if (!createNewProjectResponse.Success)
            {
                return StatusCode(500, createNewProjectResponse.Message);
            }
            return DownloadProject();
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
