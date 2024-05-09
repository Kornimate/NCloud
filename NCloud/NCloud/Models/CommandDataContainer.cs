using System.Text.Json.Serialization;

namespace NCloud.Models
{
    public class CommandDataContainer
    {
        public string Name {  get; set; }
        public string Description { get; set; }

        [JsonConstructor]
        public CommandDataContainer(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
