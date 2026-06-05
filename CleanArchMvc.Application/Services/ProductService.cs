using AutoMapper;
using CleanArchMvc.Application.DTOs;
using CleanArchMvc.Application.Interface;
using CleanArchMvc.Application.Products.Commands;
using CleanArchMvc.Application.Products.Queries;
using CleanArchMvc.Domain.Entities;
using CleanArchMvc.Domain.Interfaces;
using MediatR;

namespace CleanArchMvc.Application.Services;

public class ProductService : IProductService
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public ProductService(IMapper mapper, IMediator mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<IEnumerable<ProductDTO>> GetProducts()
    {
        var productsQuery = new GetProductsQuery();

        if(productsQuery == null)
            throw new Exception($"Entity could not be loaded.");

        var result = await _mediator.Send(productsQuery);

        return _mapper.Map<IEnumerable<ProductDTO>>(result);
    }

    public async Task<ProductDTO> GetById(int? id)
    {
        var productByIdQuery = new GetProductByIdQuery(id.Value);

        if (productByIdQuery == null)
            throw new Exception($"Entity could not be loaded.");

        var result = await _mediator.Send(productByIdQuery);

        return _mapper.Map<ProductDTO>(result);
    }

    //public async Task<ProductDTO> GetProductCategory(int? id)
    //{
    //    var productByIdQuery = new GetProductByIdQuery(id.Value);

    //    if (productByIdQuery == null)
    //        throw new Exception($"Entity could not be loaded.");

    //    var result = await _mediator.Send(productByIdQuery);

    //    return _mapper.Map<ProductDTO>(result);
    //}

    public async Task Add(ProductDTO productDTO)
    {
        var createProductCommand = _mapper.Map<ProductCreateCommand>(productDTO);
        await _mediator.Send(createProductCommand);
    }

    public async Task Update(ProductDTO productDTO)
    {
        var updateProductCommand = _mapper.Map<ProductUpdateCommand>(productDTO);
        await _mediator.Send(updateProductCommand);
    }

    public async Task Remove(int? id)
    {
        var productremoveCommand = new ProductRemoveCommand(id.Value);
        if(productremoveCommand == null)
            throw new Exception($"Entity could not be loaded.");

        await _mediator.Send(productremoveCommand);
    }
}
