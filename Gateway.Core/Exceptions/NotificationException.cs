namespace Gateway.Core.Exceptions
{
    public class NotificationException : ApiException
    {
        public NotificationException(string message) : base(message) { }
        public NotificationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
