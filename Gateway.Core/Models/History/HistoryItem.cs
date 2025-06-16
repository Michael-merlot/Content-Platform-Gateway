namespace Gateway.Core.Models.History;
public class HistoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public Guid ContentId { get; set; }
    public ContentType ContentType { get; set; }

    public DateTimeOffset ViewedAt { get; set; }
}
public enum ContentType
{
    Unknown = 0,
    Video = 1,
    Article = 2,
    Image = 3,
    Audio = 4,
    LiveStream = 5
}
