using System.Threading.Tasks;
using TrainingTask.Server.Models;

namespace TrainingTask.Server.Services
{
    public interface IDialogflowService
    {
        Task<IntentDTO> DetectIntentAsync(ChatRequest request, string credentialsJson, string languageCode);
    }
}
