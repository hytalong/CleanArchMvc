using AutoMapper;
using CleanArchMvc.Application.DTOs;
using CleanArchMvc.Application.Interface;
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

    //public async Task<ProductDTO> GetById(int? id)
    //{
    //    var productEntity = await _productRepository.GetByIdAsync(id);
    //    return _mapper.Map<ProductDTO>(productEntity);
    //}

    //public async Task<ProductDTO> GetProductCategory(int? id)
    //{
    //    var productEntity = await _productRepository.GetProductCategoryAsync(id);
    //    return _mapper.Map<ProductDTO>(productEntity);
    //}

    //public async Task Add(ProductDTO productDTO)
    //{
    //    var productEntity = _mapper.Map<Product>(productDTO);
    //    await _productRepository.CreateAsync(productEntity);
    //}

    //public async Task Update(ProductDTO productDTO)
    //{
    //    var productEntity = _mapper.Map<Product>(productDTO);
    //    await _productRepository.UpdateAsync(productEntity);
    //}

    //public async Task Remove(int? id)
    //{
    //    var productEntity = _productRepository.GetByIdAsync(id).Result;
    //    await _productRepository.RemoveAsync(productEntity);
    //}
}
