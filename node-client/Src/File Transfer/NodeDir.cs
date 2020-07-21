using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Src.FileTransfer {
    class NodeDir {
        Serial serial;
        DataGridView table;
        ProgressBar progress;
        Stream file;
        public bool DownloadInProgress { get; set; }

        const int DownloadColumnIndex = 3;
        const int DeleteColumnIndex = 4;
        public NodeDir(Serial ser_, Dictionary<string, Control> controls) {
            serial = ser_;
            table = controls["table"] as DataGridView;
            progress = controls["progress"] as ProgressBar;

            InitTable();
        }
        private void InitTable() {
            table.CellClick += GridCellClick;
            table.MouseDoubleClick += MouseDoubleClick;
        }

        public void Update() {
            table.Rows.Clear();
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
        bool OpenFileFromDialog(string defaultFilename) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.FilterIndex = 2;
            dialog.RestoreDirectory = true;
            dialog.FileName = defaultFilename;

            if (dialog.ShowDialog() == DialogResult.OK) {
                if ((file = dialog.OpenFile()) != null) {
                    return true;
                }
            }
            return false;
        }
        private void InitProgress(int maxValue, bool visible) {
            progress.Minimum = 0;
            progress.Maximum = maxValue;
            progress.Value = 0;
            progress.Visible = visible;
        }
        private void IncrementProgress(int value) {
            try {
                progress.Value += value;
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }
        private void AppendToFile(string data) {
            try {
                byte[] info = new UTF8Encoding(true).GetBytes(data);
                file.Write(info, 0, info.Length);
            }catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }
        private void CloseFile() {
            file.Close();
        }

        public void Parse(string data) {
            if (String.IsNullOrEmpty(data)) {
                return;
            }
            List<string> d = data.Split(',').ToList();

            if(d.Count < 3) {
                return;
            }
            
            if (d[1].Equals("Dir")) {
                table.Rows.Add(d[2], d[3], d[4]);
            }else if (d[1].Equals("File")) {
                if (d[2].Equals("Start")){
                    DownloadInProgress = true;
                    InitProgress(Convert.ToInt32(d[4]), true);
                } else if(d[2].Equals("Stop")){
                    DownloadInProgress = false;
                    InitProgress(0, false);                    
                    CloseFile();
                }
            }else if (DownloadInProgress) {
                string toWrite = data + Environment.NewLine;
                AppendToFile(toWrite);
                IncrementProgress(toWrite.Length);
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
                    if (OpenFileFromDialog(filename)) { 
                        DownloadFile(filename);                        
                    }
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

            v.ClearSelection();
            v.CurrentCell = null;
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

        }
    }
}
