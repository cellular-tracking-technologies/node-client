using LocalConsole.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Src.LocalConsole {
    class SettingsManager {

        private DataGridView view;
        private Dictionary<string, Control> controls;
        private NodeSettings settings = new NodeSettings();
        private Serial serial;
        public SettingsManager(Serial ser, DataGridView grid, Dictionary<string, Control> ctrls) {
            view = grid;
            controls = ctrls;
            serial = ser;

            InitControls();
        }
        /// <summary>
        /// Parses incoming data related to node settings. When a setting
        /// is polled, the information gets saved in a local dictionary 
        /// then added to a table for viewing.
        /// </summary>
        /// <param name="data"></param>
        public void Parse(string data) {
            if (String.IsNullOrEmpty(data)){
                return;
            }

            /*
             * Parse incoming node message into a setting name and
             * setting value.
             * 
             * state[0] = name
             * state[1] = value
             */

            Setting setting = ParseStateMessage(data.Split(',').ToList());

            if (setting == null) {
                return;
            }

            UpdateTable(setting);
        }
        private void UpdateTable(Setting s) {
            object[] rowToAdd = new object[] {
                    s.ReadableName,
                    s.Value,
                    s.Units,
                    node_client.Properties.Resources.gear
            };

            foreach (DataGridViewRow row in view.Rows) {
                if (row.Cells[0].Value.ToString().Equals(s.ReadableName)) {
                    row.SetValues(rowToAdd);
                    return;
                }
            }

            view.Rows.Add(rowToAdd);
            view.Sort(this.view.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
        }
        private void InitControls() {

            view.CellClick += GridCellClick;

            (controls["refresh"] as Button).Click += (object sender, EventArgs e) => {
                if (serial.IsOpen()) {
                    serial.WriteData(settings.GetPollCommand());
                }
            };
            (controls["save"] as Button).Click += (object sender, EventArgs e) => {
                if (serial.IsOpen()) {
                    serial.WriteData(settings.GetSaveCommand());
                }
            };
        }
        private Setting ParseStateMessage(List<string> data) {
            // State Message Format
            // Time,State,CommandClass,Command,Value

            if (data == null) {
                return null;
            }else if (data.Count < 5) {
                return null;
            }else if (data[1] != "State") {
                return null;
            }

            Console.WriteLine(String.Format("{0} {1} {2} {3} {4}\r\n", 
                data[0],data[1],data[2],data[3],data[4]));

            string commandClass = data[2];
            string command = data[3];
            string value = data[4];

            foreach(Setting s in settings.settings) {
                // Find setting based on Id and command_class
                if(s.Id.Equals(commandClass) == false) {
                    continue;
                }else if(s.Command.Equals(command) == false) {
                    continue;
                }

                s.Value = s.InFormat(value);
                return s;
            }
            return null;
        }

        void GridCellClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0) {
                return;
            }

            DataGridView grid = sender as DataGridView;
           
            switch (e.ColumnIndex) {
                case 3:

                    Setting setting = settings.Get(grid.Rows[e.RowIndex].Cells[0].Value.ToString());
                    if (setting != null) {
                        node_client.Forms.SettingUpdate f = new node_client.Forms.SettingUpdate(serial, setting);
                        f.Show();
                    }

                    break;
                default:
                    break;
            }

            grid.ClearSelection();
            grid.CurrentCell = null;
        }
    }
}
