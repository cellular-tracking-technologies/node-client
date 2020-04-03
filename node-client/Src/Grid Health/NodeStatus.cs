using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Src.GridHealth {
    class NodeStatus {
        public double Battery { get; private set; }
        public double InitialBattery { get; private set; }
        public double DeltaBattery { get; private set; }
        
        public string Serial { get; set; }
        public DateTime LastHealth { get; set; }
        public int Rssi { get; set; }
        public string Firmware { get; set; }

        public double SolarVoltage { get; set; }
        public int TotalSolarCurrent { get; set; }
        public DateTime FixTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Temperature { get; set; }
        
        public NodeStatus(dynamic serial, dynamic battery) {
            Serial = Convert.ToString(serial);
            InitialBattery = Convert.ToDouble(battery);            
        }
        
        public NodeStatus(dynamic json) {
            Serial = Convert.ToString(json.meta.source.id);
            InitialBattery = Convert.ToDouble(json.data.bat_v) / 100;
            Battery = InitialBattery;

            Rssi = Convert.ToInt32(json.meta.rssi);
            Firmware = Convert.ToString(json.data.fw);
            
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            LastHealth  = epoch.AddSeconds(Convert.ToInt64(json.data.sent_at));
            FixTime = epoch.AddSeconds(Convert.ToInt64(json.data.fix_at));            

            SolarVoltage = Convert.ToDouble(json.data.sol_v) / 100;
            TotalSolarCurrent = Convert.ToInt32(json.data.sum_sol_ma);
            Temperature = Convert.ToInt32(json.data.temp_c);


            Latitude = Convert.ToDouble(json.data.lat) / 1E6;
            Longitude = Convert.ToDouble(json.data.lon) / 1E6;
        }

        public void Update(NodeStatus node) {
            Battery = node.Battery;
            DeltaBattery = Battery - InitialBattery;

            Rssi = node.Rssi;
            Firmware = node.Firmware;
            LastHealth = node.LastHealth;
            FixTime = node.FixTime;

            SolarVoltage = node.SolarVoltage;
            TotalSolarCurrent = node.TotalSolarCurrent;
            Temperature = node.Temperature;

            Latitude = node.Latitude;
            Longitude = node.Longitude;
        }



    }
}
