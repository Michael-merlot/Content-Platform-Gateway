using Gateway.Core.Models.History;

namespace Gateway.Core.Interfaces.History
{
    public interface IHistoryService 
    {
        Task<HistoryItem> AddHistoryItemAsync(HistoryItem historyItem);

        Task<IEnumerable<HistoryItem>> GetUserHistoryAsync(Guid userId, ContentType? contentType = null);
        Task<int> GetViewsCountByContentIdAsync(Guid contentId);
        Task<int> GetViewsCountByContentTypeAsync(ContentType contentType);
    }
}
