using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommerceAPI.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [JsonProperty("title")]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "TEXT")]
        public string Description { get; set; } = string.Empty;
        [JsonProperty("image")]
        public string ImageURL { get; set; } = string.Empty;
        public string? Category { get; set; }
        public double Price { get; set; }
    }
}
