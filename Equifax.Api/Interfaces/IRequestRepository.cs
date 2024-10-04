using Equifax.Api.Domain.DTOs;

namespace Equifax.Api.Interfaces
{
    public interface IRequestRepository
    {
        Task<ResponseBody> CheckRequestQueue(DisputeRequestDto requestDto);
        Task<ResponseBody> InsertRequest(DisputeRequestDto requestDto);
    }
}
