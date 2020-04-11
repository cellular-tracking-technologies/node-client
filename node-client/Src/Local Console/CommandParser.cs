using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Src.LocalConsole {
    class CommandParser {
        int MinimumFields => 2;
        private bool Parsed { get; set; }
        Dictionary<string, string> dict = new Dictionary<string, string>();

        public CommandParser(string data) {
            Parsed = this.Parse(data);
        }
        public Dictionary<string, string> GetData() {
            if (this.Parsed) {
                return this.dict;
            } else {
                return null;
            }
        }
        private bool Parse(string data) {

            if (String.IsNullOrEmpty(data)) {
                return false;
            } else if (String.IsNullOrWhiteSpace(data)) {
                return false;
            }

            List<string> fields = this.GetFields(data);
            if (fields.Count < MinimumFields) {
                return false;
            }

            this.dict.Clear();

            switch (fields[1]) {
                case "Power":
                    return ParsePower(fields);
                case "Beep":
                    return ParseBeep(fields);
                case "Gps":
                    return ParseGps(fields);
                case "Id":
                    return ParseId(fields);
                case "Sd":
                    return ParseSd(fields);
                case "Sun":
                    return ParseSun(fields);
                default:
                    return false;
            }
        }

        private bool ParseGps(List<string> data) {
            if (data.Count < 4) {
                return false;
            }

            this.dict.Add("time", data[0]);
            this.dict.Add("key", data[1]);

            const string kNullLocation = "0.000000";
            if (kNullLocation.Equals(data[2]) == false) { 
                this.dict.Add("Latitude", data[2]);
            }
            if (kNullLocation.Equals(data[3]) == false) {
                this.dict.Add("Longitude", data[3]);
            }

            return true;
        }
        private bool ParsePower(List<string> data) {
            if (data.Count < 6) {
                return false;
            }

            this.dict.Add("time", data[0]);
            this.dict.Add("key", data[1]);
            this.dict.Add("BatteryVolts", data[2]);
            this.dict.Add("SolarVolts", data[3]);
            this.dict.Add("SolarCurrent", data[4]);
            this.dict.Add("TemperatureCelsius", data[5]);
            
            return true;
        }
        private bool ParseBeep(List<string> data) {
            if(data.Count < 3) {
                return false;
            }

            this.dict.Add("time", data[0]);
            this.dict.Add("key", data[1]);
            this.dict.Add("id", data[2]);
            this.dict.Add("rssi", data[3]);

            return true;
        }

        private bool ParseId(List<string> data) {
            if (data.Count < 4) {
                return false;
            }

            this.dict.Add("time", data[0]);
            this.dict.Add("key", data[1]);
            this.dict.Add("DeviceId", data[2]);
            this.dict.Add("Firmware", data[3]);

            return true;
        }
        private bool ParseSd(List<string> data) {
            //2017-01-01 01:01:01,Sd,1
            if (data.Count < 3) {
                return false;
            }

            this.dict.Add("time", data[0]);
            this.dict.Add("key", data[1]);
            this.dict.Add("SdOk", data[2]);

            return true;
        }
        private bool ParseSun(List<string> data) {
            if (data.Count < 5) {
                return false;
            }
            // 2020-04-03 19:13:50,Sun,1,957,9

            int sunrise = Convert.ToInt32(data[3]);
            int sunset = Convert.ToInt32(data[4]);

            int sunriseHour = sunrise / 100;
            int sunriseMinute = sunrise - (sunriseHour * 100);

            int sunsetHour = sunset / 100;
            int sunsetMinute = sunset - (sunsetHour * 100);

            DateTime now = DateTime.UtcNow;
            DateTime sr = DateTime.Parse(String.Format("{0}/{1}/{2} {3}:{4}:{5}",
                now.Year, now.Month, now.Day, sunriseHour, sunriseMinute, 0));
            DateTime ss = DateTime.Parse(String.Format("{0}/{1}/{2} {3}:{4}:{5}",
                now.Year, now.Month, now.Day, sunsetHour, sunsetMinute, 0));

            this.dict.Add("time", data[0]);
            this.dict.Add("key", data[1]);
            this.dict.Add("SunIsUp", data[2]);
            this.dict.Add("Sunrise", sr.ToLocalTime().ToString());
            this.dict.Add("Sunset", ss.ToLocalTime().ToString());

            return true;
        }
        private List<string> GetFields(string data) {
            return data.Split(',').ToList();
        }
        public static bool Verify(Dictionary<string, string> dict, List<string> keys) {
            if (dict == null) {
                return false;
            }

            foreach (string key in keys) {
                if (dict.ContainsKey(key) == false) {
                    return false;
                }
                string value = "";
                if (dict.TryGetValue(key, out value) == false) {
                    return false;
                }
            }
            return true;
        }
    }
}
