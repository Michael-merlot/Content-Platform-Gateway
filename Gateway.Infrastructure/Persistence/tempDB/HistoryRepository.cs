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

    public Task<IEnumerable<HistoryItem>> GetByUserIdAsync(Guid userId, ContentType? contentType = null)
    {
        // Фильтруем историю по userId
        var userHistory = _historyItems.Values.Where(item => item.UserId == userId);

        // Если указан contentType, фильтруем дополнительно
        if (contentType.HasValue && contentType.Value != ContentType.Unknown)
        {
            userHistory = userHistory.Where(item => item.ContentType == contentType.Value);
        }

        // Сортируем по дате просмотра в убывающем порядке (самые новые сверху)
        userHistory = userHistory.OrderByDescending(item => item.ViewedAt);

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

    public Task<int> CountViewsByContentIdAsync(Guid contentId)
    {
        // Считаем количество записей, у которых ContentId совпадает
        var count = _historyItems.Values.Count(item => item.ContentId == contentId);
        return Task.FromResult(count);
    }

    public Task<int> CountViewsByContentTypeAsync(ContentType contentType)
    {
        // Считаем количество записей, у которых ContentType совпадает
        var count = _historyItems.Values.Count(item => item.ContentType == contentType);
        return Task.FromResult(count);
    }
}
