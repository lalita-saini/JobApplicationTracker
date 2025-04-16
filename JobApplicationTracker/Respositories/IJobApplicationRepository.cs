using JobApplicationTracker.Models;

namespace JobApplicationTracker.Respositories
{
    public interface IJobApplicationRepository
    {
        Task<IEnumerable<JobApplication>> GetAllApplicationsAsync(int userId);
        Task<JobApplication> GetApplicationByIdAsync(int id, int userId);
        Task AddApplicationAsync(JobApplication application, int userId);
        Task UpdateApplicationAsync(JobApplication application, int userId);
    }
}
