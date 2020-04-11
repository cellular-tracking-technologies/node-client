using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Src.LocalConsole {
    class DeviceTasks {
        static public string IdCommand() {
            return "id" + Environment.NewLine;
        }
        static public string GpsFixCommand() {
            return "gps:6,1" + Environment.NewLine;
        }
        static public string HealthCommand() {
            return "radio:8,1" + Environment.NewLine;
        }
        static public string RelayCommand() {
            return "radio:7,1" + Environment.NewLine;
        }
    }
}
