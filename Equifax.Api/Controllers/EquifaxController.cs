using AutoMapper;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Domain.Enums;
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
        private readonly string _scrappingUrl;

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
            _scrappingUrl = _configuration["ScrappingURL"] ?? "https://my.equifax.com";
        }


        [HttpPost("Verify")]
        public async Task<IActionResult> LoadEquifaxQueue([FromBody] DisputeRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var loginCredentials = _mapper.Map<LoginCredentialRequestDto>(requestDto);


                // Checking Existing Request in DB
                var requestData = await _requestRepository.CheckRequestQueue(requestDto);


                if (requestData.data == null)
                {
                    // Handle new request
                    return await HandleNewRequestAsync(requestDto, loginCredentials);
                }
                else
                {
                    // Handle existing request
                    return await HandleExistingRequestAsync(requestData.data, requestDto, loginCredentials);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseBody
                {
                    status = false,
                    message = ex.Message
                });
            }
        }


        //---------------------------HELPER FUNCTIONS------------------------------//
        private async Task<IActionResult> HandleNewRequestAsync(DisputeRequestDto requestDto, LoginCredentialRequestDto loginCredentials)
        {
            // Generate new request in DB
            await _requestRepository.InsertRequest(requestDto);

            _logger.LogInformation("New Request Generated and Inserted in DB");

            return await ProcessRequestWithBrowserAutomationAsync(requestDto, loginCredentials);
        }


        private async Task<IActionResult> HandleExistingRequestAsync(IEnumerable<RequestMaster> requestData, DisputeRequestDto requestDto, LoginCredentialRequestDto loginCredentials)
        {
            foreach (var request in requestData)
            {
                if (request.request_status == RequestStatus.Completed)
                {
                    return Ok(BuildResponseBody(request, "Request has Already been Completed."));
                }
                else if (request.request_status == RequestStatus.Cancelled)
                {
                    return Ok(BuildResponseBody(request, "Request has been Cancelled."));
                }
            }

            // Continue last request if not completed or cancelled
            return await ProcessRequestWithBrowserAutomationAsync(requestDto, loginCredentials);
        }


        private async Task<IActionResult> ProcessRequestWithBrowserAutomationAsync(DisputeRequestDto requestDto, LoginCredentialRequestDto loginCredentials)
        {
            // Log and start browser automation
            _logger.LogInformation($"Starting Browser Automation for URL: {_scrappingUrl}");

            string confirmationNumber = await _browserUtility.BrowserAutomationProcess(_scrappingUrl, loginCredentials, requestDto);

            if (!string.IsNullOrEmpty(confirmationNumber))
            {
                List<RequestMaster> updatedRequests = new List<RequestMaster>();

                foreach (var account in requestDto.equifax_data.account)
                {
                    var response = new RequestMaster
                    {
                        RequestId = 0,
                        user_name = requestDto.user_name,
                        user_password = requestDto.user_password,
                        client_id = requestDto.client_id,
                        request_status = RequestStatus.Completed,
                        creditor_name = account.creditor_name,
                        open_date = account.open_date,
                        confirmation_number = confirmationNumber,
                        credit_repair_id = account.credit_repair_id,
                    };

                    // Update request with confirmation number
                    var updateResult = await _requestRepository.UpdateRequest(response);


                    if (updateResult.status)
                    {
                        account.confirmation_number = response.confirmation_number;
                        updatedRequests.Add(response);
                    }
                }

                if (updatedRequests.Count != 0)
                {
                    // Build the response using your BuildResponseBody-like logic
                    var responseBody = new ResponseBody
                    {
                        status = true,
                        message = "Request processed successfully.",
                        data = updatedRequests
                    };

                    return Ok(responseBody);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseBody
                    {
                        status = false,
                        message = "Failed to update the requests."
                    });
                }
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseBody
            {
                status = false,
                message = "Failed to get confirmation number."
            });
        }


        private ResponseBody BuildResponseBody(RequestMaster request, string message)
        {
            return new ResponseBody
            {
                status = true,
                message = $"{message} Request ID: {request.RequestId}",
                data = request
            };
        }


    }
}
