using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CubeForge.Views
{
    public partial class PlaceholderView : UserControl
    {
        public PlaceholderView(string message)
        {
            InitializeComponent();

            MessageTextBlock.Text = message;
        }
    }
}