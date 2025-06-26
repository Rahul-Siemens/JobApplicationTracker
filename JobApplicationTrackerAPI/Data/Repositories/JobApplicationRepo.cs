using JobApplicationTrackerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationTrackerAPI.Data.Repositories
{
    public class JobApplicationRepo : IJobApplicationRepo
    {
        private readonly JobApplicationDbContext _context;

        public JobApplicationRepo(JobApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobApplication>> GetAllJobApplications() =>
            await _context.JobApplications.ToListAsync();

        public async Task<JobApplication> GetJobApplicationById(int id) =>
            await _context.JobApplications.FindAsync(id);

        public async Task<JobApplication> AddJobApplication(JobApplication application)
        {
            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task UpdateJobApplication(JobApplication application)
        {
            _context.Entry(application).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> JobApplicationExist(int id) =>
            await _context.JobApplications.AnyAsync(a => a.Id == id);
    }
}
