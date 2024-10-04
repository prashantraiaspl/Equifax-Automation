using Equifax.Api.Data;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Domain.Enums;
using Equifax.Api.Domain.Models;
using Equifax.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net;

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


        public async Task<ResponseBody> CheckRequestQueue(DisputeRequestDto requestDto)
        {
            var responseBody = new ResponseBody();

            try
            {
                var query = _dbSet.Where(request => request.user_name == requestDto.user_name);

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
    }
}
