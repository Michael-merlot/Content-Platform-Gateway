using Gateway.Core.Interfaces.History;
using Gateway.Core.Models.History;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Persistence.tempDB;

public class HistoryRepository : IHistoryRepository
{
    private static readonly ConcurrentDictionary<Guid, HistoryItem> _historyItems = new ConcurrentDictionary<Guid, HistoryItem>();

    public Task<HistoryItem> AddAsync(HistoryItem historyItem)
    {
        
         historyItem.Id = Guid.NewGuid();

        _historyItems.TryAdd(historyItem.Id, historyItem); // Пытаемся добавить элемент

        return Task.FromResult(historyItem); // Возвращаем добавленный элемент
    }

    public Task DeleteAsync(Guid id)
    {
        _historyItems.TryRemove(id, out _); // Пытаемся удалить элемент по ID
        return Task.CompletedTask; // Возвращаем завершенный таск
    }

    public Task<IEnumerable<HistoryItem>> GetAllAsync()
    {
        // Возвращаем все элементы из коллекции
        return Task.FromResult<IEnumerable<HistoryItem>>(_historyItems.Values.ToList());
    }

    public Task<HistoryItem?> GetByIdAsync(Guid id)
    {
        // Пытаемся получить элемент по ID. TryGetValue безопаснее, чем прямой доступ.
        _historyItems.TryGetValue(id, out HistoryItem? historyItem);
        return Task.FromResult(historyItem); // Может вернуть null, если не найдено
    }

    public Task<IEnumerable<HistoryItem>> GetByUserIdAsync(Guid userId, ContentType? contentType = null, int skip = 0, int take = 50)
    {
        var userHistory = _historyItems.Values.Where(item => item.UserId == userId);

        if (contentType.HasValue && contentType.Value != ContentType.Unknown)
        {
            userHistory = userHistory.Where(item => item.ContentType == contentType.Value);
        }

        userHistory = userHistory.OrderByDescending(item => item.ViewedAt)
                                 .Skip(skip)
                                 .Take(take);

        return Task.FromResult<IEnumerable<HistoryItem>>(userHistory.ToList());
    }

    public Task UpdateAsync(HistoryItem historyItem)
    {
        // Пытаемся обновить элемент.
        // Сначала удаляем старый, потом добавляем новый
        _historyItems.AddOrUpdate(
            historyItem.Id,
            historyItem, // Значение для добавления, если ключа нет
            (key, existingItem) => historyItem // Функция обновления, если ключ есть
        );
        return Task.CompletedTask;
    }


}
