using CleanArchMvc.Application.DTOs;

namespace CleanArchMvc.Application.Interface;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDTO>> GetCategories();
    Task<CategoryDTO> GetById(int? id);
    Task Add(CategoryDTO categoryDTO);
    Task Update(CategoryDTO categoryDTO);
    Task Delete(int? id);
}
