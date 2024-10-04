using AutoMapper;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Domain.Models;

namespace Equifax.Api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DisputeRequestDto, LoginCredentialRequestDto>();
        }
    }
}
