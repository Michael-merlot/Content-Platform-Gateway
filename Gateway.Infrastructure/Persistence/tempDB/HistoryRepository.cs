using Gateway.Core.Interfaces.History;
using Gateway.Core.Models.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Persistence.tempDB;

public class HistoryRepository : IHistoryRepository
{
    public Task<HistoryItem> AddAsync(HistoryItem historyItem)
    {
        return Task.FromResult(historyItem);
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<HistoryItem>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<HistoryItem?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<HistoryItem>> GetByUserIdAsync(Guid userId, ContentType? contentType = null)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(HistoryItem historyItem)
    {
        throw new NotImplementedException();
    }
}
