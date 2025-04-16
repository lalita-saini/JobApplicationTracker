namespace JobApplicationTracker.Models.Dtos
{
    public class JobApplicationResponseDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string Status { get; set; }
        public DateTime DateApplied { get; set; }
    }
}
