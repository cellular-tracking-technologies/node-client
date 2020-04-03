using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Src {
    class LineBuilder {
        private string remainder = "";
        public LineBuilder() {

        }
        public void Ingest(string data, List<Action<string>> parsers) {

            if (data.Contains(Environment.NewLine) == false) {
                remainder += data;
                return;
            }

            data = remainder + data;

            while (true) {
                int idx = data.IndexOf(Environment.NewLine);
                if (idx == -1) {
                    remainder = data;
                    return;
                } else {
                    remainder = data.Substring(idx + Environment.NewLine.Length);
                    data = data.Substring(0, idx);
                }

                // Execute list of functions that take a line
                foreach(Action<string> function in parsers) {
                    function(data);
                }

                data = remainder;
            }
        }
    }
}
