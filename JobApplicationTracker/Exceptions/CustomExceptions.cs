using System.Net;

namespace JobApplicationTracker.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
        {
            Errors = errors;
        }
    }

    public class ApplicationProcessingException : Exception
    {
        public ApplicationProcessingException(string message) : base(message)
        {
        }
    }
} 