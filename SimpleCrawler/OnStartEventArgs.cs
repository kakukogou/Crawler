using System;

namespace SimpleCrawler
{
    public class OnStartEventArgs
    {
        public Uri Uri { get; }

        public OnStartEventArgs(Uri uri)
        {
            Uri = uri;
        }
    }
}