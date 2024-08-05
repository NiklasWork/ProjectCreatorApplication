using ProjectCreatorApplication.Models;

namespace ProjectCreatorApplication.Interfaces

{
    public interface ICreateProjectRepository
    {
        CustomResult CreateProject(CreateProjectConfig projectConfig);

        CustomResult CreateZipFile();
    }
}
