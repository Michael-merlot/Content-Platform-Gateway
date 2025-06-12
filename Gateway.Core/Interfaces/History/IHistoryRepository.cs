using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gateway.Core.Models.History;

namespace Gateway.Core.Interfaces.History
{
    public interface IHistoryRepository
    {
        Task<HistoryItem> AddAsync(HistoryItem historyItem);

        Task<HistoryItem?> GetByIdAsync(Guid id);

        Task<IEnumerable<HistoryItem>> GetAllAsync();

        Task<IEnumerable<HistoryItem>> GetByUserIdAsync(Guid userId, ContentType? contentType = null);

        Task UpdateAsync(HistoryItem historyItem);

        Task DeleteAsync(Guid id);
    }
}