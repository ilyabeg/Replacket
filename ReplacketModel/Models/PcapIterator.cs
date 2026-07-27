using ReplacketModel.Events;
using SharpPcap;
using SharpPcap.LibPcap;

namespace ReplacketModel.Models
{
    public class PcapIterator
    {
        public string PcapFile;      // packets file
        public string DestFile;      // destenation interface
        public double PlaybackSpeed; // speed of forwarding

        // keep pcap file reader reference for continuation
        private CaptureFileReaderDevice? _pcapReader;
        private int _currentRepeat;

        // keep reference of the selected network interface
        private ILiveDevice? _destInterface;

        // fields for calculating progress 
        private int _totalPackets;
        private int _currentPacketIndex;

        private bool _isRunning;

        // public events to notify UI
        public event EventHandler<SystemErrorEventArgs>? OnSystemError;
        public event EventHandler<ProgressUpdateEventArgs>? OnProgressUpdate;
        public event EventHandler<PacketReceivedEventArgs>? OnPacketReceived;

        // public events to block user from activating model more than once at a time
        public event Action OnSystemStart;
        public event Action OnSystemEnd;

        public PcapIterator() { }

        public async Task StartIterations(int delay, int repeats)
        {
            if (string.IsNullOrEmpty(PcapFile)) return;

            // start system
            _isRunning = true;
            OnSystemStart?.Invoke();

            try
            {
                // repeat all of this the specified repeat times provided
                while (_currentRepeat <= repeats && _isRunning)
                {
                    // if reader not initialized or was reset
                    if (_pcapReader == null) InitReader();
                    InitDestenation();

                    // open async to reuse worker threads from thread pool to not crash when the pickup file is too long
                    await Task.Run(async () => { await Iterate(delay); });
                }
            }
            catch (Exception e)
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Couldn't iterate due to: {e.Message}."));
            }
            finally
            {
                // close system
                if (_currentRepeat > repeats) Reset();
                OnSystemEnd?.Invoke();
            }
        }

        private void InitReader()
        {
            _totalPackets = GetPcapLength();
            _pcapReader = new CaptureFileReaderDevice(PcapFile);
            _pcapReader.Open();
            _currentPacketIndex = 0;
        }
        private void InitDestenation()
        {
            if (_destInterface != null) _destInterface.Close();

            CaptureDeviceList devices = CaptureDeviceList.Instance;
            ICaptureDevice capDevice = devices.FirstOrDefault(d => d.Description == DestFile || d.Name == DestFile);
            _destInterface = capDevice as ILiveDevice;

            if (_destInterface == null)
                throw new Exception($"Network interface '{DestFile}' was not found.");

            // open device for sending
            _destInterface.Open();
        }

        private async Task Iterate(int delay)
        {
            try
            {
                GetPacketStatus status = _pcapReader!.GetNextPacket(out PacketCapture prevCapture);
                if (status == GetPacketStatus.NoRemainingPackets) return;

                RawCapture prevPacket = prevCapture.GetPacket();
                HandlePacket(prevPacket);

                // foreach packet read
                while (_isRunning)
                {
                    status = _pcapReader!.GetNextPacket(out PacketCapture capture);
                    // if finished reading the whole pickup
                    if (status == GetPacketStatus.NoRemainingPackets)
                    {
                        _pcapReader.Close();     // <- close reader to re-open in the next iteration
                        _pcapReader = null;
                        _currentPacketIndex = 0; // <- start from the beginning
                        _currentRepeat++;        // <- move to next iteration
                        break; // Break out of the loop so StartIterations can repeat
                    }

                    RawCapture currentPacket = capture.GetPacket();
                    double timeDiff = CalculatePacketTimeDiff(currentPacket, prevPacket);

                    // accurate packet arrival time (plus change by playback speed)
                    await Task.Delay((int)(timeDiff / PlaybackSpeed));

                    HandlePacket(currentPacket);
                    prevPacket = currentPacket;

                    // delay by provided milliseconds
                    if (delay > 0) await Task.Delay(delay);                    
                }            
            }
            catch (Exception e)
            {
                OnSystemError?.Invoke(this, new SystemErrorEventArgs($"Couldn't iterate through file due to: {e.Message}."));
            }
        }

        // STOP button click
        public void CeaseIterating() => _isRunning = false;


        // method to handle each packet 
        private void HandlePacket(RawCapture packet)
        {
            // update packet and progress in UI
            UpdateUIPacket(packet);
            CalculateProgress();
            _currentPacketIndex++;

            // forward packet
            SendToDestInterface(packet);
        }

        /// <summary>
        /// Returns the time difference between 2 packets in MS: microseconds / 1000 = milliseconds
        /// </summary>
        private double CalculatePacketTimeDiff(RawCapture current, RawCapture prev) => (current.Timeval.MicroSeconds - prev.Timeval.MicroSeconds) / 1000;

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
            if (_destInterface != null)
                _destInterface.SendPacket(packet.Data);
        }

        // packet info invoker
        private void UpdateUIPacket(RawCapture packet)
        {
            OnPacketReceived?.Invoke(this, new PacketReceivedEventArgs(
                packet.Data,
                packet.PacketLength,
                packet.LinkLayerType.ToString(),
                _currentPacketIndex
            ));
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
