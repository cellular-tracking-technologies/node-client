using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Src {
    class LogManager {

        private string active_file = "";
        private bool serial_pause = false;
        private static string folder = "Logs";
        private static string file_extension = "*.log";

        private string full_dir = "";

        public bool SerialPause { get => serial_pause; set => serial_pause = value; }
        public string ActiveFile { get => active_file; set => active_file = value; }

        public LogManager(string relative_path) {

            if (String.IsNullOrEmpty(relative_path) || (Directory.Exists(relative_path) == false)) {
                full_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), folder);
            } else {
                full_dir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, relative_path + folder));                 
            }

            if (!Directory.Exists(full_dir)) {
                try {
                    System.IO.Directory.CreateDirectory(full_dir);
                } catch (IOException ie) {
                    System.Console.WriteLine("IO Error: " + ie.Message);
                } catch (Exception e) {
                    System.Console.WriteLine("General Error: " + e.Message);
                }
            }
        }

        public void Init() {
            ActiveFile = null;
        }

        public void UpdateFileName(string name) {
            string file_name = Path.Combine(full_dir, name);
            using (FileStream fs = File.Open(file_name, FileMode.Create)) {
                ActiveFile = file_name;
            }
        }

         public void AppendToLog(string data) {

            if (ActiveFile == null) {
                return;
            }

            if (data.Length == 0) {
                return;
            }

            try {
                using (FileStream writer = new FileStream(ActiveFile, FileMode.Open)) {

                    if (writer.Length > 1) {
                        writer.Seek(-1, SeekOrigin.End);
                    }

                    byte[] bytes = Encoding.ASCII.GetBytes(data);
                    for (int i = 0; i < bytes.Length; i++) {
                        writer.WriteByte(bytes[i]);
                    }
                }
            } catch {
                System.Console.WriteLine("Log Exception");
            }
        }

        public string[] UpdateDirectory() {

            var file_names = new List<string>();

            DirectoryInfo d = new DirectoryInfo(full_dir);
            FileInfo[] Files = d.GetFiles(file_extension);

            foreach (FileInfo file in Files) {
                file_names.Add(file.Name);
            }

            return file_names.ToArray();
        }

        public string ReadFile(string file_name) {          

            DirectoryInfo d = new DirectoryInfo(full_dir);
            FileInfo[] Files = d.GetFiles(file_extension);

            string file_contents = "";

            foreach (FileInfo file in Files) {
                if (file_name == file.Name) {
                    string full_path = Path.Combine(full_dir, file_extension);
                    StreamReader reader = new StreamReader(Path.Combine(full_dir, file.Name));
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    file_contents = reader.ReadToEnd();
                    reader.Close();
                    break;
                }
            }

            return file_contents;
        }

        public void DeleteFile(string file_name) {
            DirectoryInfo d = new DirectoryInfo(full_dir);
            FileInfo[] Files = d.GetFiles(file_extension);

            if (Path.GetFileName(ActiveFile) == file_name) {
                ActiveFile = null;
            }

            foreach (FileInfo file in Files) {
                if (file_name == file.Name) {
                    file.Delete();
                    break;
                }
            }
        }
    }
}
