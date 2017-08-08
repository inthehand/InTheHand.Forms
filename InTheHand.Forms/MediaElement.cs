// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaElement.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Xamarin.Forms;

namespace InTheHand.Forms
{
    /// <summary>
    /// Represents an object that renders audio and video to the display.
    /// </summary>
    public sealed class MediaElement : View
    {
        
        /// <summary>
        /// Identifies the AreTransportControlsEnabled dependency property.
        /// </summary>
        public static readonly BindableProperty AreTransportControlsEnabledProperty =
          BindableProperty.Create(nameof(AreTransportControlsEnabled), typeof(bool), typeof(MediaElement), false);
        
        /// <summary>
        /// Identifies the AutoPlay dependency property.
        /// </summary>
        public static readonly BindableProperty AutoPlayProperty =
          BindableProperty.Create(nameof(AutoPlay), typeof(bool), typeof(MediaElement), true);

        /// <summary>
        /// Identifies the BufferingProgress dependency property.
        /// </summary>
        public static readonly BindableProperty BufferingProgressProperty =
          BindableProperty.Create(nameof(BufferingProgress), typeof(double), typeof(MediaElement), 0.0);

        /// <summary>
        /// Identifies the IsLooping dependency property.
        /// </summary>
        public static readonly BindableProperty IsLoopingProperty =
          BindableProperty.Create(nameof(IsLooping), typeof(bool), typeof(MediaElement), false);

        /// <summary>
        /// Identifies the KeepScreenOn dependency property.
        /// </summary>
        public static readonly BindableProperty KeepScreenOnProperty =
          BindableProperty.Create(nameof(KeepScreenOn), typeof(bool), typeof(MediaElement), false);

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static readonly BindableProperty SourceProperty =
          BindableProperty.Create(nameof(Source), typeof(Uri), typeof(MediaElement));

        /// <summary>
        /// Identifies the CurrentState dependency property.
        /// </summary>
        public static readonly BindableProperty CurrentStateProperty =
          BindableProperty.Create(nameof(CurrentState), typeof(MediaElementState), typeof(MediaElement), MediaElementState.Closed);

        /// <summary>
        /// Identifies the Position dependency property.
        /// </summary>
        public static readonly BindableProperty PositionProperty =
          BindableProperty.Create(nameof(Position), typeof(TimeSpan), typeof(MediaElement), TimeSpan.Zero, validateValue:ValidatePosition);

        private static bool ValidatePosition(BindableObject bindable, object value)
        {
            MediaElement element = bindable as MediaElement;
            if (element != null)
            {
                if (element._renderer != null)
                {
                    element._renderer.Seek((TimeSpan)value);
                }
            }

            return true;
        }

        /// <summary>
        /// Identifies the Stretch dependency property.
        /// </summary>
        public static readonly BindableProperty StretchProperty =
          BindableProperty.Create(nameof(Stretch), typeof(Stretch), typeof(MediaElement), Stretch.Uniform);



        private IMediaElementRenderer _renderer = null;

        internal void SetRenderer(IMediaElementRenderer renderer)
        {
            _renderer = renderer;
        }


