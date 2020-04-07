using System.Diagnostics;
using System.Windows.Forms;

namespace Src.Console {
    class SettingsSummary {
        private DataGridView view;
        public SettingsSummary(DataGridView grid) {
            view = grid;
        }
        public void Parse(string data) {
            Debug.WriteLine(data);
        }
    }
}
