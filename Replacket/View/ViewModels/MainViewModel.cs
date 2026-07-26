using Replacket.View.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.IO;

namespace Replacket.View.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        // switch commands
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        // browse files command
        public ICommand BrowsePickupCommand { get; }
        public ICommand BrowseDestCommand { get; }


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

        // destanation interface file
        private string _destInterface;
        public string DestFile
        {
            get { return _destInterface; }
            set 
            {
                _destInterface = value;
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
                OnPropertyChanged();
            }
        }

        // browse buttons click event
        public event Action OnPickupBrowseClick;
        public event Action OnDestBrowseClick;

        // model reference
        //private Model _model;

        // constructor
        public MainViewModel()
        {
            //_model = new Model();

            StartCommand = new RelayCommand(ExecuteStart, CanStart);
            StopCommand = new RelayCommand(ExecuteStop);
            BrowsePickupCommand = new RelayCommand(BrowsePickupClicked);
            BrowseDestCommand = new RelayCommand(BrowseDestClicked);
        }


        // button commands
        private void ExecuteStart(object parameter)
        {
            // model.SendToDest(_destInterface, _pcapFile);
        }
        private bool CanStart(object parameter)
        {
            if (string.IsNullOrWhiteSpace(PcapFile) || string.IsNullOrWhiteSpace(DestFile))
                return false;

            if (!File.Exists(PcapFile))
                return false;

            if (!File.Exists(DestFile))
                return false;

            if (_delay < 0 || _repeat < 0)
                return false;

            return true;
        }

        private void ExecuteStop(object parameter)
        {
            //model.Stop();
        }

        // invoke event in main window to show file browse dialog
        private void BrowsePickupClicked(object parameter) => OnPickupBrowseClick?.Invoke();
        private void BrowseDestClicked(object parameter) => OnDestBrowseClick?.Invoke();


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertName));
        }
    }
}
