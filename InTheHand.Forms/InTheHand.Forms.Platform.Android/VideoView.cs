using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;

namespace InTheHand.Forms.Platform.Android
{
    public sealed class VideoViewEx : VideoView
    {
        private int _videoHeight, _videoWidth;
        private TimeSpan _duration;

        public VideoViewEx(Context context) : base(context) { }

        public override void SetVideoPath(string path)
        {
            MediaMetadataRetriever retriever = new MediaMetadataRetriever();
            retriever.SetDataSource(path);
            ExtractMetadata(retriever);

            base.SetVideoPath(path);
        }

        private void ExtractMetadata(MediaMetadataRetriever retriever)
        {
            _duration = TimeSpan.Zero;
            _videoWidth = int.Parse(retriever.ExtractMetadata(MetadataKey.VideoWidth));
            _videoHeight = int.Parse(retriever.ExtractMetadata(MetadataKey.VideoWidth));

            string durationString = retriever.ExtractMetadata(MetadataKey.Duration);
            if (!string.IsNullOrEmpty(durationString))
            {
                long durationMS = long.Parse(durationString);
                _duration = TimeSpan.FromMilliseconds(durationMS);
            }
        }

        public override void SetVideoURI(global::Android.Net.Uri uri)
        {
            MediaMetadataRetriever retriever = new MediaMetadataRetriever();
            if (uri.Scheme.StartsWith("http"))
            {
                retriever.SetDataSource(uri.ToString(), new Dictionary<string, string>());
            }
            else
            {
                retriever.SetDataSource(Context, uri);
            }

            ExtractMetadata(retriever);

            base.SetVideoURI(uri);
        }

        public int VideoHeight
        {
            get
            {
                return _videoHeight;
            }
        }

        public int VideoWidth
        {
            get
            {
                return _videoWidth;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return _duration;
            }
        }
        
    }
}