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
                if (req.HttpMethod == "POST")
                {

                    runServer = PostHandler(req, resp);
                }

                if (req.HttpMethod == "HEAD")
                {
                    HeadHandler(req, resp);
                }

                if(req.HttpMethod == "GET")
                {
                    await GetHandler(req, resp, runServer);
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
            string readRequest = req.Url.ToString();
            string[] subStr = readRequest.Split("/");

            resp.Headers.Clear();
            resp.Headers.Add("Server", "");

            if (string.Equals(subStr[3], "downloads"))
            {
                string dlFile = subStr[4];
                if (File.Exists(serverRoot + @"\\" + subStr[3] +"\\" + dlFile))
                {
                    FileInfo fileSize = new FileInfo(serverRoot + @"\\" + subStr[3] + "\\" + dlFile);
                   
                    resp.ContentType = "application/octet-stream";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = fileSize.Length;
                    resp.StatusCode = (int)HttpStatusCode.OK;
                    resp.KeepAlive = true;
                }
                else
                {
                    Console.WriteLine("File not found");
                    resp.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            else
            {
                Console.WriteLine("Not a valid url");
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            resp.Close();
        }

        private static async Task GetHandler(HttpListenerRequest req, HttpListenerResponse resp, bool runServer)
        {
            await Task.Run(() =>
            {
                string[] getRange;
                NameValueCollection headers = req.Headers;
                getRange = headers.GetValues("Range");
                if (getRange != null)
                {
                    char[] separators = new char[] { '=', '-' };
                    string t = getRange[0].ToString();
                    string[] substr = t.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    string upbound = substr[2];
                    string lobound = substr[1];

                    int range = int.Parse(upbound) - int.Parse(lobound);
                    int start = int.Parse(lobound);

                    //not checking for url formatting issues, assuming just our client connecting for now

                    string readRequest = req.Url.ToString();
                    string[] subStrFn = readRequest.Split("/");

                    using (FileStream fs = File.OpenRead(serverRoot + @"\" + subStrFn[3] + @"\" + subStrFn[4]))
                    {
                        string FileName = (serverRoot + @"\" + subStrFn[3] + @"\" + subStrFn[4]);
                        resp.ContentLength64 = range;
                        resp.SendChunked = false;
                        resp.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;

                        byte[] data = new byte[range + 1]; //+1?
                        int read;
                        using (BinaryWriter bw = new BinaryWriter(resp.OutputStream))
                        {
                            while ((read = fs.Read(data, start, range)) > 0)
                            {
                                bw.Write(data, 0, read);
                                bw.Flush();//remove?
                            }

                            bw.Close();
                        }

                        resp.StatusCode = (int)HttpStatusCode.OK;
                        resp.OutputStream.Close();
                    }

                }

                // Write out to the response stream (asynchronously), then close it
                //await resp.OutputStream.WriteAsync(data, 0, data.Length);
                //resp.Close();
            });
        }

        private static bool PostHandler(HttpListenerRequest req, HttpListenerResponse resp)
        {
            if(req.Url.AbsolutePath == "/shutdown")
            {
                Console.WriteLine("Shutting down server");
                resp.StatusCode = 202;
                resp.Close();
                return false;
            }
            else
            {
                //do something later perhaps?
                Console.WriteLine("bad requet");
                resp.StatusCode = 400;
                resp.Close();
                return true;
            }
        }
    }
}
    