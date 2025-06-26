using JobApplicationTrackerAPI.Models;

namespace JobApplicationTrackerAPI.Repositories
{
    public interface IJobApplicationRepository
    {
        Task<IEnumerable<JobApplication>> GetAllJobApplications();
        Task<JobApplication> GetJobApplicationById(int id);
        Task<JobApplication> AddJobApplication(JobApplication application);
        Task UpdateJobApplication(JobApplication application);
        Task<bool> JobApplicationExist(int id);
    }
}
