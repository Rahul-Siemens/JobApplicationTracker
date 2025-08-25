using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using JobApplicationTrackerAPI.Data.Repositories;
using JobApplicationTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationTrackerAPI.Controllers
{
    [ApiController]
    [Route("applications")]
    [Authorize]
    public class JobApplicationsController : ControllerBase
    {
        private readonly IJobApplicationRepo _repository;

        public JobApplicationsController(IJobApplicationRepo repository)
        {
            _repository = repository;
        }

        [ProducesResponseType(typeof(IEnumerable<JobApplication>), (int)HttpStatusCode.OK)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var apps = await _repository.GetAllJobApplications(userId);
            return Ok(apps);
        }

        [ProducesResponseType(typeof(JobApplication), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var app = await _repository.GetJobApplicationById(userId, id);
            if (app == null)
            {
                return NotFound(new { message = $"Job application with ID {id} not found." });
            }
            else
            {
                return Ok(app);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        public async Task<IActionResult> Create(JobApplication application)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                application.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var created = await _repository.AddJobApplication(application);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "An error occurred while creating the job application.",
                        details = ex.Message,
                        stackTrace = ex?.InnerException?.StackTrace,
                    }
                );
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, JobApplication application)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _repository.JobApplicationExist(userId, id))
                return NotFound(
                    new { message = $"Job application with ID {id} not found for User." }
                );

            application.Id = id;
            try
            {
                await _repository.UpdateJobApplication(application);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = $"Error updating the job application with ID {id}.",
                        details = ex.Message,
                        stackTrace = ex?.InnerException?.StackTrace,
                    }
                );
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _repository.JobApplicationExist(userId, id))
                return NotFound(
                    new { message = $"Job application with ID {id} not found for User." }
                );

            try
            {
                await _repository.DeleteJobApplication(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = $"Error deleting the job application with ID {id} .",
                        details = ex.Message,
                        ex?.InnerException?.StackTrace,
                    }
                );
            }
        }
    }
}
