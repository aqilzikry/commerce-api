using CommerceAPI.Data;
using CommerceAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly CommerceAPIContext dBContext;

        public ProductsController(CommerceAPIContext dBContext)
        {
            this.dBContext = dBContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            IQueryable<Product> query = dBContext.Products;

            var products = await query.ToListAsync();

            if (products == null || products.Count == 0)
            {
                return Ok(new List<Product>());
            }

            return Ok(products);
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct(string category)
        {
            var products = await dBContext.Products
                .Where(p => p.Category == category)
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound("No products found under the specified category.");
            }

            return Ok(products);
        }
    }
}
