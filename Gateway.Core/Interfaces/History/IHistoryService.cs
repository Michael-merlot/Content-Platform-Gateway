using Gateway.Core.Models.History;

namespace Gateway.Core.Interfaces.History
{
    public interface IHistoryService 
    {
        Task<HistoryItem> AddHistoryItemAsync(HistoryItem historyItem);
        Task ClearUserHistoryAsync(int userId);
        Task<IEnumerable<HistoryItem>> GetUserHistoryAsync(int userId, ContentType? contentType = null, int skip = 0, int take = 50);
    }
}
