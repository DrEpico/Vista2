using System.ComponentModel.DataAnnotations;

namespace Vista.Api.Dtos
{
    public class SessionBookingRequestDto
    {
        [Required]
        public int SessionId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime SessionDate { get; set; }
    }
}
