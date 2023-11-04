using CommerceAPI.Models;
using Newtonsoft.Json;

namespace CommerceAPI.Data
{
    public class ProductSeeder
    {
        private readonly CommerceAPIContext _context;

        public ProductSeeder(CommerceAPIContext context)
        {
            _context = context;
        }

        public async Task Seed()
        {
            Console.WriteLine("Checking for products in database...");
            if (!_context.Products.Any())
            {
                Console.WriteLine("Seeding products...");
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        string apiUrl = "https://fakestoreapi.com/products?limit=10";

                        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonContent = await response.Content.ReadAsStringAsync();
                            List<Product> products = JsonConvert.DeserializeObject<List<Product>>(jsonContent);

                            foreach (var product in products)
                            {
                                product.Id = 0; // Assuming '0' means a new auto-generated ID.
                            }

                            _context.Products.AddRange(products);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            Console.WriteLine($"API request failed with status code: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Products already exist in database!");
            }
        }
    }
}
