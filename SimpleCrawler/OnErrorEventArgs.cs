using System;

namespace SimpleCrawler
{
    public class OnErrorEventArgs
    {
        public Uri Uri { get; private set; }

        public Exception Exception { get; private set; }

        public OnErrorEventArgs(Uri uri, Exception exception)
        {
            Uri = uri;
            Exception = exception;
        }
    }
}