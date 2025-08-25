using JobApplicationTrackerAPI.Models;

namespace JobApplicationTrackerAPI.Data.Repositories
{
    public interface IJobApplicationRepo
    {
        Task<IEnumerable<JobApplication>> GetAllJobApplications(string userId);
        Task<JobApplication> GetJobApplicationById(string userId, int id);
        Task<JobApplication> AddJobApplication(JobApplication application);
        Task UpdateJobApplication(JobApplication application);
        Task<bool> JobApplicationExist(string userId, int id);
        Task DeleteJobApplication(int id);
    }
}
