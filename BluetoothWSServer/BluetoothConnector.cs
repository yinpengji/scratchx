using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.Threading;
using System.IO;
using log4net;
using log4net.Config;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace BluetoothWSServer
{
    public class JsonEventArgs : EventArgs
    {
        public Object jsonObj;
    }
    public class JsonCommand
    {
        public int ID { get; set; }
        public string Command { get; set; }
        public IList<object> Parameters { get; set; }
    }

    public class JsonResult
    {
        public int ID { get; set; }
        public string Result { get; set; }

        public string Message { get; set; }
    }
    public class BluetoothConnector
    {
        public const string MY_BT_DeviceName = "BT04-A";
        public const string MY_BT_Device_PIN = "1234";
        private BluetoothAddress ba = new BluetoothAddress(0xAB4E16000000);
        private Guid _serviceClassId = Guid.NewGuid();
        private List<BluetoothDeviceInfo> deviceList = new List<BluetoothDeviceInfo>();
        private BluetoothClient _bluetoothClient;
        private BluetoothDeviceInfo _bluetoothDevice;
        private bool _connected = false;
        private System.Net.Sockets.NetworkStream _bluetoothStream = null;

        private static ILog logger = LogManager.GetLogger(typeof(BluetoothConnector));

        public BluetoothConnector()
        {
        }

        private static void Connect(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                // client is connected now :)
                logger.Info("connected");

            }
        }

        public event EventHandler<JsonEventArgs> OnJsonEvent;
        
        
        private void ReadFromDevice()
        {
            if (_bluetoothStream != null)
            {
                int braceCount = 0;
                do
                {
                    try
                    {
                        if (_bluetoothStream.DataAvailable)
                        {
                            var reader = new StreamReader(_bluetoothStream);
                            StringBuilder stringBuilder = new StringBuilder();

                            char c = (char)reader.Read();
                            bool needParse = false;
                            while (true)
                            {
                                if (c == -1)
                                {
                                    break;
                                }
                                else if(c == '{')
                                {
                                    braceCount++;
                                }
                                else if (c=='}')
                                {
                                    braceCount--;
                                    if (braceCount==0)
                                    {
                                        needParse = true;
                                    }
                                }
                                
                                stringBuilder.Append(c);
                                logger.Debug(c);
                                if (needParse)
                                {
                                    //parsing.
                                    var jobj = JObject.Parse(stringBuilder.ToString());
                                    OnJsonEvent?.Invoke(this, new JsonEventArgs { jsonObj = jobj });
                                    logger.Debug("get a json object:" + JsonConvert.SerializeObject(jobj));
                                    needParse = false;
                                    braceCount = 0;
                                }
                                
                                c = (char)reader.Read();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                        throw;
                    }
                } while (true);

            }
                
        }
        public bool ConnectDevice(string name, string pin)
        {
            BluetoothAddress arduinoAdress = null;
            _bluetoothClient = new BluetoothClient();
            var devices = _bluetoothClient.DiscoverDevices();
            foreach (var device in devices.Where(device => device.DeviceName == name))
            {
                arduinoAdress = device.DeviceAddress;
                logger.Info("Device found, Address is:" + arduinoAdress.ToString());
            }
            _bluetoothDevice = new BluetoothDeviceInfo(arduinoAdress);

            var pairResult = false;

            int count = 3;
            while (pairResult == false && count > 0)
            {
                pairResult = BluetoothSecurity.PairRequest(_bluetoothDevice.DeviceAddress, pin);
                Thread.Sleep(1000);
                count--;
            }

            if (pairResult)
            {
                logger.Info("Pair request success");

                if (_bluetoothDevice.Authenticated)
                {
                    logger.Info("Authenticated result: Cool :D");

                    _bluetoothClient.SetPin(pin);

                    var res = _bluetoothClient.BeginConnect(_bluetoothDevice.DeviceAddress, BluetoothService.SerialPort, Connect, _bluetoothDevice);
                    res.AsyncWaitHandle.WaitOne();
                    _connected = true;
                }
                else
                {
                    logger.Error("Authenticated: So sad :(");
                }
            }
            else
            {
                logger.Error("PairRequest: Sad :(");
            }

            if (_connected)
            {
                _bluetoothStream = _bluetoothClient.GetStream();
                if (_bluetoothStream != null)
                {
                    Task task = new Task(() => ReadFromDevice());
                    task.Start();
                }
                

            }
            return _bluetoothStream != null;
        }

        public void Disconnect()
        {
            if (_bluetoothClient == null)
                return;

            try
            {
                if (_bluetoothClient != null)
                {
                    if (_bluetoothStream != null)
                    {
                        _bluetoothStream.ReadTimeout = 500;
                        _bluetoothStream.WriteTimeout = 500;
                        _bluetoothStream.Close();
                    }

                    if (_bluetoothClient.Connected)
                        _bluetoothClient.Close();
                    _bluetoothClient.Dispose();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public bool SendMessage(string message)
        {
            logger.Debug(message);
            if (_bluetoothStream != null)
            {
                var buffer = System.Text.Encoding.ASCII.GetBytes(message);
                _bluetoothStream.Write(buffer, 0, buffer.Length);
                _bluetoothStream.Flush();
                return true;
            }
            else
            {
                logger.Error("stream is empty.");
                return false;
            }
        }
        public bool SendMessage(JsonCommand jobj)
        {
            string command = JsonConvert.SerializeObject(jobj);
            return SendMessage(command);
        }
    }
}
