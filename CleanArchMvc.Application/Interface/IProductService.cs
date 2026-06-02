using CleanArchMvc.Application.DTOs;

namespace CleanArchMvc.Application.Interface;

public interface IProductService
{
    Task<IEnumerable<ProductDTO>> GetProducts();
    Task<ProductDTO> GetById(int? id);

    Task<ProductDTO> GetProductCategory(int? id);
    Task Add(ProductDTO productDTO);
    Task Update(ProductDTO productDTO);
    Task Remove(int? id);
}
