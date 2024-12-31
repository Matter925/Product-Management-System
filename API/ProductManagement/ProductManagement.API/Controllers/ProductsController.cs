using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using ProductManagement.API.Helpers;
using ProductManagement.API.ResponseModule;
using ProductManagement.EFCore.Pagination;
using ProductManagement.EFCore.ResourceParams;
using ProductManagement.Services.Interfaces;
using ProductManagement.API.DTOs;
using ProductManagement.EFCore.Models;

namespace Bashrahil.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController(IProductsService service, IMapper mapper) : ControllerBase
{
    private string EntityName { get; } = "Products";

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Get([FromQuery] ResourceParams resourceParams)
    {
        var entities = await service.GetAllAsync(false, resourceParams: resourceParams, selector: p => new ProductDto { Id = p.Id, Name = p.Name, Description = p.Description,Price=p.Price,CreatedDate=p.CreatedDate}) as PagedList<ProductDto>;
        if (entities != null)
        {
            var paginationMetaData = PaginationMetaData<ProductDto>.CreatePaginationMetaData(entities);
            Response.Headers.Append("X-Pagination", paginationMetaData);
        }
        return Ok(entities ?? new List<ProductDto>());
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> Get(int id)
    {
        var entity = await service.GetByIdAsync(id, false, selector: p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CreatedDate = p.CreatedDate
        });
        if (entity == null)
            return NotFound(new ApiResponse(404));

        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var mappedEntity = mapper.Map<Product>(dto);
        var id = await service.CreateAsync(mappedEntity);
        if (id == 0 || id == null)
            return BadRequest(new ApiResponse(400));

        return Ok(new ApiResponse(200, id: id.Value));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateProductDto dto)
    {
        var mappedEntity = mapper.Map<Product>(dto);
        var isUpdated = await service.UpdateAsync(id: id, model: mappedEntity, directUpdate: false, updateType: typeof(UpdateProductDto));
        if (!isUpdated)
            return NotFound(new ApiResponse(404));

        return Ok(new ApiResponse(200));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var isDeleted = await service.DeleteAsync(id);
        if (!isDeleted)
            return NotFound(new ApiResponse(400));

        return Ok(new ApiResponse(200));
    }
}
