using Replacket.View.Helpers;
using ReplacketModel.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SharpPcap;

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
        private int _progress;
        public int Progress
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

        // delay time between each packet
        private int _delay;
        public int Delay
        {
            get { return _delay; }
            set 
            {
                _delay = value;
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
                _model.PcapFile = value;
                OnPropertyChanged();
            }
        }

        // browse buttons click event
        public event Action OnPickupBrowseClick;

        // model reference
        private PcapIterator _model;

        // constructor
        public MainViewModel()
        {
            _model = new PcapIterator();

            StartCommand = new RelayCommand(ExecuteStart, CanStart);
            StopCommand = new RelayCommand(ExecuteStop);
            BrowsePickupCommand = new RelayCommand(BrowsePickupClicked);

            GetNetworkInterfaces();
        }

        private void GetNetworkInterfaces()
        {
            AvailableInterfaces.Clear();

            // all devices that SharpPcap finds
            var devices = CaptureDeviceList.Instance;

            if (devices.Count < 1)
            {
                AvailableInterfaces.Add("None.");
                return;
            }

            foreach (var device in devices)
            {
                AvailableInterfaces.Add(device.Description); // al devices names
            }
        }


        // button commands
        private void ExecuteStart(object parameter)
        {
            _model.Iterate();
        }
        private bool CanStart(object parameter)
        {
            if (string.IsNullOrWhiteSpace(PcapFile) || string.IsNullOrWhiteSpace(DestenationInterface))
                return false;

            if (!File.Exists(PcapFile))
                return false;

            if (_delay < 0 || _repeat < 0)
                return false;

            return true;
        }

        private void ExecuteStop(object parameter)
        {
            _model.CeaseIterating();
        }

        // invoke event in main window to show file browse dialog
        private void BrowsePickupClicked(object parameter) => OnPickupBrowseClick?.Invoke();


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertName));
        }
    }
}
