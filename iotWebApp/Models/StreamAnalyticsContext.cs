using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iotWebApp.Models
{
    public class StreamAnalyticsContext
    {
        private StorageContext storage;
        private DeviceWebAPIParameters parms;

        public StreamAnalyticsContext(DeviceWebAPIParameters parms)
        {
            storage = new StorageContext(parms.EhubStorage, "averages", null);
            this.parms = parms;
        }

        public async Task<EvaluationResult> VerifyData()
        {
            var result = new EvaluationResult { Code = 0, Passed = true, Message = "Verified Stream Analytics results" };
            try
            {
                var iotreg = new IotUtilities.IotRegistry(parms.IotConnection);
                var devices = await iotreg.GetDeviceNames();
                result = await storage.RetrieveTableData(5, devices);
            }
            catch (Exception ex)
            {
                result.Code = ex.HResult;
                result.Passed = false;
                result.Message = $"Error testing Stream Analytics: {ex.Message}";
            }
            return result;
        }
    }
}
