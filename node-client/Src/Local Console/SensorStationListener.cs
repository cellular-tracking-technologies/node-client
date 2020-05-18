using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Src.LocalConsole {
    class SensorStationListener {
        private TextBox box_;
        public SensorStationListener(TextBox box) {
            box_ = box;
        }
        public void Parse(string data) {
            if (data.Contains(",SS,")) {
                box_.Text += data + Environment.NewLine;
            }
        }
    }
}
