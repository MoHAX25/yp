namespace yp.Exceptions
{
    public class NoAvailableSeatsException : Exception
    {
        public NoAvailableSeatsException(string message = "No available seats for this event")
            : base(message)
        {
        }
    }
}
