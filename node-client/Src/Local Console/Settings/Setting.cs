using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalConsole.Settings {
    public class Setting {
        public string Tag { get; set; }
        public string Id { get; set; }
        public string Command { get; set; }
        public string Value { get; set; }
        public string ReadableName { get; set; }

        public Func<string, string> InFormat = null;
        public Func<string, string> OutFormat = null;
        public List<string> OutputOptions = null;
        public string Units { get; set; }

        public Setting() {

        }
        public string GetUpdateCommand(string value) {
            return String.Format("{0}:{1},{2}\r\n", Tag, Command, OutFormat(value));
        }
    }

    public class NodeSettings {
        public List<Setting> settings;       
        public NodeSettings() {

            Dictionary<string, string> operationMode = new Dictionary<string, string>() {
                ["0"] = "Day",
                ["1"] = "Night",
                ["2"] = "Day+Night",
            };

            Dictionary<string, string> logMode = new Dictionary<string, string>() {
                ["0"] = "Disabled",
                ["1"] = "Periodic",
                ["2"] = "Watermark",
                ["3"] = "Periodic + Watermark",
            };

            settings = new List<Setting>() {

                    // System Settings
                    new Setting(){Tag="system",Id = "3", Command = "4", ReadableName = "DetectDayVolts", Units="Volts", InFormat = FormatInputAsVoltage, OutFormat = FormatOutputVoltage},
                    new Setting(){Tag="system",Id = "3", Command = "5", ReadableName = "StopRadioAndGps", Units="Boolean", InFormat = DefaultFormat, OutFormat = DefaultFormat},
                    new Setting(){Tag="system",Id = "3", Command = "3", ReadableName = "OperationMode", Units="Option", InFormat = (input) => {
                        if (operationMode.ContainsKey(input)) {
                            return operationMode[input];
                        }
                        return "Unknown";
                    }, OutFormat = (input) => {
                        return GetKeyByValue(input, operationMode);
                    }, OutputOptions = operationMode.Values.ToList()},

                    // Gps Settings
                    new Setting(){Tag="gps",Id="4",Command="2",ReadableName="GpsTimeout",Units="Seconds",InFormat=FormatInputAsSeconds,OutFormat=FormatOutputAsMs},
                    new Setting(){Tag="gps",Id="4",Command="3",ReadableName="GpsOnTime",Units="Seconds",InFormat=FormatInputAsSeconds,OutFormat=FormatOutputAsMs},
                    new Setting(){Tag="gps",Id="4",Command="4",ReadableName="GpsInterval",Units="Seconds",InFormat=FormatInputAsSeconds,OutFormat=FormatOutputAsMs},
                    new Setting(){Tag="gps",Id="4",Command="5",ReadableName="GpsEnabled",Units="Boolean",InFormat=DefaultFormat,OutFormat=DefaultFormat},

                    // Power Settings
                    new Setting(){Tag="power",Id = "6", Command = "2", ReadableName = "GpsCutoffVoltage", Units="Volts", InFormat = FormatInputAsVoltage, OutFormat = FormatOutputVoltage},
                    new Setting(){Tag="power",Id = "6", Command = "3", ReadableName = "GpsResumeVoltage", Units="Volts", InFormat = FormatInputAsVoltage, OutFormat = FormatOutputVoltage},
                    new Setting(){Tag="power",Id = "6", Command = "4", ReadableName = "RadioCutoffVoltage", Units="Volts", InFormat = FormatInputAsVoltage, OutFormat = FormatOutputVoltage},
                    new Setting(){Tag="power",Id = "6", Command = "5", ReadableName = "RadioResumeVoltage", Units="Volts", InFormat = FormatInputAsVoltage, OutFormat = FormatOutputVoltage},
                    new Setting(){Tag="power",Id = "6", Command = "6", ReadableName = "LedCutoffVoltage", Units="Volts", InFormat = FormatInputAsVoltage, OutFormat = FormatOutputVoltage},
                    new Setting(){Tag="power",Id = "6", Command = "7", ReadableName = "LedResumeVoltage", Units="Volts", InFormat = FormatInputAsVoltage, OutFormat = FormatOutputVoltage},

                    // Radio Settings
                    new Setting(){Tag="radio",Id = "7", Command = "2", ReadableName = "RadioRelayWatermark", Units="Detections", InFormat = DefaultFormat, OutFormat = DefaultFormat},
                    new Setting(){Tag="radio",Id = "7", Command = "3", ReadableName = "RadioRelayInterval", Units="Seconds", InFormat = FormatInputAsSeconds, OutFormat=FormatOutputAsMs},
                    new Setting(){Tag="radio",Id = "7", Command = "4", ReadableName = "RadioHealthInterval", Units="Seconds",  InFormat = FormatInputAsSeconds, OutFormat=FormatOutputAsMs},
                    new Setting(){Tag="radio",Id = "7", Command = "5", ReadableName = "RadioTxFrequency", Units="Hz", InFormat = DefaultFormat, OutFormat = DefaultFormat},
                    new Setting(){Tag="radio",Id = "7", Command = "6", ReadableName = "RadioTxPower", Units="dBm", InFormat = DefaultFormat, OutFormat = DefaultFormat},
                    new Setting(){Tag="radio",Id = "7", Command = "9", ReadableName = "RadioEnabled", Units="Boolean", InFormat = DefaultFormat, OutFormat = DefaultFormat},

                    // Acc Settings
                    new Setting(){Tag="acc",Id = "9", Command = "3", ReadableName = "AccDataRate", Units="Hz", InFormat = DefaultFormat, OutFormat = DefaultFormat},
                    new Setting(){Tag="acc",Id = "9", Command = "4", ReadableName = "AccDataRange", Units="+/- Gees", InFormat = DefaultFormat, OutFormat = DefaultFormat},
                    new Setting(){Tag="acc",Id = "9", Command = "5", ReadableName = "AccActivityThreshold", Units="milli-Gees", InFormat = DefaultFormat, OutFormat = DefaultFormat},
                    new Setting(){Tag="acc",Id = "9", Command = "6", ReadableName = "AccActivityWatermark", Units="Detections", InFormat = DefaultFormat, OutFormat = DefaultFormat},
                    new Setting(){Tag="acc",Id = "9", Command = "7", ReadableName = "AccSnapshotInterval", Units="Seconds", InFormat = FormatInputAsSeconds, OutFormat=FormatOutputAsMs},
                    new Setting(){Tag="acc",Id = "9", Command = "8", ReadableName = "AccLogMode", Units="Option", InFormat = (input) => {
                        if (logMode.ContainsKey(input)) {
                            return operationMode[input];
                        }
                        return "Unknown";
                    }, OutFormat = (input) => {
                        return GetKeyByValue(input, logMode);
                    }, OutputOptions = logMode.Values.ToList()}
            };
        }
        public string GetPollCommand() {
            string ret = "";
            ret += "gps:8,0\r\n";
            ret += "radio:12,0\r\n";
            ret += "system:6,0\r\n";
            ret += "power:13,0\r\n";
            ret += "acc:2,0\r\n";
            return ret;
        }

        public string GetSaveCommand() {
            string ret = "";
            ret += "gps:1,0\r\n";
            ret += "radio:1,0\r\n";
            ret += "system:2,0\r\n";
            ret += "power:1,0\r\n";
            ret += "acc:1,0\r\n";
            return ret;
        }

        public Setting Get(string readableName) {

            Setting ret = null;

            if (String.IsNullOrEmpty(readableName)) {
                return ret;
            }

            settings.ForEach((s) => {
                if (readableName.Equals(s.ReadableName)) {
                    ret = s;
                }
            });

            return ret;
        }

        static string GetKeyByValue(string input, Dictionary<string,string> d) {
            foreach (KeyValuePair<string, string> element in d) {
                if (element.Value.Equals(input)) {
                    return element.Key.ToString();
                }
            }
            return null;
        }

        static string DefaultFormat(string input) {
            return input;
        }
        static string FormatInputAsVoltage(string input) {
            return Math.Round((Convert.ToDouble(input) / 100), 2).ToString();
        }
        static string FormatInputAsSeconds(string ms) {
            return (Convert.ToUInt32(ms) / 1000).ToString();
        }
        static string FormatOutputAsMs(string ms) {
            return (Convert.ToUInt32(ms) * 1000).ToString();
        }
        static string FormatOutputVoltage(string input) {
            return Math.Round((Convert.ToDouble(input) * 1000), 2).ToString();
        }
    }
}
