using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using iotWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace iotWebApp.Controllers
{
    [Route("evaluate")]
    public class EvaluationController : Controller
    {
        private readonly IConfiguration Config;
        private DeviceContext deviceContext = new DeviceContext();

        public EvaluationController(IConfiguration config)
        {
            Config = config;
        }

        [HttpPost]
        [Route("generatedata")]
        public async Task<EvaluationResult> GenerateData(DeviceWebAPIParameters parms)
        {
            parms.Fix(Config);
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
        [Route("verifyevents")]
        public async Task<EvaluationResult> VerifyEvents(DeviceWebAPIParameters parms)
        {
            parms.Fix(Config);
            var context = new EventHubContext(parms);
            var result = await context.ReceiveEvents();
            return result;
        }

        [HttpPost]
        [Route("messagetodevice")]
        public async Task<EvaluationResult> MessageToDevice(DeviceWebAPIParameters parms)
        {
            parms.Fix(Config);
            var context = new DeviceContext();
            return await context.ReceiveCommand(parms);

        }

        [HttpPost]
        [Route("devicetwin")]
        public async Task<EvaluationResult> DeviceTwin(DeviceWebAPIParameters parms)
        {
            parms.Fix(Config);
            var context = new DeviceContext();
            return await context.DeviceTwin(parms);

        }

        [HttpPost]
        [Route("streamanalytics")]
        public async Task<EvaluationResult> StreamAnalytics(DeviceWebAPIParameters parms)
        {
            parms.Fix(Config);
            var context = new StreamAnalyticsContext(parms);
            var result = await context.VerifyData();
            return result;
        }

        [HttpPost]
        [Route("consumergroups")]
        public async Task<EvaluationResult> ConsumerGroups(DeviceWebAPIParameters parms)
        {
            parms.Fix(Config);
            var context = new EventHubContext(parms);
            var result = await context.TestConsumerGroups();
            return result;
        }

        [HttpPost]
        [Route("endpoint")]
        public async Task<EvaluationResult> Endpoint(DeviceWebAPIParameters parms)
        {
            parms.Fix(Config);
            var context = new EventHubContext(parms);
            var result = await context.TestEndpoint();
            return result;
        }
        [HttpPost]
        [Route("cosmosdb")]
        public EvaluationResult CosmosDB(DeviceWebAPIParameters parms)
        {
            parms.Fix(Config);
            var context = new CosmosDBContext(parms);
            var result =  context.TestDocuments("iot","events");
            return result;
        }
    }



}