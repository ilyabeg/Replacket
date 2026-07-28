using Replacket.View.Helpers;
using ReplacketModel.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SharpPcap;
using ReplacketModel.Events;
using System.Windows;

namespace Replacket.View.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        // switch commands
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        // browse files command
        public ICommand BrowsePickupCommand { get; }


        // progress bar progress
        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set 
            { 
                _progress = value;
                OnPropertyChanged();
            }
        }

        // is checked boolean for the check box
        private bool _normalChecked;
        public bool NormalChecked
        {
            get { return _normalChecked; }
            set 
            {
                _normalChecked = value;
                OnPropertyChanged();

                // no delay is checked
                if (_normalChecked)
                {
                    Delay = 0;
                }
            }
        }

        // og difference boolean for the check box
        private bool _diffChecked;
        public bool DifferenceChecked
        {
            get { return _diffChecked; }
            set
            {
                _diffChecked = value;
                OnPropertyChanged();

                // regular speed
                if (_diffChecked)
                {
                    SelectedSpeed = 1.0;
                }
            }
        }

        // delay time between each packet
        private int _delay;
        public int Delay
        {
            get { return _delay; }
            set 
            {
                if (_delay == value) return;

                _delay = value;
                OnPropertyChanged();
            }
        }

        // all options for settings combo box
        public ObservableCollection<double> AvailableSpeeds { get; } = new() { 0.25, 0.5, 0.75, 1, 1.25, 1.5, 1.75, 2, 3, 5, 10, 100 };

        // destanation interface file (selection)
        private double? _speed;
        public double? SelectedSpeed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                _model.PlaybackSpeed = value;
                OnPropertyChanged();
            }
        }

        // pick up file repeats
        private int _repeat;
        public int Repeat
        {
            get { return _repeat; }
            set 
            {
                if (_repeat == value) return;

                _repeat = value;
                OnPropertyChanged();
            }
        }

        // all avaiable network interfaces on my computer
        public ObservableCollection<string> AvailableInterfaces { get; set; } = new();

        // destanation interface file (selection)
        private string _destInterface;
        public string DestenationInterface
        {
            get { return _destInterface; }
            set
            {
                _destInterface = value;
                _model.DestFile = value;
                OnPropertyChanged();
            }
        }

        // selected pickup file 
        private string _pcapFile;
        public string PcapFile
        {
            get { return _pcapFile; }
            set 
            {
                _pcapFile = value;
                Progress = 0;
                _model.PcapFile = value;
                _model.Reset(); // reset iterator data
                OnPropertyChanged();
            }
        }

        // browse buttons click event
        public event Action OnPickupBrowseClick;

        // system crash event to invoke kmsgbox error in UI
        public event Action<object, SystemErrorEventArgs> OnSystemCrash;

        // model reference
        private PcapIterator _model;

        // packet info vm reference
        public PacketInfoViewModel PacketVM { get; }

        // bool to enable/disable start/stop button
        private bool _canClickStart = true;
        public bool CanClickStart
        {
            get { return _canClickStart; }
            set 
            { 
                _canClickStart = value;
                OnPropertyChanged();
            }
        }

        // CONSTRUCTOR
        public MainViewModel()
        {
            PacketVM = new PacketInfoViewModel();

            _model = new PcapIterator();
            _model.OnSystemError += (s, e) => OnSystemCrash?.Invoke(this, e);
            _model.OnProgressUpdate += (s, e) => UpdateProgressBar(this, e);
            _model.OnPacketReceived += (s, e) => PacketVM.UpdatePacketInfo(this, e);
            _model.OnSystemStart += () => CanClickStart = false;
            _model.OnSystemEnd += () => CanClickStart = true;

            StartCommand = new RelayCommand(ExecuteStart, CanStart);
            StopCommand = new RelayCommand(ExecuteStop);
            BrowsePickupCommand = new RelayCommand(BrowsePickupClicked);

            GetNetworkInterfaces();
        }

        // initialize combo boxes
        private void GetNetworkInterfaces()
        {
            AvailableInterfaces.Clear();

            // all devices that SharpPcap finds
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            devices.Refresh();

            if (devices.Count < 1)
            {
                AvailableInterfaces.Add("None.");
                return;
            }

            foreach (var device in devices)
            {   
                AvailableInterfaces.Add(device.Description); // al devices names}
            }
        }

        // button commands
        private async void ExecuteStart(object parameter)
        {
            CanClickStart = false;
            await _model.StartIterations(Delay, Repeat);
        }
        private bool CanStart(object parameter)
        {
            if (string.IsNullOrWhiteSpace(PcapFile) || string.IsNullOrWhiteSpace(DestenationInterface))
                return false;

            if (!File.Exists(PcapFile)) 
                return false;

            if (_delay < 0 || _repeat < 0) 
                return false;

            if (SelectedSpeed == null) 
                return false;

            return true;
        }
        private void ExecuteStop(object parameter)
        {
            CanClickStart = true;
            _model.CeaseIterating();
        }

        // invoke event in main window to show file browse dialog
        private void BrowsePickupClicked(object parameter) => OnPickupBrowseClick?.Invoke();


        // progress bar update event handler
        private void UpdateProgressBar(object sender, ProgressUpdateEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Progress = e.Progress;
            });
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertName));
        }
    }
}
