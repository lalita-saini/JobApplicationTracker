using JobApplicationTracker.Database;
using JobApplicationTracker.Exceptions;
using JobApplicationTracker.Models;
using JobApplicationTracker.Respositories;
using Microsoft.EntityFrameworkCore;

public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<JobApplicationRepository> _logger;

    public JobApplicationRepository(ApplicationDbContext context, ILogger<JobApplicationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<JobApplication>> GetAllApplicationsAsync(int userId)
    {
        try
        {
            return await _context.Applications
                .Where(a => a.UserId == userId)  // Filter by user ID
                .AsNoTracking()
                .OrderByDescending(a => a.DateApplied)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving applications for user {UserId}", userId);
            throw new ApplicationProcessingException("Error retrieving applications");
        }
    }

    public async Task<JobApplication> GetApplicationByIdAsync(int id, int userId)
    {
        try
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (application == null)
            {
                throw new NotFoundException($"Application with ID {id} not found");
            }

            return application;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Error retrieving application {Id} for user {UserId}", id, userId);
            throw new ApplicationProcessingException($"Error retrieving application with ID {id}");
        }
    }

    public async Task AddApplicationAsync(JobApplication application, int userId)
    {
        try
        {
            application.UserId = userId; 
            await ValidateApplication(application);

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error adding application for user {UserId}", userId);
            throw new ApplicationProcessingException("Error saving the application");
        }
    }

    public async Task UpdateApplicationAsync(JobApplication application, int userId)
    {
        try
        {
            var existingApplication = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == application.Id && a.UserId == userId);

            if (existingApplication == null)
            {
                throw new NotFoundException($"Application with ID {application.Id} not found");
            }

            await ValidateApplication(application);

            application.UserId = userId; 
            _context.Entry(existingApplication).CurrentValues.SetValues(application);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating application {Id} for user {UserId}",
                application.Id, userId);
            throw;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Error updating application {Id} for user {UserId}",
                application.Id, userId);
            throw new ApplicationProcessingException($"Error updating application with ID {application.Id}");
        }
    }

    private async Task ValidateApplication(JobApplication application)
    {
        var validationErrors = new Dictionary<string, string[]>();

        if (application.DateApplied > DateTime.UtcNow)
        {
            validationErrors.Add("DateApplied", new[] { "Application date cannot be in the future" });
        }

        var existingApplication = await _context.Applications
            .Where(a => a.UserId == application.UserId
                    && a.CompanyName == application.CompanyName
                    && a.Position == application.Position
                    && a.Id != application.Id)
            .FirstOrDefaultAsync();

        if (existingApplication != null)
        {
            validationErrors.Add("Application", new[] { "A similar application already exists" });
        }

        if (validationErrors.Any())
        {
            throw new ValidationException(validationErrors);
        }
    }
}
