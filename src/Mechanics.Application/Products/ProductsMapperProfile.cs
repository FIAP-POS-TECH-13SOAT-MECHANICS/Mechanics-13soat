using AutoMapper;
using Mechanics.Application.Products.Requests;
using Mechanics.Application.Products.Responses;
using Mechanics.Domain.Products;

namespace Mechanics.Application.Products;

public class ProductsMapperProfile : Profile
{
    public ProductsMapperProfile()
    {
        //CreateMap<PersonalDocumentRequest, PersonalDocument>()
            //.ConstructUsing(src => new PersonalDocument(src.Type!.Value, src.Number));
        CreateMap<CreateProductRequest, Product>();

        CreateMap<Product, GetProductResponse>();
    }
}
