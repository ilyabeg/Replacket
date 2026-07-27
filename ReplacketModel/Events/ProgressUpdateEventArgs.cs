namespace ReplacketModel.Events
{
    public class ProgressUpdateEventArgs : EventArgs
    {
        public double Progress { get; }

        public ProgressUpdateEventArgs(double progress)
        {
            Progress = progress;
        }
    }
}
