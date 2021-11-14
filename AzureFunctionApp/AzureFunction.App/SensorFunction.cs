using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using AzureFunction.App.Authentication;

namespace AzureFunction.App
{
    public class SensorFunction
    {
        private readonly ISensorService _sensorService;
        private readonly ILogger<SensorFunction> _logger;

        public SensorFunction(ISensorService sensorService, ILogger<SensorFunction> logger)
        {
            _sensorService = sensorService;
            _logger = logger;
        }

        [FunctionName("GetSensor")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequest request,
            [Principal] ClaimsPrincipal principal,
            CancellationToken cancellationToken)
        {
            if (!(principal?.Identity?.IsAuthenticated).GetValueOrDefault(false))
            {
                return new UnauthorizedResult();
            }
            try
            {
                var sensorBoxId = request.GetQueryParameterDictionary()["sensorBoxId"];
                var sensorType = Enum.Parse<SensorType>(request.GetQueryParameterDictionary()["sensorType"]);
                var sensor = await _sensorService.GetByBoxIdAndTypeAsync(sensorBoxId, sensorType);

                return sensor != null ? (IActionResult)new OkObjectResult(sensor) : new NotFoundResult();
            }
            catch (Exception e) when (e is KeyNotFoundException || e is ArgumentException)
            {
                return new BadRequestResult();
            }
        }
    }
}