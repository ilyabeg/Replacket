using ReplacketModel.Events;
using SharpPcap;
using SharpPcap.LibPcap;

namespace ReplacketModel.Models
{
    public class PcapIterator
    {
        public string PcapFile; // packets file
        public string DestFile; // destenation interface

        private bool _isRunning;

        // public events to notify UI
        public event EventHandler<SystemErrorEventArgs>? OnSystemError;

        public PcapIterator() 
        {
        }

        public async Task Iterate(int delay, int repeats)
        {
            if (string.IsNullOrEmpty(PcapFile)) 
                return;

            // start running flag
            _isRunning = true;

            try
            {
                // repeat all of this the specified times
                for (int i = 1; i <= repeats; i++)
                {
                    if (!_isRunning) break;

                    // open async to reuse worker threads from thread pool to not crash when the pickup file is too long
                    await Task.Run(() =>
                    {
                        RunIteration(delay);
                    });
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        private void RunIteration(int delay)
        {
            try
            {
                // SharpPcap file reader
                using CaptureFileReaderDevice pcapReader = new CaptureFileReaderDevice(PcapFile);
                pcapReader.Open();

                // each packet read
                while (_isRunning && (pcapReader.GetNextPacket(out PacketCapture capture)) != null)
                {
                    // TODO: Later, you will grab the live interface and send the packet here!
                    // byte[] rawData = packet.Data; 

                    RawCapture packet = capture.GetPacket();
                    packet.GetPacket().PrintHex();

                    // delay by provided seconds
                    if (delay > 0)
                    {
                        Thread.Sleep(delay * 1000);
                    }
                }

                // close reader before dispose
                pcapReader.Close();
            }
            catch (Exception e)
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Couldn't iterate through file due to: {e.Message}."));
            }
        }

        public void CeaseIterating()
        {
            _isRunning = false;
        }
    }
}
