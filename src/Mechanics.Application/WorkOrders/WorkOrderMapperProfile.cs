using AutoMapper;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.WorkOrders;

public class WorkOrderMapperProfile : Profile
{
    public WorkOrderMapperProfile()
    {
        CreateMap<WorkOrder, GetWorkOrderResponse>()
            .ForMember(dest => dest.ProductIds,
                opt => opt.MapFrom(src =>
                    src.Products != null ? src.Products.Select(p => p.Id) : Enumerable.Empty<Guid>()))
            .ForMember(dest => dest.ServiceCatalogIds,
                opt => opt.MapFrom(src =>
                    src.ServiceCatalog != null ? src.ServiceCatalog.Select(s => s.Id) : Enumerable.Empty<Guid>()));
    }
}
