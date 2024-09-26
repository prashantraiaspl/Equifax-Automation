using AutoMapper;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Interfaces;
using Equifax.Api.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Equifax.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquifaxController : ControllerBase
    {
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<EquifaxController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public EquifaxController(
            IRequestRepository requestRepository,
            ILogger<EquifaxController> logger,
            IConfiguration configuration,
            IMapper mapper
            )
        {
            _requestRepository = requestRepository;
            _logger = logger;
            _configuration = configuration;
            _mapper = mapper;
        }


        [HttpPost("Verify")]
        public async Task<IActionResult> LoadEquifaxQueue([FromBody] PayloadRequestDto requestDto)
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
                var requestData = await _requestRepository.CheckRequestQueue(requestDto);


                if (requestData.data == null)
                {
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

                string scrappingUrl = _configuration["ScrappingURL"];

                var loginCredentials = _mapper.Map<LoginCredentialRequestDto>(requestDto);

                BrowserUtility.OpenBrowserAndNavigate(scrappingUrl, loginCredentials);


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
