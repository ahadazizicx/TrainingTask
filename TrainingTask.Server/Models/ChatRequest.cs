using System.Globalization;

namespace TrainingTask.Server.Models
{
    public class ChatRequest
    {
        public string SessionId { get; set; }
        public string Message { get; set; }
        public string JsonCreds { get; set; }
    }
}
