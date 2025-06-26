using JobApplicationTrackerAPI.Data;
using JobApplicationTrackerAPI.Data.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<JobApplicationDbContext>(opt =>
    opt.UseSqlite("Data Source=jobapplicationdata.db")
);
builder.Services.AddScoped<IJobApplicationRepo, JobApplicationRepo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var ex = error.Error;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    message = "An unexpected error occurred.",
                    error = ex.Message,
                    stackTrace = ex?.StackTrace,
                }
            );
        }
    });
});

app.Run();
