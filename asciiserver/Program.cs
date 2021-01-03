using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using asciiserver;

namespace asciiserver
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverURL = "http://localhost:8080/";
            asciiServer.serverRoot = @"C:\SPaCE\testAutomation\testserver\asciiserver\asciiserver";
            asciiServer.ServerInit(serverURL);

        }
    }
}
