using DeviceIo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace node_client {
    public partial class NodeClient : Form {

        Serial serialConsole = new Serial();
        Serial serialTransceiver = new Serial();

        public NodeClient() {
            InitializeComponent();
            this.Icon = Properties.Resources.ctt_logo_icon;
            tabControlMain.DrawItem += new DrawItemEventHandler(TabControlDrawItem);

            this.InitTabUsb();
            InitAboutTab();
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
        private void ComListToComboBox(Serial serial, ComboBox box) {
            // Determine if the list of com port names has changed since last checked
            string selected = serial.GetPortString(box.Items.Cast<string>(),
                box.SelectedItem as string);

            // If there was an update, then update the control showing the user the list of port names
            if (!String.IsNullOrEmpty(selected)) {
                box.Items.Clear();
                box.Items.AddRange(Serial.GetSortedAvailablePorts());
                box.SelectedItem = selected;
            } else if (Serial.GetSortedAvailablePorts().Count() == 0) {
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
        private bool OpenPortClickCallback(Button click, Serial serial,
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
            MessageBox.Show(String.Format("Error {0} is busy or no longer available.", port), "Port Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        Handshake GetHandshakeMode(string data) {
            if (data.Equals("None")) {
                return Handshake.None;
            }
            if (data.Equals("xOnXOff")) {
                return Handshake.XOnXOff;
            }
            if (data.Equals("RequestToSendXOnXOff")) {
                return Handshake.RequestToSendXOnXOff;
            }
            if (data.Equals("RequestToSend")) {
                return Handshake.RequestToSend;
            }
            return Handshake.None;
        }
        private void ButtonOpenPort1_Click(object sender, EventArgs e) {
            if (OpenPortClickCallback(sender as Button, 
                this.serialConsole,
                this.comboBoxPort1.Text, 
                this.comboBoxBaud1.Text, 
                Handshake.RequestToSendXOnXOff, false) == true) {
                // todo success
            } else {
                this.DisplayPortError(this.comboBoxPort1.Text);
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
                // todo success
            } else {
                this.DisplayPortError(this.comboBoxPort1.Text);
            }
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
    }
}
