using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deviceSimulator.Models
{
    public class Device:ISettings
    {
        public string DeviceID { get; set; }
        public string Connection { get; set; }
        public string DeviceName { get; set; }
        public double Reading { get; set; }
        public DateTime StartUp { get; set; }
        public string Firmware { get; set; }
        public DateTime CurrentTime { get; set; }
        public int Interval { get; set; }
        public double Range { get; set; }

        public SampleData GetReading()
        {
            var rand = new Random();
            CurrentTime = CurrentTime.AddMilliseconds(Interval);
            Reading += Range * (rand.NextDouble() - .5);
            return new SampleData
            {
                DeviceID = this.DeviceID,
                Reading = Reading,
                Time = CurrentTime
            };
        }

        public int UpdateFirmware(string firmware)
        {
            this.Firmware = firmware;
            return 0;
        }
        public ISettings GetSettings()
        {
            return this;
        }
        public int SetSettings(ISettings settings)
        {
            this.DeviceName = settings.DeviceName;
            this.Firmware = settings.Firmware;
            this.Interval = settings.Interval;
            return 0;
        }
        public DateTime Reboot()
        {
            this.StartUp = DateTime.Now;
            return this.StartUp;
        }
    }
    public interface ISettings
    {
        string DeviceID { get; set; }
        string DeviceName { get; set; }
        DateTime StartUp { get; set; }
        string Firmware { get; set; }
        int Interval { get; set; }

    }

    public class SampleData
    {
        public DateTime Time { get; set; }
        public string DeviceID { get; set; }
        public double Reading { get; set; }
    }
}
