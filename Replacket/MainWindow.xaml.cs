using Replacket.View.ViewModels;
using System.Windows;
using Microsoft.Win32;

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
    }
}