using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Src.LocalConsole {

    class FileUpload {
        public string FileType { get; set; }
        public string Extension { get; set; }
        public string Header { get { return FetchHeader(); } }
        public string FileContents { get; set; }
        public string FileName { get; set; }
        public string Command { get; set; }
        public string Options { get; set; }
        public List<string> Filters { get; set; }
        public Action<bool> PreUploadAction { get; set; }
        public FileUpload() {

        }

        public string GetFileFromDialog(string file_type, string extension) {
            string filterAll = "All files (*.*)|*.*";
            string filterSpecific = file_type + "|" + "*" + extension;
            OpenFileDialog selector = new OpenFileDialog {
                Filter = filterSpecific + "|" + filterAll,
                Title = "Choose " + file_type + " file to upload",
                InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString()
            };

            if (selector.ShowDialog() == DialogResult.OK) {
                return selector.FileName;
            }

            return String.Empty;
        }

        public void Upload(Serial serial, ProgressBar progress) {
            string file_name = GetFileFromDialog(FileType, Extension);
            bool quit = false;
            if (String.IsNullOrEmpty(file_name)) {
                quit = true;
            } else if (String.IsNullOrWhiteSpace(file_name)) {
                quit = true;
            }

            if (quit) {
                PreBuildTask(false);
                return;
            }

            FileName = Path.GetFileName(file_name);
            FileContents = File.ReadAllText(file_name);

            ApplyFilters();

            progress.Minimum = 0;
            progress.Maximum = FileContents.Length;
            progress.Value = 0;
            progress.Visible = true;
            
            MethodInvoker updateProgress = new MethodInvoker(() => progress.Value += 1);
            MethodInvoker hideProgress = new MethodInvoker(() => progress.Visible = false);

            Thread upload = new Thread(() => {
                try {

                    PreBuildTask(true);
                    serial.WriteData(Header);
                    Thread.Sleep(2000);

                    foreach(char c in FileContents) {
                        serial.WriteData(c.ToString());
                        progress.Invoke(updateProgress);
                    }
                    progress.Invoke(hideProgress);

                } finally {
                }
            });
            upload.Start();
        }

        private string FetchHeader() {
            return Command + ":" +
                Options + " " +
                FileContents.Length + " " +
                "90000" +
                Environment.NewLine;
        }
        private void ApplyFilters() {
            if (Filters == null) {
                return;
            }
            foreach (string filter in Filters) {
                FileContents = FileContents.Replace(filter, "");
            }
        }
        private void PreBuildTask(bool start) {
            if (PreUploadAction != null) {
                PreUploadAction.Invoke(start);
            }
        }
        private string CalculateMd5(string input) {
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++) {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
