// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaElement.cs" company="In The Hand Ltd">
//   Copyright (c) 2017-19 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace InTheHand.Forms
{
    /// <summary>
    /// Represents an object that renders audio and video to the display.
    /// </summary>
    public sealed class MediaElement : View, IMediaElementController
    {
        /// <summary>
        /// Gets or sets a value that describes how an MediaElement should be stretched to fill the destination rectangle.
        /// </summary>
        /// <value>A value of the <see cref="Aspect"/> enumeration that specifies how the source visual media is rendered.
        /// The default value is AspectFit.</value>
        public static readonly BindableProperty AspectProperty =
             BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(MediaElement), Aspect.AspectFit);

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
        /// Identifies the CurrentState dependency property.
        /// </summary>
        public static readonly BindableProperty CurrentStateProperty =
          BindableProperty.Create(nameof(CurrentState), typeof(MediaElementState), typeof(MediaElement), MediaElementState.Closed);

        public static readonly BindableProperty DurationProperty =
          BindableProperty.Create(nameof(Duration), typeof(TimeSpan?), typeof(MediaElement), null);

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
        /// Identifies the ShowsPlaybackControls dependency property.
        /// </summary>
        public static readonly BindableProperty ShowsPlaybackControlsProperty =
          BindableProperty.Create(nameof(ShowsPlaybackControls), typeof(bool), typeof(MediaElement), false);

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static readonly BindableProperty SourceProperty =
          BindableProperty.Create(nameof(Source), typeof(Uri), typeof(MediaElement));

           /// <summary>
        /// Identifies the Position dependency property.
        /// </summary>
        public static readonly BindableProperty PositionProperty =
          BindableProperty.Create(nameof(Position), typeof(TimeSpan), typeof(MediaElement), TimeSpan.Zero);

        public static readonly BindableProperty VideoHeightProperty =
          BindableProperty.Create(nameof(VideoHeight), typeof(int), typeof(MediaElement));

        public static readonly BindableProperty VideoWidthProperty =
          BindableProperty.Create(nameof(VideoWidth), typeof(int), typeof(MediaElement));

        public static readonly BindableProperty VolumeProperty =
		  BindableProperty.Create(nameof(Volume), typeof(double), typeof(MediaElement), 1.0, BindingMode.TwoWay, new BindableProperty.ValidateValueDelegate(ValidateVolume));

        private static bool ValidateVolume(BindableObject o, object newValue)
		{
			double d = (double)newValue;

			return d >= 0.0 && d <= 1.0;
		}
        
        public Aspect Aspect
        {
            get => (Aspect)GetValue(AspectProperty);
            set => SetValue(AspectProperty, value);
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
        /// Gets the status of this MediaElement.
        /// </summary>
        public MediaElementState CurrentState
        {
            get { return (MediaElementState)GetValue(CurrentStateProperty); }
        }

        public TimeSpan? Duration
        {
            get { return (TimeSpan?)GetValue(DurationProperty); }
        }

        public IDictionary<string, string> HttpHeaders { get; } = new Dictionary<string, string>();

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
        /// Gets or sets a value that determines whether the standard transport controls are enabled.
        /// </summary>
        public bool ShowsPlaybackControls
        {
            get { return (bool)GetValue(ShowsPlaybackControlsProperty); }
            set { SetValue(ShowsPlaybackControlsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the current position of progress through the media's playback time.
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                PositionRequested?.Invoke(this, EventArgs.Empty);
                return (TimeSpan)GetValue(PositionProperty);
            }

            set
            {
                SeekRequested?.Invoke(this, new SeekRequested(value));
            }
        }

        public int VideoHeight
        {
            get { return (int)GetValue(VideoHeightProperty); }
        }

        public int VideoWidth
        {
            get { return (int)GetValue(VideoWidthProperty); }
        }

        public double Volume
        {
            get
            {
                VolumeRequested?.Invoke(this, EventArgs.Empty);
                return (double)GetValue(VolumeProperty);
            }
            set
            {
                SetValue(VolumeProperty, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler<SeekRequested> SeekRequested;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler<StateRequested> StateRequested;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler PositionRequested;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler VolumeRequested;

        /// <summary>
        /// Plays media from the current position.
        /// </summary>
        public void Play()
        {
            StateRequested?.Invoke(this, new StateRequested(MediaElementState.Playing));
        }

        /// <summary>
        /// Pauses media at the current position.
        /// </summary>
        public void Pause()
        {
            StateRequested?.Invoke(this, new StateRequested(MediaElementState.Paused));
        }

        /// <summary>
        /// Stops and resets media to be played from the beginning.
        /// </summary>
        public void Stop()
        {
            StateRequested?.Invoke(this, new StateRequested(MediaElementState.Stopped));
        }
        
        double IMediaElementController.BufferingProgress
        {
            get => (double)GetValue(BufferingProgressProperty);
            set => SetValue(BufferingProgressProperty, value);
        }
        MediaElementState IMediaElementController.CurrentState
        {
            get => (MediaElementState)GetValue(CurrentStateProperty);
            set => SetValue(CurrentStateProperty, value);
        }
        TimeSpan? IMediaElementController.Duration
        {
            get => (TimeSpan?)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }
        TimeSpan IMediaElementController.Position
        {
            get => (TimeSpan)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        int IMediaElementController.VideoHeight
        {
            get => (int)GetValue(VideoHeightProperty);
            set => SetValue(VideoHeightProperty, value);
        }
        int IMediaElementController.VideoWidth
        {
            get => (int)GetValue(VideoWidthProperty);
            set => SetValue(VideoWidthProperty, value);
        }
        double IMediaElementController.Volume
        {
            get => (double)GetValue(VolumeProperty);
            set => SetValue(VolumeProperty, value);
        }

        void IMediaElementController.OnMediaEnded()
        {
            SetValue(CurrentStateProperty, MediaElementState.Stopped);
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the MediaElement finishes playing audio or video.
        /// </summary>
        public event EventHandler MediaEnded;

        void IMediaElementController.OnMediaFailed()
        {
            MediaFailed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler MediaFailed;

        void IMediaElementController.OnMediaOpened()
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the media stream has been validated and opened, and the file headers have been read.
        /// </summary>
        public event EventHandler MediaOpened;

        void IMediaElementController.OnSeekCompleted()
        {
            SeekCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the seek point of a requested seek operation is ready for playback.
        /// </summary>
        public event EventHandler SeekCompleted;

        internal void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SeekRequested : EventArgs
    {
        public TimeSpan Position { get; }

        public SeekRequested(TimeSpan position)
        {
            Position = position;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StateRequested : EventArgs
    {
        public MediaElementState State { get; }

        public StateRequested(MediaElementState state)
        {
            State = state;
        }
    }

    public interface IMediaElementController
    {
        double BufferingProgress { get; set; }
        MediaElementState CurrentState { get; set; }
        TimeSpan? Duration { get; set; }
        TimeSpan Position { get; set; }
        int VideoHeight { get; set; }
        int VideoWidth { get; set; }
        double Volume { get; set; }

        void OnMediaEnded();
        void OnMediaFailed();
        void OnMediaOpened();
        void OnSeekCompleted();

    }
}
