using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Src.GridHealth {
    class Manager {
        LineBuilder healthLineManager;
        private DataGridView view;
        List<NodeStatus> nodes = new List<NodeStatus>();

        public Manager(DataGridView grid) {
            view = grid;
            this.healthLineManager = new LineBuilder();
        }
        public void Init() {
            this.view.Rows.Clear();
        }
        public void IngestHealthData(string data) {

            Debug.WriteLine(data);

            List<Action<string>> parsers = new List<Action<string>>();
            parsers.Add(ParseNodeHealth);

            healthLineManager.Ingest(data, parsers);
        }
        public void Update() {
            DataGridView table = this.view;

            table.Rows.Clear();
            foreach (NodeStatus node in nodes) {
                table.Rows.Add(
                    node.Serial,
                    node.LastHealth.ToLocalTime(),
                    node.Rssi,
                    node.Firmware,
                    node.Battery,
                    node.DeltaBattery,
                    node.SolarVoltage,
                    node.TotalSolarCurrent,
                    node.FixTime.ToLocalTime(),
                    node.Latitude,
                    node.Longitude,
                    0 // beeps
                    );

                Int32 rowIndex = table.Rows.Count - 1;

                //UInt64 staleHealthSeconds = Convert.ToUInt64(tbTestStaleHealth.Text);
                //table.Rows[rowIndex].Cells["RxLastHealth"].Style.BackColor = Src.ReceiverMarkup.StaleDateTimeColor(node.LastHealth, staleHealthSeconds);

                //int rssiThresholdDbm = Convert.ToInt32(tbTestRssiThreshold.Text);
                //table.Rows[rowIndex].Cells["RxRssiAvg"].Style.BackColor = Src.ReceiverMarkup.GetRssiColor(node.Rssi, rssiThresholdDbm);

                //double batteryThresholdVolts = Convert.ToDouble(tbTestBatteryVolts.Text);
                //table.Rows[rowIndex].Cells["RxBattery"].Style.BackColor = Src.ReceiverMarkup.GetBatteryColor(node.Battery, batteryThresholdVolts);

                //UInt64 staleFixSeconds = Convert.ToUInt64(tbTestStaleFix.Text);
                //table.Rows[rowIndex].Cells["RxFixAt"].Style.BackColor = Src.ReceiverMarkup.StaleDateTimeColor(node.FixTime, staleFixSeconds);

            }
        }
        private void ParseNodeHealth(string data) {
            if (data.Contains("node_health") == false) {
                return;
            }

            NodeStatus node = new NodeStatus(JsonConvert.DeserializeObject(data));

            if (this.Exists(node)) {
                this.Update(node);
            } else {
                this.Create(node);
            }
        }
        private bool Exists(NodeStatus node) {
            foreach (NodeStatus n in nodes) {
                if (n.Serial.Equals(node.Serial)) {
                    return true;
                }
            }
            return false;
        }
        private void Update(NodeStatus node) {
            foreach (NodeStatus n in nodes) {
                if (n.Serial.Equals(node.Serial)) {
                    n.Update(node);
                    return;
                }
            }
        }
        private void Create(NodeStatus node) {
            nodes.Add(node);
        }        
    }
}
