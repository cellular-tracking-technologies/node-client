using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Src.FileTransfer {
    class NodeDir {
        Serial serial;
        DataGridView view;

        const int DownloadColumnIndex = 3;
        const int DeleteColumnIndex = 4;
        public NodeDir(Serial ser_, DataGridView view_) {
            serial = ser_;
            view = view_;

            view.CellClick += GridCellClick;
            view.MouseDoubleClick += MouseDoubleClick;

        }
        public void Update() {
            this.view.Rows.Clear();
            serial.WriteData("ls:\r\n");
        }
        void DeleteFile(string filename) {
            if (String.IsNullOrEmpty(filename) == false) {
                serial.WriteData(String.Format("rm:{0}\r\n", filename));
            }
        }
        void DownloadFile(string filename) {
            if (String.IsNullOrEmpty(filename) == false) {
                serial.WriteData(String.Format("cat:{0}\r\n", filename));
            }
        }
        public void Parse(string data) {
            if (String.IsNullOrEmpty(data)) {
                return;
            }
            List<string> d = data.Split(',').ToList();
            if (d.Count < 5) {
                return;
            }
            if (d[1].Equals("Dir")) {
                view.Rows.Add(d[2], d[3], d[4]);
            }
        }

        void GridCellClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0) {
                return;
            }

            DataGridView v = sender as DataGridView;
            
            string filename = v.Rows[e.RowIndex].Cells[0].Value.ToString();

            switch (e.ColumnIndex) {
                case DownloadColumnIndex:
                    DownloadFile(filename);
                    break;
                case DeleteColumnIndex:
                    if (ConfirmDeleteFile(filename)) {
                        DeleteFile(filename);
                        Update();                        
                    }

                    break;
                default:
                    break;
            }

            Debug.WriteLine(e.ColumnIndex);
            ClearViewSelection();
        }
        void ClearViewSelection() {
            view.ClearSelection();
            view.CurrentCell = null;
        }

        bool ConfirmDeleteFile(string filename) {
            string title = "Permanently Delete File From Node";
            string message = String.Format("This operation will delete {0} from the node forever. Are you sure?", filename);

            DialogResult dialogResult = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes) {
                return true;
            }
            return false;
        }
        private void MouseDoubleClick(object sender, MouseEventArgs e) {
            Update();
        }
    }
}
