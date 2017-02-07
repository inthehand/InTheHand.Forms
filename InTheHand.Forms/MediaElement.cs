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
    public sealed class MediaElement : Xamarin.Forms.View
    {
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
          BindableProperty.Create(nameof(Position), typeof(TimeSpan), typeof(MediaElement), TimeSpan.Zero);

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
            private set
            {
                SetValue(CurrentStateProperty, value);
                CurrentStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the current position of progress through the media's playback time.
        /// </summary>
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
        /// Occurs when the value of the CurrentState property changes.
        /// </summary>
        public event EventHandler CurrentStateChanged;

        /// <summary>
        /// Occurs when the MediaElement finishes playing audio or video.
        /// </summary>
        public event EventHandler MediaEnded;

        /// <summary>
        /// Occurs when the media stream has been validated and opened, and the file headers have been read.
        /// </summary>
        public event EventHandler MediaOpened;

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

        internal void OnMediaOpened()
        {
            if (MediaOpened != null)
            {
                System.Diagnostics.Debug.WriteLine("Media Opened");
                MediaOpened(this, EventArgs.Empty);
            }
        }
    }
}
