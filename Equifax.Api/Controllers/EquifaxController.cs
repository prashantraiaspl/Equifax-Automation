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
                    // Generating New Request ID in Database
                    var generatedRequest = await _requestRepository.InsertRequest(requestDto);

                    // NEW Request Generated
                    _logger.LogInformation("New Request Generated and Inserted in DB");

                    // Log before performing browser automation
                    _logger.LogInformation("Starting browser automation for URL: {scrappingUrl}", _scrappingUrl);

                    // Login Request Sent to Chrome Driver.
                    ResponseBody result = await _browserUtility.BrowserAutomationProcess(_scrappingUrl, loginCredentials, requestDto);

                    if(result.data is not null)
                    {
                        var response = new RequestMaster
                        {
                            request_status = RequestStatus.Completed,
                            creditor_name = requestDto.equifax_data.account[0].creditor_name,
                            open_date = requestDto.equifax_data.account[0].open_date,
                            confirmation_number = result?.data?.confirmation_number,
                        };

                        if (response.confirmation_number != null)
                        {
                            await _requestRepository.UpdateRequest(response);
                        }
                    }
                }
                else
                {
                    foreach (var request in requestData.data)
                    {
                        if (request.request_status == RequestStatus.Completed)
                        {
                            return Ok(new ResponseBody
                            {
                                status = true,
                                message = $"Request ID: {request.RequestId} has Already been Completed.",
                                data = request
                            });
                        }
                        else if (request.request_status == RequestStatus.Cancelled)
                        {
                            return Ok(new ResponseBody
                            {
                                status = true,
                                message = $"Request ID: {request.RequestId} has been Cancelled.",
                                data = request
                            });
                        }

                        //continue last request
                        else
                        {
                            // Login Request Sent to Chrome Driver.
                            var result = await _browserUtility.BrowserAutomationProcess(_scrappingUrl, loginCredentials, requestDto);

                            if (result.data)
                            {
                                var response = new RequestMaster
                                {
                                    request_status = RequestStatus.Completed,
                                    creditor_name = requestDto.equifax_data.account[0].creditor_name,
                                    open_date = requestDto.equifax_data.account[0].open_date,
                                    confirmation_number = result?.data?.confirmation_number,
                                };

                                if (response.confirmation_number != null)
                                {
                                    await _requestRepository.UpdateRequest(response);
                                }
                            }
                        }
                    }
                }


                return Ok(requestDto);
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
    }
}
