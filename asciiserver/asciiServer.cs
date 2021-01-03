using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using asciiserver;
using System.Collections.Specialized;

namespace asciiserver
{
    public class asciiServer
    {
        private static HttpListener listener;
        public static string url { get; set;}
        public static string serverRoot { get; set; }

        public static int pageViews = 0;
        public int myinteger = 0;
        public static int requestCount = 0;
        public static string pageData =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>HttpListener Example</title>" +
            "  </head>" +
            "  <body>" +
            "    <p>Page Views: {0}</p>" +
            "    <form method=\"post\" action=\"shutdown\">" +
            "      <input type=\"submit\" value=\"Shutdown\" {1}>" +
            "    </form>" +
            "  </body>" +
            "</html>";


        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                if (req.HttpMethod == "HEAD")
                {
                    HeadHandler(req, resp);
                }

                if(req.HttpMethod == "GET")
                {
                    // Make sure we don't increment the page views counter if `favicon.ico` is requested
                    if (req.Url.AbsolutePath != "/favicon.ico")
                        pageViews += 1;

                    // Write the response info
                    string disableSubmit = !runServer ? "disabled" : "";
                    byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData, pageViews, disableSubmit));
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }

            }
        }

        public static void ServerInit(string url)
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }

        private static void HeadHandler(HttpListenerRequest req, HttpListenerResponse resp)
        {
            //testing
            string readRequest = req.Url.ToString();
            string[] subStr = readRequest.Split("/");

            string whatIsThis = subStr[3];
            {
                string dlFile = subStr[4];
                if (File.Exists(serverRoot + @"\\" + subStr[3] +"\\" + dlFile))
                {
                    Console.WriteLine("ha, ha success");

                }
                else
                {
                    resp.StatusCode = 404;
                }
            }
            else
            {
                Console.WriteLine("not a valid url");
                resp.StatusCode = 400;
            }

            //string[] requestValues = req.Headers.GetValues("HEAD");

            resp.Headers.Clear();
            resp.ContentType = "application/octet-stream";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = 12345L;
            resp.StatusCode = 200;
            resp.Headers.Add("Server", "");
            resp.KeepAlive = true;
            resp.Close();
        }
    }
}
    