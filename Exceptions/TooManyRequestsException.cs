namespace RoSharp.Exceptions
{
    public class TooManyRequestsException : RobloxAPIException
    {
        public TooManyRequestsException() : base("Too many requests.") { }
    }
}
