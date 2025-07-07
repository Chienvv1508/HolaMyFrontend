namespace HolaMyFrontend.Models.RoomTypeDTOs
{

    public class RoomTypeListDTO
    {
        public int RoomTypeId { get; set; }

        public string RoomTypeName { get; set; } = null!;

        public int? RoomCount { get; set; } = null!;
        public string? Description { get; set; }

        public int? CreatedBy { get; set; }
        public string? Creater { get; set; }
        public int? UpdatedBy { get; set; }
        public string? Updater { get; set; }
        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
