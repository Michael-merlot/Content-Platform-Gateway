using Gateway.Core.Models.History;

namespace Gateway.Core.Interfaces.History
{
    public interface IHistoryService 
    {
        Task<HistoryItem> AddHistoryItemAsync(HistoryItem historyItem);
        Task ClearUserHistoryAsync(Guid userId);
        Task<IEnumerable<HistoryItem>> GetUserHistoryAsync(Guid userId, ContentType? contentType = null, int skip = 0, int take = 50);
    }
}
