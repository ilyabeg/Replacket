using ReplacketModel.Events;
using SharpPcap;
using SharpPcap.LibPcap;

namespace ReplacketModel.Models
{
    public class PcapIterator
    {
        public string PcapFile; // packets file
        public string DestFile; // destenation interface

        // keep pcap file reader reference for continuation
        private CaptureFileReaderDevice? _pcapReader;
        private int _currentRepeat;

        // fields for calculating progress 
        private int _totalPackets;
        private int _currentPacketIndex;

        private bool _isRunning;

        // public events to notify UI
        public event EventHandler<SystemErrorEventArgs>? OnSystemError;
        public event EventHandler<ProgressUpdateEventArgs>? OnProgressUpdate;
        public event EventHandler<PacketReceivedEventArgs>? OnPacketReceived;

        public PcapIterator() 
        {
        }

        public async Task StartIterations(int delay, int repeats)
        {
            if (string.IsNullOrEmpty(PcapFile)) 
                return;

            // start running flag
            _isRunning = true;

            try
            {
                // repeat all of this the specified repeat times provided
                while (_currentRepeat <= repeats)
                {
                    if (!_isRunning) break;

                    // if reader not initialized or was reset
                    if (_pcapReader == null)
                    {
                        _totalPackets = GetPcapLength();

                        _pcapReader = new CaptureFileReaderDevice(PcapFile);
                        _pcapReader.Open();
                        _currentPacketIndex = 0;
                    }

                    // open async to reuse worker threads from thread pool to not crash when the pickup file is too long
                    await Task.Run(async () => { await Iterate(delay); });
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task Iterate(int delay)
        {
            try
            {
                // foreach packet read
                while (_isRunning)
                {
                    GetPacketStatus status = _pcapReader!.GetNextPacket(out PacketCapture capture);

                    // if finished reading the whole pickup
                    if (status == GetPacketStatus.NoRemainingPackets)
                    {
                        _pcapReader.Close();
                        _pcapReader = null;

                        _currentPacketIndex = 0; // <- start from the beginning
                        _currentRepeat++;        // <- move to next iteration

                        break; // Break out of the loop so StartIterations can repeat
                    }

                    RawCapture packet = capture.GetPacket();

                    // update packet and progress in UI
                    OnPacketReceived?.Invoke(this, new PacketReceivedEventArgs(
                        packet.Data,
                        "tcp",
                        packet.PacketLength,
                        packet.LinkLayerType.ToString(),
                        _currentPacketIndex
                    ));

                    _currentPacketIndex++;
                    CalculateProgress();

                    // forward packet
                    SendToDestInterface(packet);

                    // delay by provided milliseconds
                    if (delay > 0)
                    {
                        await Task.Delay(delay);
                    }
                }            
            }
            catch (Exception e)
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Couldn't iterate through file due to: {e.Message}."));
            }
        }

        // STOP button click
        public void CeaseIterating() => _isRunning = false;

        // reset data if files changed
        public void Reset()
        {           
            _isRunning = false;

            if (_pcapReader != null)
            {
                _pcapReader.Close();
                _pcapReader = null;
            }

            _currentPacketIndex = 0; // <- start from the begining
            _totalPackets = 0;       // <- reset pickup length
            _currentRepeat = 0;      // <- repeat again
        }

        // forward to dest interface
        private void SendToDestInterface(RawCapture packet)
        {
            //Send(packetBytes, DestInterface);
        }

        // pcap progress calculation
        private void CalculateProgress()
        {
            if (_totalPackets > 0)
            {
                double progress = (_currentPacketIndex / (double)_totalPackets) * 100;
                OnProgressUpdate?.Invoke(this, new ProgressUpdateEventArgs(progress));
            }
        }

        // pickup length calculation
        private int GetPcapLength()
        {
            int count = 0;

            using CaptureFileReaderDevice reader = new CaptureFileReaderDevice(PcapFile);
            reader.Open();

            while (reader?.GetNextPacket(out _) != GetPacketStatus.NoRemainingPackets)
            {
                count++;
            }

            reader.Close();
            return count;
        }
    }
}
