using Equifax.Api.Domain.DTOs;
using Equifax.Api.Domain.Models;

namespace Equifax.Api.Interfaces
{
    public interface IRequestRepository
    {
        Task<ResponseBody> CheckRequestQueue(DisputeRequestDto requestDto);
        Task<ResponseBody> InsertRequest(DisputeRequestDto requestDto);
        Task<ResponseBody> UpdateRequest(RequestMaster response);
    }
}
