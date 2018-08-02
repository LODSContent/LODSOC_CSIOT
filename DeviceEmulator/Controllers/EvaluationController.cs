using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DeviceEmulator.Models;
namespace DeviceEmulator.Controllers
{
    [RoutePrefix("evaluate")]
    public class EvaluationController : ApiController
    {
        private DeviceContext deviceContext = new DeviceContext();
        [HttpPost]
        [Route("getreadings")]
        public async Task<EvaluationResult> GetReadings(DeviceWebAPIParameters parms)
        {
            parms.Fix();
            var result = new EvaluationResult
            {
                Passed = true,
                Code = 0,
                Message = "Successfully processed readings"
            };

            try
            {
                result = await deviceContext.GetReadings(parms);
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Code = ex.HResult;
                result.Message = $"There was an error processing readings:\t{ex.Message}";
            }
            return result;
        }

        [HttpPost]
        [Route("emptytest")]
        public EvaluationResult EmptyTest()
        {
            return new EvaluationResult
            {
                Code = 0,
                Message = "Connected",
                Passed = true
            };
        }

        [HttpPost]
        [Route("handleevents")]
        public async Task<EvaluationResult> HandleEvents(DeviceWebAPIParameters parms)
        {
            parms.Fix();
            var context = new EventHubContext();
            var result = await context.ReceiveEvents(parms);
            return result;
        }

        [HttpPost]
        [Route("messagetodevice")]
        public async Task<EvaluationResult> MessageToDevice(DeviceWebAPIParameters parms)
        {
            parms.Fix();
            var context = new DeviceContext();
            return await context.ReceiveCommand(parms);

        }

        [HttpPost]
        [Route("devicetwin")]
        public async Task<EvaluationResult> DeviceTwin(DeviceWebAPIParameters parms)
        {
            parms.Fix();
            var context = new DeviceContext();
            return await context.DeviceTwin(parms);

        }
    }



}