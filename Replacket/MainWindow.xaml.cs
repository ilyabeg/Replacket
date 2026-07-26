using Replacket.View.ViewModels;
using System.Windows;
using Microsoft.Win32;
using ReplacketModel.Events;

namespace Replacket
{
    public partial class MainWindow : Window
    {
        // this data context
        private MainViewModel _mainVM;
        public MainWindow()
        {
            InitializeComponent();
            _mainVM = new MainViewModel();

            _mainVM.OnPickupBrowseClick += BrowsePickupFile;
            _mainVM.OnSystemCrash += DisplayErrorMsgBox;

            DataContext = _mainVM;
        }

        // get user pcap file
        private void BrowsePickupFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            // search for pcap files only
            dialog.Filter = "Packet Capture Files (*.pcap;*.pcapng)|*.pcap;*.pcapng|All Files (*.*)|*.*";
            dialog.Title = "Select a PCAP File";

            if (dialog.ShowDialog() == true)
            {
                _mainVM.PcapFile = dialog.FileName;
            }
        }

        // error message box display
        private void DisplayErrorMsgBox(object sender, SystemErrorEventArgs e) => MessageBox.Show(e.ErrorMessage, "SYSTEM ERROR", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
    }
}