using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Company
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public List<User> Owners { get; set; } = new();
    }
}
