using System;
using System.Windows.Threading;

namespace CubeForge.Views
{
    public partial class CrossTrainerView
    {
        private void UpdatePracticePanel()
        {
            target.UpdatePracticePanel();
        }

        private void RefreshTrainerLayouts()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SettingsExpander?.RefreshLayout();
            }), DispatcherPriority.Background);
        }
    }
}
