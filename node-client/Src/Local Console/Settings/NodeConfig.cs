using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Src.LocalConsole.Settings {
    enum ValueType {
        None,
        Range,
        Set
    }
    struct Option {
        public int task;
        public int command;
        public ValueType type;
        public List<int> value;
    }
    class NodeConfig {
        List<Option> options = new List<Option>();
        public NodeConfig() {
            options.Add(new Option {
                task = 4,
                command = 6,
                type = ValueType.None,
                value = null
            });
        }
    }
}
