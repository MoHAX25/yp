namespace yp.Exceptions
{
    public class ValidationAppException : Exception
    {
        public IDictionary<string, string[]>? Errors { get; }

        public ValidationAppException(string message, IDictionary<string, string[]>? errors = null)
            : base(message)
        {
            Errors = errors;
        }
    }
}