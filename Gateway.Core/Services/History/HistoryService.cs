using Gateway.Core.Interfaces.History;
using Gateway.Core.Models.History;
namespace Gateway.Core.Services.History
{
    public class HistoryService : IHistoryService
    {
        private readonly IHistoryRepository _historyRepository;

        public HistoryService(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public async Task<HistoryItem> AddHistoryItemAsync(HistoryItem historyItem)
        {
            return await _historyRepository.AddAsync(historyItem);
        }

        public async Task<IEnumerable<HistoryItem>> GetUserHistoryAsync(Guid userId, ContentType? contentType = null)
        {
            return await _historyRepository.GetByUserIdAsync(userId, contentType);
        }
    }
}
