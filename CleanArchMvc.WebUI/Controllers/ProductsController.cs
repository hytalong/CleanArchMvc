using CleanArchMvc.Application.DTOs;
using CleanArchMvc.Application.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CleanArchMvc.WebUI.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    public ProductsController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var product = await _productService.GetProducts();
        return View(product);
    }

    [HttpGet()]
    public async Task<IActionResult> Create()
    {
        ViewBag.CategoryId =
            new SelectList(await _categoryService.GetCategories(), "Id", "Name");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductDTO productDTO)
    {
        if (ModelState.IsValid)
        {
            await _productService.Add(productDTO);
            return RedirectToAction(nameof(Index));
        }
        return View(productDTO);
    }
}
