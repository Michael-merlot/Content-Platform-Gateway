using Gateway.Core.Models.History;
using System.ComponentModel.DataAnnotations;
namespace Gateway.Api.Models.History
{
    public class AddHistoryRequest
    {
        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Content ID is required.")]
        public Guid ContentId { get; set; }

        [Required(ErrorMessage = "Content type is required.")]
        public ContentType ContentType { get; set; }

        public string? Name { get; set; }
    }

}
