using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Src.LocalConsole {
    class SettingsManager {
        private DataGridView view;
        private Settings.NodeConfig config;
        private Dictionary<string, Control> controls;
        Dictionary<string, string> info = new Dictionary<string, string>();
        private Serial serial;
        public SettingsManager(Serial ser, DataGridView grid, Dictionary<string, Control> ctrls) {
            config = new Settings.NodeConfig();
            view = grid;
            controls = ctrls;
            serial = ser;

            InitControls();
        }
        public void Parse(string data) {
            if (String.IsNullOrEmpty(data)){
                return;
            }

            List<string> state = LookupState(data.Split(',').ToList());

            if (state == null) {
                return;
            }else if(state.Count < 2) {
                return;
            }

            string key = state[0];
            string value = state[1];

            if (info.ContainsKey(key)) {
                info[key] = value;
            } else {
                info.Add(key, value);
            }

            this.CopyToTable(this.info);
        }

        private void CopyToTable(Dictionary<string, string> src) {
            this.view.Rows.Clear();
            foreach (KeyValuePair<string, string> entry in src) {
                view.Rows.Add(entry.Key, entry.Value);
            }
            this.view.Sort(this.view.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
        }
        private ComboBox GetCommandClassBox() {
            return controls["class"] as ComboBox;
        }
        private ComboBox GetCommandBox() {
            return controls["command"] as ComboBox;
        }
        private ComboBox GetValueBox() {
            return controls["value"] as ComboBox;
        }
        private Button GetSubmitButton() {
            return controls["submit"] as Button;
        }
        private void InitControls() {
            ComboBox classBox = GetCommandClassBox();
            ComboBox commandBox = GetCommandBox();
            ComboBox valueBox = GetValueBox();
            Button submitButton = GetSubmitButton();

            InitClassBox(classBox);
            InitCommandBox(commandBox, classBox.Text);
            InitValueBox(valueBox, classBox.Text, commandBox.Text);
            InitSubmitButton(submitButton);
        }
        private void InitSubmitButton(Button button) {
            button.Click += SubmitButtonClick;
        }
        private void InitClassBox(ComboBox box) {
            box.DataSource = config.GetCommandClassList();
            box.Enabled = true;
            box.DropDownStyle = ComboBoxStyle.DropDownList;
            box.SelectedIndexChanged += ClassBoxSelectedIndexChanged;
        }
        private void InitCommandBox(ComboBox box, string commandClass) {
            box.DataSource = config.GetCommands(commandClass);
            box.Enabled = true;
            box.DropDownStyle = ComboBoxStyle.DropDownList;
            box.SelectedIndexChanged += CommandBoxSelectedIndexChanged;
        }
        private void InitValueBox(ComboBox box, string commandClass, string command) {            
            box.DataSource = config.GetValues(commandClass, command);

            switch(config.GetValueType(commandClass, command)){
                case Settings.ValueType.None:
                    box.Enabled = false;
                    break;
                case Settings.ValueType.Int:
                case Settings.ValueType.Float:
                    box.Enabled = true;
                    box.DropDownStyle = ComboBoxStyle.Simple;
                    break;
                case Settings.ValueType.Set:
                    box.Enabled = true;
                    box.DropDownStyle = ComboBoxStyle.DropDownList;
                    break;
                default:
                    box.Enabled = false;                    
                    break;
            };
        }
        private void ClassBoxSelectedIndexChanged(object sender, EventArgs e) {
            ComboBox commandBox = GetCommandBox();
            ComboBox classBox = GetCommandClassBox();
            
            InitCommandBox(commandBox, classBox.Text);
        }
        private void CommandBoxSelectedIndexChanged(object sender, EventArgs e) {
            ComboBox commandBox = GetCommandBox();
            ComboBox classBox = GetCommandClassBox();
            ComboBox valueBox = GetValueBox();

            InitValueBox(valueBox, classBox.Text, commandBox.Text);
        }
        private void SubmitButtonClick(object sender, EventArgs e) {
            if (serial.IsOpen()) {
                ComboBox commandBox = GetCommandBox();
                ComboBox classBox = GetCommandClassBox();
                ComboBox valueBox = GetValueBox();

                string commandString = config.GetSerialCommand(classBox.Text, commandBox.Text, valueBox.Text);              

                serial.WriteData(commandString);
                serial.WriteData(config.GetPollCommand());
            }
        }

        private List<string> LookupState(List<string> data) {
            if(data == null) {
                return null;
            }
            if(data.Count < 5) {
                return null;
            }
            if (data[1] != "State") {
                return null;
            }

            var SystemState = new Dictionary<string, string>() {
                { "3", "OperationMode"},
                { "4", "DetectDayVolts"},
                { "5", "StopRadioAndGps"},
            };
            var GpsState = new Dictionary<string, string>() {
                { "2", "GpsTimeout"},
                { "3", "GpsOnTime"},
                { "4", "GpsInterval"},
                { "5", "GpsEnabled"},
            };
            var PowerState = new Dictionary<string, string>() {
                { "2", "GpsCutoffVoltage"},
                { "3", "GpsResumeVoltage"},
                { "4", "RadioCutoffVoltage"},
                { "5", "RadioResumeVoltage"},
                { "6", "LedCutoffVoltage"},
                { "7", "LedResumeVoltage"}
            };
            var RadioState = new Dictionary<string, string>() {
                { "2", "RadioRelayWatermark"},
                { "3", "RadioRelayInterval"},
                { "4", "RadioHealthInterval"},
                { "5", "RadioTxFrequency"},
                { "6", "RadioTxPower"}
            };
            var state = new Dictionary<string, Dictionary<string, string>>() {
                { "3", SystemState},
                { "4", GpsState},
                { "7", RadioState},
                { "6", PowerState}, 
            };

            // data[0] - time
            // data[1] - State
            // data[2] - commandClass
            // data[3] - command
            // data[4] - value

            string commandClass = data[2];
            string command = data[3];
            string value = data[4];

            if (state.ContainsKey(commandClass)) {
                if (state[commandClass].ContainsKey(command)) {
                    string key = state[commandClass][command];
                    return new List<string>() {key, value};
                }
            }

            return null;
        }


    }
}
