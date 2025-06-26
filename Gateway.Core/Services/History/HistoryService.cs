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

        public async Task<IEnumerable<HistoryItem>> GetUserHistoryAsync(
            int userId,
            ContentType? contentType = null,
            int skip = 0,
            int take = 50)
        {
            return await _historyRepository.GetByUserIdAsync(userId, contentType, skip, take);
        }

        public async Task ClearUserHistoryAsync(int userId)
        {
            var userHistory = await _historyRepository.GetByUserIdAsync(userId);
            foreach (var item in userHistory)
            {
                await _historyRepository.DeleteAsync(item.Id);
            }
        }
    }
}
