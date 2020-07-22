using System.Diagnostics;
using System.Windows.Forms;
using System;
using System.Collections.Generic;

namespace Src.LocalConsole {
    class TagDetections {
        private DataGridView view;
        public TagDetections(DataGridView grid) {
            view = grid;
        }

        public void Parse(string data) {

            CommandParser parser = new CommandParser(data);
            Dictionary<string, string> detection = parser.GetData();
            List<string> requiredKeys = new List<string> {
                "time",
                "key",
                "id",
                "rssi"
            };

            if (CommandParser.Verify(detection, requiredKeys) == false) {
                return;
            } else if (detection["key"].Equals("Beep") == false) {
                return;
            }

            // 2020-04-03 16:53:12,Beep,6166002a,-28

            try {

                if (view.Rows.Count > 1000) {
                    for (int i = 0; i < 900; i++) {
                        view.Rows.RemoveAt(0);
                    }
                }

                DateTime now = DateTime.Parse(detection["time"]).ToLocalTime();
                string tagId = detection["id"];
                int rssi = Convert.ToInt16(detection["rssi"]);

                this.view.FirstDisplayedScrollingRowIndex =
                    this.view.Rows.Add(now, tagId, rssi);
            }catch(Exception ex) {
                Console.WriteLine(ex);
            }
        }
    }
}
