using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Src.LocalConsole.Settings {
    public enum ValueType {
        None,
        Int,
        Float,
        Set
    }

    public struct Option {
        public ValueType type;
        public KeyValuePair<string, int> commandClass;
        public KeyValuePair<string, int> command;
        public List<KeyValuePair<string, int>> value;
    }

    class NodeConfig {
        List<Option> options = new List<Option>();
        public NodeConfig() {
            AddGpsCommands();
            AddRadioCommands();
            AddLedCommands();
            AddSystemCommands();
            AddPowerCommands();
        }
        public ValueType GetValueType(string commandClass, string command) {
            Option opt = GetOption(commandClass, command);
            return opt.type;
        }
        private Option GetOption(string commandClass, string command) {
            Option opt = new Option();
            options.ForEach(element => {
                if (commandClass.Equals(element.commandClass.Key)) {
                    if (command.Equals(element.command.Key)) {
                        opt = element;
                    }
                }
            });
            return opt;
        }
        public string GetSerialCommand(string commandClass, string command, string value) {
            Option opt = GetOption(commandClass, command);
            UInt32 valueNumber = 0;

            switch (opt.type) {
                case ValueType.None:
                    break;                
                case ValueType.Int:
                    if (String.IsNullOrEmpty(value)) {break;}
                    if (String.IsNullOrWhiteSpace(value)) { break; }
                    valueNumber = Convert.ToUInt32(value.Trim());
                    break;
                case ValueType.Set:
                    if (opt.value != null) {
                        opt.value.ForEach(element => {
                            if (element.Key.Equals(value)) {
                                valueNumber = Convert.ToUInt32(element.Value);
                            }
                        });
                    }
                    break;
                default:
                    break;
            }

            return String.Format("{0}:{1},{2}{3}",
                opt.commandClass.Key.ToLower(),
                opt.command.Value,
                valueNumber,
                Environment.NewLine);
        }

        public string GetPollCommand() {
            string ret = "";
            ret += "gps:8,0\r\n";
            ret += "radio:12,0\r\n";
            ret += "system:6,0\r\n";
            ret += "power:13,0\r\n";
            return ret;
        }

        public List<string> GetCommandClassList() {
            List<string> classes = new List<string>();
            this.options.ForEach(element => {
                string commandClass = element.commandClass.Key;
                if (classes.Contains(commandClass) == false) {
                    classes.Add(commandClass);
                }
            });
            return classes;
        }
        public List<string> GetCommands(string commandClass) {
            List<string> commands = new List<string>();
            this.options.ForEach(element => {
                if (commandClass.Equals(element.commandClass.Key)) {
                    commands.Add(element.command.Key);
                }
            });
            return commands;
        }
        public List<string> GetValues(string commandClass, string command) {
            List<string> values = new List<string>();
            foreach(Option option in this.options) {
                if (commandClass.Equals(option.commandClass.Key)) {
                    if (command.Equals(option.command.Key)) {                       
                        if (option.value == null) {
                            return null;
                        }
                        option.value.ForEach(element => {
                            values.Add(element.Key);
                        });                                               
                    }
                }
            }          
            return values.Count > 0 ? values : null;
        }
        private void AddSystemCommands() {
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("System", 3),
                command = new KeyValuePair<string, int>("StopRadioAndGps", 5),
                type = ValueType.Set,
                value = new List<KeyValuePair<string, int>>() {
                    new KeyValuePair<string, int>("True", 1),
                    new KeyValuePair<string, int>("False", 0),                    
                }
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("System", 3),
                command = new KeyValuePair<string, int>("InstallMode", 1),
                type = ValueType.Int,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("System", 3),
                command = new KeyValuePair<string, int>("OperationMode", 3),
                type = ValueType.Set,
                value = new List<KeyValuePair<string, int>>() {
                    new KeyValuePair<string, int>("Day Only", 0),
                    new KeyValuePair<string, int>("Night Only", 1),
                    new KeyValuePair<string, int>("Day and Night", 2)
                }
            });
        }
        private void AddGpsCommands() {
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Gps", 4),
                command = new KeyValuePair<string, int>("GetFix", 6),
                type = ValueType.None,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Gps", 7),
                command = new KeyValuePair<string, int>("Timeout", 2),
                type = ValueType.Int,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Gps", 7),
                command = new KeyValuePair<string, int>("OnTime", 3),
                type = ValueType.Int,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Gps", 7),
                command = new KeyValuePair<string, int>("Interval", 4),
                type = ValueType.Int,
                value = null
            });
        }
        private void AddRadioCommands() {
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Radio", 7),
                command = new KeyValuePair<string, int>("Health", 8),
                type = ValueType.None,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Radio", 7),
                command = new KeyValuePair<string, int>("Relay", 7),
                type = ValueType.None,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Radio", 7),
                command = new KeyValuePair<string, int>("RelayWatermark", 2),
                type = ValueType.Int,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Radio", 7),
                command = new KeyValuePair<string, int>("RelayInterval", 3),
                type = ValueType.Int,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Radio", 7),
                command = new KeyValuePair<string, int>("HealthInterval", 4),
                type = ValueType.Int,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Radio", 7),
                command = new KeyValuePair<string, int>("TxFrequency", 5),
                type = ValueType.Int,
                value = null
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Radio", 7),
                command = new KeyValuePair<string, int>("TxPower", 6),
                type = ValueType.Int,
                value = null
            });
        }
        private void AddLedCommands() {

        }
        private void AddPowerCommands() {

            List<KeyValuePair<string, int>> volts = new List<KeyValuePair<string, int>>() {
                    new KeyValuePair<string, int>("3.3", 3300),
                    new KeyValuePair<string, int>("3.4", 3400),
                    new KeyValuePair<string, int>("3.5", 3500),
                    new KeyValuePair<string, int>("3.6", 3600),
                    new KeyValuePair<string, int>("3.7", 3700),
                    new KeyValuePair<string, int>("3.8", 3800),
                    new KeyValuePair<string, int>("3.9", 3900),
                    new KeyValuePair<string, int>("4.0", 4000),
                    new KeyValuePair<string, int>("4.1", 4100),
            };

            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Power", 6),
                command = new KeyValuePair<string, int>("GpsOffVolts", 2),
                type = ValueType.Set,
                value = volts
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Power", 6),
                command = new KeyValuePair<string, int>("GpsResumeVolts", 3),
                type = ValueType.Set,
                value = volts
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Power", 6),
                command = new KeyValuePair<string, int>("RadioOffVolts", 4),
                type = ValueType.Set,
                value = volts
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Power", 6),
                command = new KeyValuePair<string, int>("RadioResumeVolts", 5),
                type = ValueType.Set,
                value = volts
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Power", 6),
                command = new KeyValuePair<string, int>("LedOffVolts", 6),
                type = ValueType.Set,
                value = volts
            });
            options.Add(new Option {
                commandClass = new KeyValuePair<string, int>("Power", 6),
                command = new KeyValuePair<string, int>("LedResumeVolts", 7),
                type = ValueType.Set,
                value = volts
            });
        }
    }
}
