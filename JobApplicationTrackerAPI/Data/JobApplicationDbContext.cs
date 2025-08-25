using JobApplicationTrackerAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationTrackerAPI.Data
{
    public class JobApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public JobApplicationDbContext(DbContextOptions<JobApplicationDbContext> options)
            : base(options) { }

        public DbSet<JobApplication> JobApplications { get; set; }
    }
}
