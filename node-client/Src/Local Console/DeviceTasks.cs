using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Src.LocalConsole {
    class DeviceTasks {
        static public string IdCommand() {
            return "id\r\n";
        }
        static public string GpsFixCommand() {
            return "gps:6,1\r\n";
        }
        static public string HealthCommand() {
            return "radio:8,1\r\n";
        }
        static public string RelayCommand() {
            return "radio:7,1\r\n";
        }
        static public string DynamicsCommand() {
            return "acc:9,1\r\n";
        }
        static public string DisableRadioAndGps(bool disable) {
            if (disable) {
                return "system:5,1\r\n";
            } else {
                return "system:5,0\r\n";
            }
        }
    }
}
