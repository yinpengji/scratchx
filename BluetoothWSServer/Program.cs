using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using log4net;
using log4net.Config;
using System.IO;
using Newtonsoft.Json;

namespace BluetoothWSServer
{
    public static class BluetoothConnectorWrapper
    {
        public static BluetoothConnector BleConnector { get; set; }

        public static bool Connect(string name, string pin)
        {
            var bc = new BluetoothConnector();
            bool res = bc.ConnectDevice("BT04-A", "1234");

            if (!res)
            {
                return false;
            }
            BleConnector = bc;
            return true;
        }
    }
    public class BleDeviceService : WebSocketBehavior
    {
        public BleDeviceService()
        {

            BleConnector = BluetoothConnectorWrapper.BleConnector;
        }
        private BluetoothConnector _bleConnector = null;
        public BluetoothConnector BleConnector {
            get
            {
                return _bleConnector;
            }
            set
            {
                if (_bleConnector != null)
                {
                    _bleConnector.OnJsonEvent -= BleConnector_OnJsonEvent;
                }
                _bleConnector = value;
                _bleConnector.OnJsonEvent += BleConnector_OnJsonEvent;
            }
        }

        private void BleConnector_OnJsonEvent(object sender, JsonEventArgs e)
        {
            var result = JsonConvert.SerializeObject(e.jsonObj);
            Send(result);
        }

        public static string StreamToString(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        private static ILog logger = LogManager.GetLogger(typeof(BleDeviceService));
        protected override Task OnMessage(MessageEventArgs e)
        {
            var task = new Task(() =>
            {
                var msg = StreamToString(e.Data);
                logger.Debug("OnMessage:" + msg);
                if (BleConnector != null)
                {
                    //BleConnector.SendMessage(new JsonCommand { ID = 1, Command = "Go", Parameters = new List<object> { 25, 25 } });
                    BleConnector.SendMessage(msg);
                }
            });
            task.Start();
            return task;
            //return (Send("hello"));
        }
    }
    class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));
        
        public static void Main(string[] args)
        {

            XmlConfigurator.Configure(new System.IO.FileInfo("log.config.xml"));
            bool ret = BluetoothConnectorWrapper.Connect("BT04-A", "1234");
            if (!ret)
            {
                logger.Error("Cannot connect to device");
                return;
            }
            var wssv = new WebSocketServer(IPAddress.Parse("127.0.0.1"), 1339);
            wssv.AddWebSocketService<BleDeviceService>("/BleDevice");
            wssv.Start();
            logger.Info("Server Started");
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
