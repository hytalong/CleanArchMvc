using CleanArchMvc.Domain.Entities;
using MediatR;

namespace CleanArchMvc.Application.Products.Commands;

public class ProductRemoveCommnd : IRequest<Product>
{
    public int Id { get; set; }
    public ProductRemoveCommnd(int id)
    {
        Id = id;
    }
}
