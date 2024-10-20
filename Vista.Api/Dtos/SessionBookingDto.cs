using System.ComponentModel.DataAnnotations;

namespace Vista.Api.Dtos
{
    public class SessionBookingDto
    {
        public int SessionId { get; set; }
        public int TrainerId { get; set; }
        [DataType(DataType.Date)]
        public DateTime SessionDate { get; set; }
        public string TrainerName { get; set; } = null!;
        public string BookingReference { get; set; } = null!;
    }
}
