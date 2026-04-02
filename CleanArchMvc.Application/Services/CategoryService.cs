using AutoMapper;
using CleanArchMvc.Application.DTOs;
using CleanArchMvc.Application.Interface;
using CleanArchMvc.Domain.Entities;
using CleanArchMvc.Domain.Interfaces;

namespace CleanArchMvc.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private ICategoryRepositry _categoryRepositry;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepositry category, IMapper mapper)
        {
            _categoryRepositry = category;
            _mapper = mapper;            
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategories()
        {
            var categoriesEntity = await _categoryRepositry.GetCategories();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categoriesEntity);
        }

        public async Task<CategoryDTO> GetById(int? id)
        {
            var categoryEntity = await _categoryRepositry.GetById(id);
            return _mapper.Map<CategoryDTO>(categoryEntity);
        }

        public async Task Add(CategoryDTO categoryDTO)
        {
            var categoryEntity = _mapper.Map<Category>(categoryDTO);
            await _categoryRepositry.Create(categoryEntity);
        }

        public async Task Update(CategoryDTO categoryDTO)
        {
            var categoryEntity = _mapper.Map<Category>(categoryDTO);
            await _categoryRepositry.Update(categoryEntity);
        }

        public async Task Delete(int? id)
        {
            var categoryEntity = _categoryRepositry.GetById(id).Result;
            await _categoryRepositry.Remove(categoryEntity);
        }
    }
}
