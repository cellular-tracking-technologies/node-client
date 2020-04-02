using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows.Forms;

namespace DeviceIo {
    public class Serial {

        SerialPort serial = new SerialPort();
        private Queue<string> buffer = new Queue<string>();
        System.Windows.Forms.Timer DataAvailableTimer = new System.Windows.Forms.Timer();

        public event EventHandler DataAvailableEvent;

        public Serial() {
            Init();
        }

        private void Init() {            
            serial.DataReceived += new SerialDataReceivedEventHandler(DataReceivedCallback);
        }

        public bool ToggleSerialState(string port, int baud, Handshake handshake, bool dtr_high) {
            if (this.IsOpen()) {
                this.Close();
                return false;
            } else if (String.IsNullOrEmpty(port)) {
                this.Close();
                return false;
            }

            this.Configure(port, baud, handshake, dtr_high);
            return this.Open();
        }

        public bool IsOpen() {
            return serial.IsOpen;
        }
        public bool Open() {
            if (serial.IsOpen) {
                return true;
            } else {
                try {
                    serial.Open();
                } catch (UnauthorizedAccessException) {
                    return false;
                } catch (IOException) {
                    return false;
                } catch (ArgumentException) {
                    return false;
                }
            }

            DataAvailableTimer.Stop();
            DataAvailableTimer.Interval = 10;
            DataAvailableTimer.Tick += new EventHandler(CheckData);
            DataAvailableTimer.Start();

            return true;
        }

        public void Close() {
            try {
                if (serial.IsOpen) {
                    serial.Close();
                    DataAvailableTimer.Stop();
                }
            } catch (System.UnauthorizedAccessException e) {
                Debug.WriteLine(e);
            }
        }

        public bool Configure(string port, int baud, Handshake handshake, bool dtr_high) {

            bool kPortState = IsOpen();

            if (kPortState) {
                Close();
            }

            serial.PortName = port;
            serial.BaudRate = baud;

            serial.DataBits = 8;
            serial.StopBits = StopBits.One;
            serial.Parity = Parity.None;
            serial.Handshake = handshake;
            serial.DtrEnable = dtr_high;

            if (kPortState) {
                return Open();
            }

            return true;
        }

        public bool DataAvailable() {
            return buffer.Count > 0;
        }

        public string GetData() {
            if (DataAvailable()) {
                return buffer.Dequeue();
            } else {
                return "";
            }
        }
        public bool WriteData(string data) {
            try {
                if (serial.IsOpen) {
                    byte[] bytes = Encoding.ASCII.GetBytes(data);
                    serial.Write(bytes, 0, bytes.Length);
                    return true;
                }
            } catch (System.UnauthorizedAccessException e) {
                Debug.WriteLine(e + " - " + "Device may have been removed during serial writing.");
                Debug.WriteLine("serial.IsOpen: " + serial.IsOpen);
            } catch (Exception e) {
                Debug.WriteLine(e + "Unknown serial communication error occurred.");
            }
            return false;
        }

        static void DelayMicroseconds(long us) {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long v = (us * System.Diagnostics.Stopwatch.Frequency) / 1000000;
            while (sw.ElapsedTicks < v) {
            }
        }

        public void WriteDataSlowly(string data, Int32 delay_us) {
            try {
                if (serial.IsOpen) {
                    byte[] bytes = Encoding.ASCII.GetBytes(data);
                    foreach (char character in data) {
                        char[] characterArray = new char[] { character };
                        serial.Write(characterArray, 0, 1);
                        DelayMicroseconds(delay_us);
                    }
                }
            } catch (System.UnauthorizedAccessException e) {
                Debug.WriteLine(e + " - " + "Device may have been removed during serial writing.");
                serial.Close();
            }
        }

        private void DataReceivedCallback(object sender, SerialDataReceivedEventArgs e) {
            if (!serial.IsOpen) {
                return;
            }

            string data = serial.ReadExisting();
            if (data.Length > 0) {
                buffer.Enqueue(data);
            }
        }
        private void CheckData(object Sender, EventArgs e) {
            if (DataAvailable()) {
                TriggerCallback(e);
            }
        }
        private void TriggerCallback(EventArgs e) {
            DataAvailableEvent?.Invoke(this, e);
        }
        static public string[] GetSortedAvailablePorts() {
            // Order the serial port names in numberic order (if possible)
            int num;
            return SerialPort.GetPortNames().OrderBy(a => a.Length > 3 && int.TryParse(a.Substring(3), out num) ? num : 0).ToArray();
        }

        public string GetPortString(IEnumerable<string> PreviousPortNames, string CurrentSelection) {

            string selected = null;
            string[] ports = SerialPort.GetPortNames();

            // Was there any port additions or removals
            bool updated = PreviousPortNames.Except(ports).Count() > 0 || ports.Except(PreviousPortNames).Count() > 0;

            if (updated) { // select an appropriate default port

                ports = GetSortedAvailablePorts();

                // Find newest port if one or more were added
                string newest = SerialPort.GetPortNames().Except(PreviousPortNames).OrderBy(a => a).LastOrDefault();

                // If the port was already open...
                if (IsOpen()) {
                    if (ports.Contains(CurrentSelection)) {
                        selected = CurrentSelection;
                    } else if (!String.IsNullOrEmpty(newest)) {
                        selected = newest;
                    } else {
                        selected = ports.LastOrDefault();
                    }
                } else {
                    if (!String.IsNullOrEmpty(newest)) {
                        selected = newest;
                    } else if (ports.Contains(CurrentSelection)) {
                        selected = CurrentSelection;
                    } else {
                        selected = ports.LastOrDefault();
                    }
                }
            }
            // If there was a change to the port list, return the recommended default selection
            return selected;
        }
    }
}
