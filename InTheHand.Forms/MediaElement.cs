using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace InTheHand.Forms
{
    public sealed class MediaElement : Xamarin.Forms.View
    {
        //Bindable properties
        public static readonly BindableProperty SourceProperty =
          BindableProperty.Create("Source", typeof(Uri), typeof(MediaElement));

        public static readonly BindableProperty CurrentStateProperty =
          BindableProperty.Create("CurrentState", typeof(MediaElementState), typeof(MediaElement), MediaElementState.Closed);

        public static readonly BindableProperty PositionProperty =
          BindableProperty.Create("Position", typeof(TimeSpan), typeof(MediaElement), TimeSpan.Zero);

        //Gets or sets the source
        [Xamarin.Forms.TypeConverter(typeof(UriTypeConverter))]
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public MediaElementState CurrentState
        {
            get { return (MediaElementState)GetValue(CurrentStateProperty); }
            private set
            {
                SetValue(CurrentStateProperty, value);
                CurrentStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public System.TimeSpan Position
        {
            get
            {
                return (TimeSpan)GetValue(PositionProperty);
            }

            set
            {
                SetValue(PositionProperty, value);
                SeekCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Play()
        {
            CurrentState = MediaElementState.Playing;
        }

        public void Pause()
        {
            if(CurrentState == MediaElementState.Playing)
            {
                CurrentState = MediaElementState.Paused;
            }
        }

        public void Stop()
        {
            if (CurrentState != MediaElementState.Closed)
            {
                CurrentState = MediaElementState.Stopped;
            }
        }

        public event EventHandler CurrentStateChanged;

        public event EventHandler MediaEnded;

        public event EventHandler MediaOpened;

        public event EventHandler SeekCompleted;

        public void OnMediaEnded()
        {
            if(MediaEnded != null)
            {
                System.Diagnostics.Debug.WriteLine("Media Ended");
                MediaEnded(this, EventArgs.Empty);
            }
        }

        public void OnMediaOpened()
        {
            if (MediaOpened != null)
            {
                System.Diagnostics.Debug.WriteLine("Media Opened");
                MediaOpened(this, EventArgs.Empty);
            }
        }
    }

    public enum MediaElementState
    {
        Buffering,
        Closed,
        Opening,
        Paused,
        Playing,
        Stopped,
    }
}
