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


        //// Updating the Request in Database.
        //public async Task<ResponseBody> UpdateRequest(IEnumerable<RequestMaster> requestDtos)
        //{
        //    var responseBody = new ResponseBody();

        //    try
        //    {
        //        // Loop through all the requestDtos to update multiple records if needed
        //        foreach (var requestDto in requestDtos)
        //        {
        //            // Find the existing request in the database by client_id and status
        //            var existingRequest = await _dbSet.FirstOrDefaultAsync(request =>
        //                request.client_id == requestDto.client_id &&
        //                request.request_status == RequestStatus.InProgress);

        //            if (existingRequest == null)
        //            {
        //                responseBody.status = false;
        //                responseBody.message = $"Sorry, Request Not Found for client_id: {requestDto.client_id}";
        //                return responseBody; // Return if any request is not found
        //            }
        //            else
        //            {
        //                // Update the existing request with new values
        //                existingRequest.request_status = requestDto.request_status;
        //                existingRequest.creditor_name = requestDto.creditor_name;
        //                existingRequest.open_date = requestDto.open_date;
        //                existingRequest.confirmation_number = requestDto.confirmation_number;

        //                // Save the updated request to the database
        //                _context.Update(existingRequest);
        //            }
        //        }

        //        // Save all changes at once after looping through all requestDtos
        //        await _context.SaveChangesAsync();

        //        responseBody.status = true;
        //        responseBody.message = "All Requests Updated Successfully.";
        //        responseBody.data = requestDtos;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);

        //        responseBody.status = false;
        //        responseBody.message = ex.Message;
        //    }

        //    return responseBody;
        //}


        // Updating the Request in Database.
        public async Task<ResponseBody> UpdateRequest(RequestMaster requestdto)
        {
            var responseBody = new ResponseBody();

            try
            {
                var existingRequest = await _dbSet.FirstOrDefaultAsync(request =>
                    request.client_id == requestdto.client_id &&
                    request.request_status == RequestStatus.InProgress);


                if (existingRequest == null)
                {
                    responseBody.status = false;
                    responseBody.message = "Sorry, Request Not Found.";
                    return responseBody;
                }

                else
                {
                    existingRequest.request_status = requestdto.request_status;
                    existingRequest.creditor_name = requestdto.creditor_name;
                    existingRequest.open_date = requestdto.open_date;
                    existingRequest.confirmation_number = requestdto.confirmation_number;
                    existingRequest.credit_repair_id = requestdto.credit_repair_id;

                    await _context.SaveChangesAsync();
                }

                var response = await _dbSet.FirstOrDefaultAsync(request => request.RequestId == existingRequest.RequestId);

                responseBody.status = true;
                responseBody.message = "Request Updated Successfully.";
                responseBody.data = response;
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
