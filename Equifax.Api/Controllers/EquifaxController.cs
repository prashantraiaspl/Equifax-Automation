using AutoMapper;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Domain.Models;
using Equifax.Api.Interfaces;
using Equifax.Api.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Equifax.Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class EquifaxController : ControllerBase
    {
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<EquifaxController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly BrowserUtility _browserUtility;

        public EquifaxController(
            IRequestRepository requestRepository,
            ILogger<EquifaxController> logger,
            IConfiguration configuration,
            IMapper mapper,
            BrowserUtility browserUtility
            )
        {
            _requestRepository = requestRepository;
            _logger = logger;
            _configuration = configuration;
            _mapper = mapper;
            _browserUtility = browserUtility;
        }


        [HttpPost("Verify")]
        public async Task<IActionResult> LoadEquifaxQueue([FromBody] DisputeRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (requestDto == null)
            {
                return BadRequest(new ResponseBody
                {
                    status = false,
                    message = "Request data cannot be null."
                });
            }

            try
            {
                // Checking Existing Request in DB
                var requestData = await _requestRepository.CheckRequestQueue(requestDto);


                if (requestData.data == null)
                {
                    // Generating New Request in DB
                    var generatedRequest = await _requestRepository.InsertRequest(requestDto);
                }
                else
                {
                    //foreach (var request in requestData.data)
                    //{
                    //    if (request.RequestStatus == "Completed")
                    //    {
                    //        return Ok(new ResponseBody
                    //        {
                    //            status = true,
                    //            message = $"Request {request.RequestId} has already been completed.",
                    //            data = request
                    //        });
                    //    }
                    //    else if (request.RequestStatus == "Cancelled")
                    //    {
                    //        return Ok(new ResponseBody
                    //        {
                    //            status = true,
                    //            message = $"Request {request.RequestId} has been cancelled.",
                    //            data = request
                    //        });
                    //    }

                    //    else
                    //    {
                    //        //continue last request
                    //    }
                    //}
                }

                var scrappingUrl = _configuration.GetValue<string>("ScrappingURL") ?? "https://my.equifax.com";

                var loginCredentials = _mapper.Map<LoginCredentialRequestDto>(requestDto);

                // Log before performing browser automation
                _logger.LogInformation("Starting browser automation for URL: {scrappingUrl}", scrappingUrl);


                // Login Request Sent to Chrome Driver.
                await _browserUtility.BrowserAutomationProcess(scrappingUrl, loginCredentials, requestDto);


                return Ok(requestData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseBody
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
    }
}
