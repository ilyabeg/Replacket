
namespace ReplacketModel.Events
{
    public class SystemErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }

        public SystemErrorEventArgs(string message)
        {
            ErrorMessage = message;
        }
    }
}
