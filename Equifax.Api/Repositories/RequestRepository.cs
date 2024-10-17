using Equifax.Api.Data;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Domain.Enums;
using Equifax.Api.Domain.Models;
using Equifax.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Equifax.Api.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RequestRepository> _logger;
        private readonly DbSet<RequestMaster> _dbSet;

        public RequestRepository(ApplicationDbContext context, ILogger<RequestRepository> logger)
        {
            _context = context;
            _dbSet = _context.Set<RequestMaster>();
            _logger = logger;
        }


        // Checking the Existing Request from Database.
        public async Task<ResponseBody> CheckRequestQueue(DisputeRequestDto requestDto)
        {
            var responseBody = new ResponseBody();

            try
            {
                //var query = _dbSet.Where(request => request.client_id == requestDto.client_id);
                var query = _dbSet
                    .Where(request => request.request_status == RequestStatus.InProgress && request.client_id == requestDto.client_id);


                var result = await query.ToListAsync();


                if (result.Count == 0)
                {
                    responseBody.status = false;
                    responseBody.message = "Record Not Found.";
                }
                else
                {
                    responseBody.status = true;
                    responseBody.message = "Record Found Successfully.";
                    responseBody.data = result;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                responseBody.status = false;
                responseBody.message = ex.Message;
            }

            return responseBody;
        }


        // Inserting the New Request in Database.
        public async Task<ResponseBody> InsertRequest(DisputeRequestDto requestDto)
        {
            var responseBody = new ResponseBody();

            try
            {
                var newRequest = new RequestMaster
                {
                    user_name = requestDto.user_name,
                    user_password = requestDto.user_password,
                    client_id = requestDto.client_id,
                    request_status = RequestStatus.InProgress
                };

                var result = await _dbSet.AddAsync(newRequest);

                await _context.SaveChangesAsync();

                responseBody.status = true;
                responseBody.message = "Request Inserted Successfully.";
                responseBody.data = result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                responseBody.status = false;
                responseBody.message = ex.Message;
            }

            return responseBody;
        }


        // Updating the Request in Database.
        public async Task<ResponseBody> UpdateRequest(RequestMaster response)
        {
            var responseBody = new ResponseBody();

            try
            {
                var existingRequest = await _dbSet.FirstOrDefaultAsync(request =>
                    request.client_id == response.client_id &&
                    request.request_status == RequestStatus.InProgress);


                if (existingRequest == null)
                {
                    responseBody.status = false;
                    responseBody.message = "Sorry, Request Not Found.";
                    return responseBody;
                }

                else
                {
                    existingRequest.request_status = response.request_status;
                    existingRequest.creditor_name = response.creditor_name;
                    existingRequest.open_date = response.open_date;
                    existingRequest.confirmation_number = response.confirmation_number;

                    await _context.SaveChangesAsync();
                }


                responseBody.status = true;
                responseBody.message = "Request Updated Successfully.";
                responseBody.data = existingRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                responseBody.status = false;
                responseBody.message = ex.Message;
            }

            return responseBody;
        }
    }
}
