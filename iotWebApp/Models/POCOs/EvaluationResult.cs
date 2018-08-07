using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iotWebApp.Models
{
    public class EvaluationResult
    {
        public EvaluationResult()
        {
            Data = new List<DeviceReadingEntity>();
        }
        public bool Passed { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
        public List<DeviceReadingEntity> Data  { get; set; }
    }
}
