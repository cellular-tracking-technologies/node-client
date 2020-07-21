using System;
using System.Windows.Forms;

namespace node_client.Forms {
    public partial class SettingUpdate : Form {
        Src.Serial usb;
        LocalConsole.Settings.Setting setting = null;
        public SettingUpdate(Src.Serial port, LocalConsole.Settings.Setting s) {
            InitializeComponent();
            usb = port;
            setting = s;
            Init();
        }
        private void Init() {

            if(setting.OutputOptions != null) {
                setting.OutputOptions.ForEach(option => {
                    comboBoxSettingsValues.Items.Add(option);
                });
                comboBoxSettingsValues.SelectedIndex = comboBoxSettingsValues.Items.IndexOf(setting.Value);
                comboBoxSettingsValues.DropDownStyle = ComboBoxStyle.DropDownList;
            } else {
                comboBoxSettingsValues.DropDownStyle = ComboBoxStyle.Simple;
                comboBoxSettingsValues.Text = setting.Value;
            }

            this.Text = setting.ReadableName;
        }
        private void ButtonSettingSend_Click(object sender, EventArgs e) {
            if (usb.IsOpen()) {
                usb.WriteData(setting.GetUpdateCommand(comboBoxSettingsValues.Text));
            }
        }
    }
}
