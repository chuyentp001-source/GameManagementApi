namespace GameManagement.Application.Exceptions
{
    public class GameInactiveException : Exception
    {
        public GameInactiveException(string message)
            : base(message)
        {
        }
    }
}