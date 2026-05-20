using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace RockoCloud.BusinessLogic.Mapping
{
    public class TenantProfile : Profile
    {
        public TenantProfile() 
        {
            CreateMap<Models.Entities.Tenant, Models.DTO.TenantDTO>();
        }
    }
}
