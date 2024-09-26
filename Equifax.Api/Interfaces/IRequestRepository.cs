using Equifax.Api.Domain.DTOs;

namespace Equifax.Api.Interfaces
{
    public interface IRequestRepository
    {
        Task<ResponseBody> CheckRequestQueue(PayloadRequestDto requestDto);
        Task<ResponseBody> InsertRequest(PayloadRequestDto requestDto);
    }
}
