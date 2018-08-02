using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deviceSimulator.Models
{
    public class EvaluationResult
    {
        public bool Passed { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
    }
}
