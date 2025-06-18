using Gateway.Core.Models.History;

namespace Gateway.Api.Models.History
{
    public class AddHistoryResponse// для ответа
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }

        public Guid ContentId { get; set; }

        public ContentType ContentType { get; set; }

        public DateTimeOffset ViewedAt { get; set; }

    }
}
