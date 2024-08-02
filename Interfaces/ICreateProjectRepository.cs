using ProjectCreatorApplication.Repositorys;
namespace ProjectCreatorApplication.Interfaces

{
    public interface ICreateProjectRepository
    {
        CustomResult CreateNewProject(string? projectName);
        CustomResult CopyProject();
    }
}
