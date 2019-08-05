using System;

namespace SimpleCrawler
{
    public class OnCompletedEventArgs
    {
        public Uri Uri { get; private set; }

        public int ThreadId { get; private set; }

        public string PageSource { get; private set; }

        public long Milliseconds { get; private set; }

        public OnCompletedEventArgs(Uri uri, int threadId, string pageSource, long milliseconds)
        {
            Uri = uri;
            ThreadId = threadId;
            PageSource = pageSource;
            Milliseconds = milliseconds;
        }
    }
}