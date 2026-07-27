using ReplacketModel.Events;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace Replacket.View.ViewModels
{
    internal class PacketInfoViewModel : INotifyPropertyChanged
    {
        // packet data
		private string _packetHexaBytes;
		public string HexaBytes
        {
			get { return _packetHexaBytes; }
			set 
			{ 
				_packetHexaBytes = value;
				OnPropertyChanged();
			}
		}

		private string _protocol;
		public string Protocol
		{
			get { return _protocol; }
			set 
			{
				_protocol = value;
				OnPropertyChanged();
			}
		}

		private string _length;
		public string Length
		{
			get { return _length; }
			set 
			{
				_length = value;
                OnPropertyChanged();
            }
		}

		private string _linkLayer;
		public string LinkLayer
		{
			get { return _linkLayer; }
			set 
			{
				_linkLayer = value;
                OnPropertyChanged();
            }
		}

		private string _index;
		public string Index
		{
			get { return _index; }
			set 
			{
				_index = value;
                OnPropertyChanged();
            }
		}

		public PacketInfoViewModel() { }

        // packet info update event handler
        public void UpdatePacketInfo(object sender, PacketReceivedEventArgs e)
        {
            HexaBytes = GetHexaBytes(e.PacketData);

            Application.Current.Dispatcher.InvokeAsync(() =>
            {				
				Protocol = "Protocol: " + e.PacketProtocol;
				Length = "Length: " + e.PacketLength;
				LinkLayer = "Link Layer: " + e.PacketLinkLayer;
				Index = "Packet index: " + e.PacketIndex;
            });
        }

		private string GetHexaBytes(byte[] receivedBytes)
		{
            if (receivedBytes == null || receivedBytes.Length == 0)
                return string.Empty;

			// allocate room for 2 character of hexa bytes plus a '-' between each one
            StringBuilder hexaBytes = new StringBuilder(receivedBytes.Length * 3);
			foreach (byte receivedByte in receivedBytes)
			{
                hexaBytes.Append(receivedByte.ToString("X2")).Append("-");
            }

			return hexaBytes.ToString().Remove(hexaBytes.Length - 1); // remove very last '-'
		}

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertName));
        }
    }
}
