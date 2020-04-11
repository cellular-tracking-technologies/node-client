using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Src.LocalConsole {
    class DeviceInfo {

        Dictionary<string, string> info = new Dictionary<string, string>();

        private DataGridView view;
        public DeviceInfo(DataGridView grid) {
            view = grid;
        }
        public void Parse(string data) {
            CommandParser parser = new CommandParser(data);
            Dictionary<string, string> dict = parser.GetData();

            List<string> basicKeys = new List<string> { "time", "key" };

            if (CommandParser.Verify(dict, basicKeys) == false) {
                return;
            } else if (dict["key"].Equals("Power")) {
                List<string> keys = new List<string> {
                    "BatteryVolts", "SolarVolts", "SolarCurrent", "TemperatureCelsius"};
                this.UpdateDict(this.info, dict, keys);
                this.CopyToTable(this.info);
                return;
            } else if (dict["key"].Equals("Gps")) {
                List<string> keys = new List<string> {"Latitude", "Longitude"};
                this.UpdateDict(this.info, dict, keys);
                this.CopyToTable(this.info);
                return;
            } else if (dict["key"].Equals("Id")) {
                List<string> keys = new List<string> { "DeviceId", "Firmware" };
                this.UpdateDict(this.info, dict, keys);
                this.CopyToTable(this.info);
                return;
            } else if (dict["key"].Equals("Sd")) {
                List<string> keys = new List<string> { "SdOk" };
                this.UpdateDict(this.info, dict, keys);
                this.CopyToTable(this.info);
            } else if (dict["key"].Equals("Sun")) {
                List<string> keys = new List<string> { "SunIsUp", "Sunrise", "Sunset" };
                this.UpdateDict(this.info, dict, keys);
                this.CopyToTable(this.info);
                return;
            }
        }
        private void UpdateDict(Dictionary<string, string> dest, 
            Dictionary<string, string> src, List<string> keys) {

            if (CommandParser.Verify(src, keys) == false) {
                return;
            }
            keys.ForEach(delegate (string key) {
                if (dest.ContainsKey(key)){
                    dest[key] = src[key];
                } else {
                    dest.Add(key, src[key]);
                }
            });
        }
        private void CopyToTable(Dictionary<string, string> src) {
            this.view.Rows.Clear();
            foreach (KeyValuePair<string, string> entry in src) {
                view.Rows.Add(entry.Key, entry.Value);
            }
            this.view.Sort(this.view.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
        }
    }
}
