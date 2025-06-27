using System.Threading.Tasks;
using TrainingTask.Server.Models;

namespace TrainingTask.Server.Services
{
    public interface IDialogflowService
    {
        Task<(string fulfillmentText, string intentName)> DetectIntentAsync(ChatRequest request, string credentialsJson, string languageCode);
    }
}
