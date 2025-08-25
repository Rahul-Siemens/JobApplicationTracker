using System.Diagnostics;
using System.Net;
using JobApplicationTrackerAPI.Data.Repositories;
using JobApplicationTrackerAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationTrackerAPI.Controllers
{
    [ApiController]
    [Route("applications")]
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
            var apps = await _repository.GetAllJobApplications();
            return Ok(apps);
        }

        [ProducesResponseType(typeof(JobApplication), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var app = await _repository.GetJobApplicationById(id);
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

            if (!await _repository.JobApplicationExist(id))
                return NotFound(new { message = $"Job application with ID {id} not found." });

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
            if (!await _repository.JobApplicationExist(id))
                return NotFound(new { message = $"Job application with ID {id} not found." });

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
