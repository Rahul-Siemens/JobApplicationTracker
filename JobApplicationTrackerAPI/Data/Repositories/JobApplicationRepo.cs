using System.Threading.Tasks;
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

        public async Task<IEnumerable<JobApplication>> GetAllJobApplications(string userId) =>
            await _context.JobApplications.Where(job => job.UserId == userId).ToListAsync();

        public async Task<JobApplication> GetJobApplicationById(string userId, int id) =>
            await _context.JobApplications.FirstOrDefaultAsync(job =>
                job.UserId == userId && job.Id == id
            );

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

        public async Task DeleteJobApplication(int id)
        {
            _context.JobApplications.Remove(new JobApplication { Id = id });
            await _context.SaveChangesAsync();
        }

        public async Task<bool> JobApplicationExist(string userId, int id) =>
            await _context.JobApplications.AnyAsync(a => a.Id == id && a.UserId == userId);
    }
}
