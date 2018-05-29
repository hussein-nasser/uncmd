using Esri.UtilityNetwork.RestSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Author  : Hussein Nasser
/// Date    : May/ 29 / 2018
/// Twitter : @hnasr 
/// </summary>
namespace uncmd
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                string username = "";
                string password = "";
                string url = "";
                string serviceName = "";
                string validateExtent = "";
                bool validate = false;
                bool updatesubnetwork = false;
                bool isConnected = false;
                bool logging = false;


                //parse the args
                foreach (var x in args)
                {
                    if (x == "/v") validate = true;
                    if (x == "/u") updatesubnetwork = true;
                    if (x == "/c") isConnected = true;
                    if (x == "/l") logging = true;

                    if (x.Contains("/user:")) username = x.Replace("/user:", "");
                    if (x.Contains("/pass:")) password = x.Replace("/pass:", "");
                    if (x.Contains("/url:")) url = x.Replace("/url:", "");
                    if (x.Contains("/s:")) serviceName = x.Replace("/s:", "");
                    if (x.Contains("/e:")) validateExtent = x.Replace("/e:", "");
                }
                //enable logging to a file on a folder called logs in the same directory as the uncmd.exe 
                if (logging)
                {
                    string logPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location), "logs");
                    //create logs folder if it doesn't exists
                    if (!System.IO.Directory.Exists(logPath))
                        System.IO.Directory.CreateDirectory(logPath);

                    FileStream filestream = new FileStream(logPath + @"\log" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt", FileMode.OpenOrCreate);
                    var streamwriter = new StreamWriter(filestream);
                    streamwriter.AutoFlush = true;
                    Console.SetOut(streamwriter);
                    Console.SetError(streamwriter);
                }
                //check that we have everthing..
                if (username == "" || password == "" || url == "" || serviceName == "")
                {
                    Console.WriteLine("Missing parameters, please provide username, password, portal url and the service name you want to connect to as follows: \nuncmd.exe /url:utilitynetwork.esri.com/portal /s:RedTrolley_Oracle /user:admin /pass:esri.agp /v\n/v will validate the entire network topology\n/v /e:{extent_no_spaces} To validate an extent\n/u will update all subnetworks\n/c update is connected/s:all apply to all services for the logged in user.");
                    // Console.ReadLine();
                    Environment.Exit(1);
                }

                url = "https://" + url;
                UNWorkspace unworkspace = Esri.UtilityNetwork.RestSDK.UNWorkspace.getWorkspace();
                List<Service> services = unworkspace.GetServices(url, username, password);
                bool updated = false;
                foreach (Service service in services)
                    if (service.Title.ToLower() == serviceName.ToLower() || serviceName == "all")
                    {
                        try
                        {
                            Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] Connecting to service " + service.Title);

                            //service found lets connect and validate
                            unworkspace.Connect(url, service.URL, username, password);

                            if (validate)
                                if (validateExtent == "")
                                    unworkspace.ValidateNetworkTopology();
                                else
                                    unworkspace.ValidateNetworkTopology(validateExtent);

                            if (updatesubnetwork)
                                unworkspace.UpdateSubnetworks();

                            if (isConnected)
                                unworkspace.UpdateIsConnected();
                            //success


                            updated = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Error] " + ex.Message);

                        }
                        // Environment.Exit(0);
                    }

                if (!updated)
                    Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Error] Service '" + serviceName + "' not found");

                Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Done] ");
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [Error] " + ex.Message);
                // Console.ReadLine();
            }

        }
    }
}
