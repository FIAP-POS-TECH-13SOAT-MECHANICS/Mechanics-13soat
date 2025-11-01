using AutoMapper;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.WorkOrders;

public class WorkOrderMapperProfile : Profile
{
    public WorkOrderMapperProfile()
    {
        CreateMap<WorkOrder, GetWorkOrderResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AccessKey, opt => opt.MapFrom(src => src.AccessKey))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
            .ForMember(dest => dest.LastUpdate, opt => opt.MapFrom(src => src.LastUpdate))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.VehicleId, opt => opt.MapFrom(src => src.VehicleId))
            .ForMember(dest => dest.ReportedProblem, opt => opt.MapFrom(src => src.ReportedProblem))
            .ForMember(dest => dest.Observations, opt => opt.MapFrom(src => src.Observations))
            .ForMember(dest => dest.ProductIds, opt => opt.MapFrom(src => src.Products != null ? src.Products.Select(p => p.Id) : Enumerable.Empty<Guid>()))
            .ForMember(dest => dest.ServiceCatalogIds, opt => opt.MapFrom(src => src.ServiceCatalog != null ? src.ServiceCatalog.Select(s => s.Id) : Enumerable.Empty<Guid>()))
            ;
    }
}
