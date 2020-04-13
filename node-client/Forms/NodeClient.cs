using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace node_client {
    public partial class NodeClient : Form {
    
        /* todo build serial port manager class.
         * Eventual goal is to allow multiple receivers
         * to be controlled by a software basestation.
        */
        Src.Serial serialConsole;
        Src.Serial serialTransceiver;

        // todo wrap console related stuff into managerclass
        Src.LineBuilder consoleDataManager;
        Src.LocalConsole.TagDetections beeps;
        Src.LocalConsole.DeviceInfo info;
        Src.LocalConsole.SettingsManager settings;

        Src.FileTransfer.NodeDir dir;

        Src.GridHealth.Manager healthManager;

        public NodeClient() {
            InitializeComponent();

            this.serialConsole = new Src.Serial();
            this.consoleDataManager = new Src.LineBuilder();

            this.serialTransceiver = new Src.Serial();

            this.beeps = new Src.LocalConsole.TagDetections(dataGridDetections);
            this.info = new Src.LocalConsole.DeviceInfo(dataGridDeviceInfo);

            Dictionary<string, Control> settingControls = new Dictionary<string, Control>() {
                {"class", comboBoxCategory},
                {"command", comboBoxSettings},
                {"value", comboBoxValue},
                {"submit", buttonSubmit}
            };

            this.settings = new Src.LocalConsole.SettingsManager(serialConsole, dataGridSettings, settingControls);

            this.healthManager = new Src.GridHealth.Manager(this.dataGridHealth);

            this.Icon = Properties.Resources.ctt_logo_icon;
            tabControlMain.DrawItem += new DrawItemEventHandler(TabControlDrawItem);

            dir = new Src.FileTransfer.NodeDir(serialConsole, dataGridDirectory);

            this.InitTabUsb();
            InitAboutTab();
            this.healthManager.Init();
            
            foreach(Control c in groupBoxLocaleSettingsUpdate.Controls) {
                Console.WriteLine(String.Format("{0} {1}", c.Name, c.GetType()));
            }                                 
        }
        private void UpdatePortBoxes() {
            this.ComListToComboBox(serialConsole, comboBoxPort1);
            this.ComListToComboBox(serialTransceiver, comboBoxPort2);
        }
        private void InitTabUsb() {
            this.UpdatePortBoxes();

            List<string> baudRates = new List<string> { "115200", "57600", "9600", "4800" };
            List<string> handshakeModes = new List<string> {
                "None",
                "xOnXOff",
                "RequestToSendXOnXOff",
                "RequestToSend"
            };

            this.PopulateListBox(comboBoxHandshake, handshakeModes);
            this.PopulateListBox(comboBoxBaud1, baudRates);
            this.PopulateListBox(comboBoxBaud2, baudRates);

            // Adafruit Feather 32u4 (Device typically used for transceiver) requires
            // the DTR line to be set HIGH before it will transmit data over the USB bus.
            this.checkBoxDtr.Checked = true;

            serialConsole.DataAvailableEvent += SerialConsoleAvailable;
            serialTransceiver.DataAvailableEvent += SerialTransceiverAvailable;
        }
        private void InitAboutTab() {
            labelVersion.Text = String.Format("Version:{0}", Application.ProductVersion);
            labelCompany.Text = String.Format("Company:{0}", Application.CompanyName);
            labelProduct.Text = String.Format("Product:{0}", Application.ProductName);
        }
        /// <summary>
        /// Draws tabs of tabControl horizontally, applies color and text. This function assumes
        /// tabControl.Alignment {Left|Right} and tabControl.DrawMode {OwnerDrawFixed}.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControlDrawItem(object sender, DrawItemEventArgs e) {
            TabControl control = sender as TabControl;
            TabPage tabPage = control.TabPages[e.Index];
            Font tabFont = new Font("Arial", 12.0f, FontStyle.Bold, GraphicsUnit.Pixel);
            Rectangle tabBounds = control.GetTabRect(e.Index);

            Brush textBrush;
            Graphics g = e.Graphics;
            if (e.State == DrawItemState.Selected) {
                textBrush = new SolidBrush(e.ForeColor);
                g.FillRectangle(Brushes.Gray, e.Bounds);
            } else {
                textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            StringFormat stringFlags = new StringFormat();
            stringFlags.Alignment = StringAlignment.Center;
            stringFlags.LineAlignment = StringAlignment.Center;

            g.DrawString(tabPage.Text, tabFont, textBrush, tabBounds, new StringFormat(stringFlags));
        }
        /// <summary>
        /// Retrieves a list of available COM ports and inserts them into a ComboBox. An open port
        /// remains selected on subsequent calls (unless disconnected).
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="box"></param>
        private void ComListToComboBox(Src.Serial serial, ComboBox box) {
            // Determine if the list of com port names has changed since last checked
            string selected = serial.GetPortString(box.Items.Cast<string>(),
                box.SelectedItem as string);

            // If there was an update, then update the control showing the user the list of port names
            if (!String.IsNullOrEmpty(selected)) {
                box.Items.Clear();
                box.Items.AddRange(Src.Serial.GetSortedAvailablePorts());
                box.SelectedItem = selected;
            } else if (Src.Serial.GetSortedAvailablePorts().Count() == 0) {
                // If there are no more ports available, clear the list
                box.Items.Clear();
                box.Text = "Select a Port";
            }
            box.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        public void PopulateListBox(ComboBox box, List<string> data) {
            box.DataSource = data;
            box.Enabled = data.Count != 0;
            box.DropDownStyle = ComboBoxStyle.DropDownList;
            box.SelectedIndex = 0;
        }
        private bool OpenPortClickCallback(Button click, Src.Serial serial,
            string port, string baud, Handshake handshake, bool dtrHigh) {
            if (String.IsNullOrEmpty(port)) {
                return false;
            } else if (String.IsNullOrEmpty(baud)) {
                return false;
            }

            if (serial.ToggleSerialState(port, int.Parse(baud), handshake, dtrHigh)) {
                click.Text = "Close";
            } else {
                click.Text = "Open";
            }
            return serial.IsOpen();
        }
        private void DisplayPortError(string port) {
            MessageBox.Show(String.Format("Check Connection Settings under USB menu!{0}Selected Port {1} is closed, busy or no longer available.", Environment.NewLine, port), "Port Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        Handshake GetHandshakeMode(string data) {
            if (String.IsNullOrEmpty(data) == false) {
                var lookupTable = new Dictionary<string, Handshake>(){
                    { Handshake.None.ToString(),                 Handshake.None},
                    { Handshake.XOnXOff.ToString(),              Handshake.XOnXOff},
                    { Handshake.RequestToSendXOnXOff.ToString(), Handshake.RequestToSendXOnXOff},
                    { Handshake.RequestToSend.ToString(),        Handshake.RequestToSend}
                };
                if (lookupTable.ContainsKey(data)) {
                    return lookupTable[data];
                }
            }
            return Handshake.None;
        }
                     
        private void ButtonOpenPort1_Click(object sender, EventArgs e) {
            if (OpenPortClickCallback(sender as Button, 
                this.serialConsole,
                this.comboBoxPort1.Text, 
                this.comboBoxBaud1.Text, 
                Handshake.RequestToSendXOnXOff, false) == true) {
            } else {
                // todo close
            }
        }

        private void ButtonOpenPort2_Click(object sender, EventArgs e) {
            Handshake handshake = GetHandshakeMode(comboBoxHandshake.Text);
            bool dtr = checkBoxDtr.Checked;

            if (OpenPortClickCallback(sender as Button,
                this.serialTransceiver,
                this.comboBoxPort2.Text,
                this.comboBoxBaud2.Text,
                handshake, dtr) == true) {
                this.serialTransceiver.WriteData("preset:node3" + Environment.NewLine);
            } else {
                // todo close
            }
        }
        private void SerialConsoleAvailable(object sender, EventArgs e) {
            Src.Serial serial = sender as Src.Serial;
            if (serial.DataAvailable()) {
                ProcessConsoleData(serial.GetData());
            }
        }
        private void SerialTransceiverAvailable(object sender, EventArgs e) {
            Src.Serial serial = sender as Src.Serial;
            if (serial.DataAvailable()) {
                string data = serial.GetData();
                this.healthManager.IngestHealthData(data);
            }
        }
      
        private void ProcessConsoleData(string data) {
            List<Action<string>> parsers = new List<Action<string>>();

            parsers.Add(this.dir.Parse);
            parsers.Add(this.beeps.Parse);
            parsers.Add(this.info.Parse);
            parsers.Add(this.settings.Parse);

            consoleDataManager.Ingest(data, parsers);
        }

        private void ButtonUpdatePorts_Click(object sender, EventArgs e) {
            this.UpdatePortBoxes();
        }

        private void LinkLabelContribute_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            VisitLink(sender as LinkLabel, "https://github.com/cellular-tracking-technologies/node-client");
        }

        private void LinkLabelCompany_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            VisitLink(sender as LinkLabel, "https://celltracktech.com/");
        }
        private void VisitLink(LinkLabel link, string url) {
            try {
                link.LinkVisited = true;
                System.Diagnostics.Process.Start(url);
            } catch {

            }
        }
        private void ButtonGetId_Click(object sender, EventArgs e) {
            this.CommandRequest(Src.LocalConsole.DeviceTasks.IdCommand());
        }
        private void ButtonGetFix_Click(object sender, EventArgs e) {
            this.CommandRequest(Src.LocalConsole.DeviceTasks.GpsFixCommand());
        }
        private void ButtonDoHealth_Click(object sender, EventArgs e) {
            this.CommandRequest(Src.LocalConsole.DeviceTasks.HealthCommand());
        }
        private void ButtonDoRelay_Click(object sender, EventArgs e) {
            this.CommandRequest(Src.LocalConsole.DeviceTasks.RelayCommand());
        }
        private void CommandRequest(string command) {
            if (String.IsNullOrEmpty(command)) {
                return;
            }else if (String.IsNullOrWhiteSpace(command)) {
                return;
            }

            if (this.serialConsole.IsOpen()) {
                this.serialConsole.WriteData(command);
            } else {
                this.DisplayPortError(this.comboBoxPort1.Text);
            }
        }

        private void ButtonHealthRefresh_Click(object sender, EventArgs e) {
            if (this.serialTransceiver.IsOpen()) {
                this.healthManager.Update();
            } else {
                this.DisplayPortError(this.comboBoxPort2.Text);
            }
        }

        private void ButtonEmulate_Click(object sender, EventArgs e) {
            string inputToEmulate = textBoxEmulateTag.Text;
            if (String.IsNullOrWhiteSpace(inputToEmulate)) {
                return;
            }
            if(System.Text.RegularExpressions.Regex.IsMatch(inputToEmulate, @"\A\b[0-9a-fA-F]+\b\Z") == true) {
                int command = 10;
                if (checkBoxEmulateCrc.Checked) {
                    command = 11;
                }
                int tag = int.Parse(inputToEmulate, System.Globalization.NumberStyles.HexNumber);
                this.serialConsole.WriteData(String.Format("tag:{0},{1}{2}", command, tag, Environment.NewLine));
            }
        }
        private void ButtonFirmware_Click(object sender, EventArgs e) {
            Src.LocalConsole.FileUpload file = new Src.LocalConsole.FileUpload {
                FileType = "Firmware",
                Extension = ".firmware",
                Command = "firmware",
                Options = "-w",
                Filters = null,
                PreUploadAction = (bool start) => {

                    // Note: This function gets called from a spawned thread, not main!

                    /*
                     * Input parameter tells if the user selected a file, or aborted the process.
                     * If start is true, instruct node to stop radio and gps so it can devote 
                     * resources to downloading the firmware. 
                     */

                    this.serialConsole.WriteData(Src.LocalConsole.DeviceTasks.DisableRadioAndGps(start ? true : false));

                    /*
                     * Give the node 10 seconds to process the request and gracefully shutdown
                     * the receiver and gps. 10 Seconds is probably excessive, but its better
                     * to take a little extra time, then for the firmware download to be 
                     * incomplete, requiring the process to be started over by the user.
                     */

                    if (start) {
                        Thread.Sleep(10000);
                    }
                }
            };

            file.Upload(this.serialConsole);
        }
        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e) {
            TabControl tab = sender as TabControl;
            if (tab.SelectedTab.Name.Equals("tabPageTransfer")) {
            }
        }
    }
}
