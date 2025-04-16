using JobApplicationTracker.Models.Dtos;
using JobApplicationTracker.Models;
using AutoMapper;

namespace JobApplicationTracker.Mapper
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateJobApplicationDto, JobApplication>();
            CreateMap<UpdateJobApplicationDto, JobApplication>();
            CreateMap<JobApplication, JobApplicationResponseDto>();
        }
    }
}
