using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Pomodoro
{
    /// <summary>
    /// A small self-dismissing notification shown when a session ends. In-app (not a Windows toast) so it
    /// needs no packaged-app identity and stays consistent with the widget. Never takes focus.
    /// </summary>
    public partial class ToastWindow : Window
    {
        private static readonly TimeSpan VisibleFor = TimeSpan.FromSeconds(4);
        private const double EdgeMargin = 24.0;

        public ToastWindow(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            Left = SystemParameters.WorkArea.Right - ActualWidth - EdgeMargin;
            Top = SystemParameters.WorkArea.Bottom - ActualHeight - EdgeMargin;

            BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)));

            DispatcherTimer timer = new DispatcherTimer { Interval = VisibleFor };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                FadeOutAndClose();
            };
            timer.Start();
        }

        private void FadeOutAndClose()
        {
            DoubleAnimation fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
            fade.Completed += (_, _) => Close();
            BeginAnimation(OpacityProperty, fade);
        }
    }
}
