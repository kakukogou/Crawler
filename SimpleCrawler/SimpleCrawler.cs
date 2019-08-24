using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCrawler
{
    public class SimpleCrawler
    {
        public event EventHandler<OnStartEventArgs> OnStart;

        public event EventHandler<OnCompletedEventArgs> OnCompleted;

        public event EventHandler<OnErrorEventArgs> OnError;

        public CookieContainer CookiesContainer { get; set; }

        public async Task<string> Start(Uri uri, string proxy = null)
        {
            return await Task.Run(() =>
            {
                var pageSource = string.Empty;

                try
                {
                    // Sent a satet event.
                    OnStart?.Invoke(this, new OnStartEventArgs(uri));

                    //
                    var watch = new Stopwatch();
                    watch.Start();

                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Accept = "*/*";
                    request.ServicePoint.Expect100Continue = false;//加快载入速度
                    request.ServicePoint.UseNagleAlgorithm = false;//禁止Nagle算法加快载入速度
                    request.AllowWriteStreamBuffering = false;//禁止缓冲加快载入速度
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");//定义gzip压缩页面支持
                    request.ContentType = "application/x-www-form-urlencoded";//定义文档类型及编码
                    request.AllowAutoRedirect = false;//禁止自动跳转
                    //设置User-Agent，伪装成Google Chrome浏览器
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
                    request.Timeout = 5000;//定义请求超时时间为5秒
                    request.KeepAlive = true;//启用长连接
                    request.Method = "GET";//定义请求方式为GET              
                    if (proxy != null) request.Proxy = new WebProxy(proxy);//设置代理服务器IP，伪装请求地址
                    request.CookieContainer = this.CookiesContainer;//附加Cookie容器
                    request.ServicePoint.ConnectionLimit = int.MaxValue;//定义最大连接数

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        // Save cookies.
                        CookiesContainer.Add(response.Cookies);

                        // Parse the response.
                        if (response.ContentEncoding.ToLower().Contains("gzip")) // decompress
                        {
                            using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("Big5")))
                                {
                                    pageSource = reader.ReadToEnd();
                                }
                            }
                        }
                        else if (response.ContentEncoding.ToLower().Contains("deflate")) // decompress
                        {
                            using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("Big5")))
                                {
                                    pageSource = reader.ReadToEnd();
                                }

                            }
                        }
                        else // not compressed.
                        {
                            using (Stream stream = response.GetResponseStream())//原始
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("Big5")))
                                {

                                    pageSource = reader.ReadToEnd();
                                }
                            }
                        }
                    }

                    // Abort the request.
                    request.Abort();
                    watch.Stop();

                    // Fetch the current thread id.
                    var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                    // Calculate the spent time.
                    var milliseconds = watch.ElapsedMilliseconds;

                    // Sent a completed event.
                    OnCompleted?.Invoke(this, new OnCompletedEventArgs(uri, threadId, pageSource, milliseconds));

                }
                catch (Exception ex)
                {
                    // Sent a error event.
                    OnError?.Invoke(this, new OnErrorEventArgs(uri, ex));
                }

                return pageSource;
            });
        }

        public SimpleCrawler()
        {
        }
    }
}