        /// <summary>
        /// Gets or sets a value that determines whether the standard transport controls are enabled.
        /// </summary>
        public bool AreTransportControlsEnabled
        {
            get { return (bool)GetValue(AreTransportControlsEnabledProperty); }
            set { SetValue(AreTransportControlsEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether media will begin playback automatically when the <see cref="Source"/> property is set.
        /// </summary>
        public bool AutoPlay
        {
            get { return (bool)GetValue(AutoPlayProperty); }
            set { SetValue(AutoPlayProperty, value); }
        }

        /// <summary>
        /// Gets a value that indicates the current buffering progress.
        /// </summary>
        /// <value>The amount of buffering that is completed for media content.
        /// The value ranges from 0 to 1. 
        /// Multiply by 100 to obtain a percentage.</value>
        public double BufferingProgress
        {
            get
            {
                return (double)GetValue(BufferingProgressProperty);
            }
        }

        /// <summary>
        /// Gets or sets a value that describes whether the media source currently loaded in the media engine should automatically set the position to the media start after reaching its end.
        /// </summary>
        public bool IsLooping
        {
            get { return (bool)GetValue(IsLoopingProperty); }
            set { SetValue(IsLoopingProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that specifies whether the control should stop the screen from timing out when playing media.
        /// </summary>
        public bool KeepScreenOn
        {
            get { return (bool)GetValue(KeepScreenOnProperty); }
            set { SetValue(KeepScreenOnProperty, value); }
        }

        public int NaturalVideoHeight
        {
            get
            {
                if(_renderer != null)
                {
                    return _renderer.NaturalVideoHeight;
                }

                return 0;
            }
        }

        public int NaturalVideoWidth
        {
            get
            {
                if (_renderer != null)
                {
                    return _renderer.NaturalVideoWidth;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets or sets a media source on the MediaElement.
        /// </summary>
        [TypeConverter(typeof(UriTypeConverter))]
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Gets the status of this MediaElement.
        /// </summary>
        public MediaElementState CurrentState
        {
            get { return (MediaElementState)GetValue(CurrentStateProperty); }
            internal set
            {
                SetValue(CurrentStateProperty, value);                
            }
        }

        internal void RaiseCurrentStateChanged()
        {
            CurrentStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the current position of progress through the media's playback time.
        /// </summary>
        public System.TimeSpan Position
        {
            get
            {
               if (_renderer != null)
                {
                    return _renderer.Position;
                }
                
                return (TimeSpan)GetValue(PositionProperty);
            }

            set
            {
                SetValue(PositionProperty, value);
            }
        }

        /// <summary>
        /// Plays media from the current position.
        /// </summary>
        public void Play()
        {
            CurrentState = MediaElementState.Playing;
        }

        /// <summary>
        /// Pauses media at the current position.
        /// </summary>
        public void Pause()
        {
            if(CurrentState == MediaElementState.Playing)
            {
                CurrentState = MediaElementState.Paused;
            }
        }

        /// <summary>
        /// Stops and resets media to be played from the beginning.
        /// </summary>
        public void Stop()
        {
            if (CurrentState != MediaElementState.Closed)
            {
                CurrentState = MediaElementState.Stopped;
            }
        }

        /// <summary>
        /// Gets or sets a value that describes how an MediaElement should be stretched to fill the destination rectangle.
        /// </summary>
        /// <value>A value of the <see cref="Stretch"/> enumeration that specifies how the source visual media is rendered.
        /// The default value is Uniform.</value>
        public Stretch Stretch
        {
            get
            {
                return (Stretch)GetValue(StretchProperty);
            }

            set
            {
                SetValue(StretchProperty, value);
            }
        }

        /// <summary>
        /// Occurs when the value of the <see cref="CurrentState"/> property changes.
        /// </summary>
        public event EventHandler CurrentStateChanged;

        /// <summary>
        /// Occurs when the MediaElement finishes playing audio or video.
        /// </summary>
        public event EventHandler MediaEnded;

        internal void RaiseMediaOpened()
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the media stream has been validated and opened, and the file headers have been read.
        /// </summary>
        public event EventHandler MediaOpened;

        internal void RaiseSeekCompleted()
        {
            SeekCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the seek point of a requested seek operation is ready for playback.
        /// </summary>
        public event EventHandler SeekCompleted;

        internal void OnMediaEnded()
        {
            if(MediaEnded != null)
            {
                System.Diagnostics.Debug.WriteLine("Media Ended");
                MediaEnded(this, EventArgs.Empty);
            }
        }

        internal void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }

    internal interface IMediaElementRenderer
    {
        double BufferingProgress { get; }

        TimeSpan NaturalDuration { get; }

        int NaturalVideoHeight { get; }

        int NaturalVideoWidth { get; }

        TimeSpan Position { get; }

        void Seek(TimeSpan time);
    }
}
