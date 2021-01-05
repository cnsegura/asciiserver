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
            string serverURL = "";
            //asciiServer.serverRoot = @"C:\SPaCE\testAutomation\testserver\asciiserver\asciiserver";
            asciiServer.serverRoot = @"C:\ascii5g";
            asciiServer.ServerInit(serverURL);

        }
    }
}
