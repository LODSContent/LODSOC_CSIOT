using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using deviceSimulator.Models;
namespace deviceSimulator.Controllers
{
    [Produces("application/json")]
    [Route("evaluation")]
    public class EvaluationController : Controller
    {
        private DeviceContext deviceContext = new DeviceContext();
        [HttpPost]
        [Route("getreadings")]
        public EvaluationResult GetReadings(string[] connectionStrings, int interval, int iterations)
        {
            var result = new EvaluationResult
            {
                Passed = true,
                Code = 0,
                Message = "Successfully processed readings"
            };

            try
            {
                deviceContext.GetReadings(connectionStrings, interval, iterations);
            }
            catch(Exception ex)
            {
                result.Passed = false;
                result.Code = ex.HResult;
                result.Message = $"There was an error getting readings:\t{ex.Message}";
            }
            return result;
        }
    }
}