using ProjectCreatorApplication.Repositorys;
namespace ProjectCreatorApplication.Interfaces

{
    public interface ICreateProjectRepository
    {
        CustomResult CreateProject(string? projectName);

        CustomResult CreateZipFile();
        //CustomResult CopyProject();
    }
}
