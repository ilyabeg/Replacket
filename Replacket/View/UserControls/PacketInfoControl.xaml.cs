using Replacket.View.ViewModels;
using System.Windows.Controls;

namespace Replacket.View.UserControls
{
    public partial class PacketInfoControl : UserControl
    {
        public PacketInfoControl()
        {
            InitializeComponent();
            DataContext = new PacketInfoViewModel();
        }
    }
}
