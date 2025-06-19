namespace Gateway.Core.Events;

// Это пример события, которое может быть обработано для создания уведомления
public class NewContentCreatedEvent
{
    public Guid ContentId { get; set; }
    public Guid AuthorId { get; set; }
    public string ContentTitle { get; set; }
    public string ContentUrl { get; set; }
    public List<Guid> SubscriberIds { get; set; } // Например, список подписчиков автора
}
