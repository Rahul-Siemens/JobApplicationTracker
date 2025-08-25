using JobApplicationTrackerAPI.Models;

namespace JobApplicationTrackerAPI.Data.Repositories
{
    public interface IJobApplicationRepo
    {
        Task<IEnumerable<JobApplication>> GetAllJobApplications();
        Task<JobApplication> GetJobApplicationById(int id);
        Task<JobApplication> AddJobApplication(JobApplication application);
        Task UpdateJobApplication(JobApplication application);
        Task<bool> JobApplicationExist(int id);
        Task DeleteJobApplication(int id);
    }
}
