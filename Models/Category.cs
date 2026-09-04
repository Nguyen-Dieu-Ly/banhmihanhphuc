namespace banhmihanhphuc.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public List<Food> Foods { get; set; } = new();
    }
}