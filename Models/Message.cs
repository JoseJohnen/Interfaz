using System.Text.Json;

namespace Interfaz.Models
{
    public struct Message
    {
        public string Text { get; set; }

        public Message(string text)
        {
            Text = text;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
