using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace InTheHand.Forms.Platform.WPF
{
    /// <summary>
    /// Interaction logic for ProgressRing.xaml
    /// </summary>
    public partial class ProgressRing : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="IsActive"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(ProgressRing), new PropertyMetadata(false, new PropertyChangedCallback(IsActiveChanged)));

        private Storyboard animation;

        public ProgressRing()
        {
            InitializeComponent();
            
            animation = (Storyboard)Resources["ProgressRingStoryboard"];
        }

        /// <summary>
        /// Gets or sets whether the <see cref="ProgressRing"/> should finish its last animation cycle on progress complete.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return (bool)GetValue(IsActiveProperty);
            }

            set
            {
                SetValue(IsActiveProperty, value);
            }
        }

        private static void IsActiveChanged(DependencyObject sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((ProgressRing)sender).OnIsActiveChanged(Convert.ToBoolean(e.NewValue));
        }

        private void OnIsActiveChanged(bool newValue)
        {
            if (newValue)
            {
                animation.Begin();
            }
            else
            {
                animation.Stop();
            }
        }
    }
}
