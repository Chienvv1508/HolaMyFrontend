namespace HolaMyFrontend.Models.AmenityDTOs
{
    public class AmenityListDTO
    {
        public int AmenityId { get; set; }

        public string Name { get; set; } = null!;

        public string? IconUrl { get; set; }

        public string? Description { get; set; }

        public int? CreatedBy { get; set; }

        public string? Creater { get; set; }

        public int? UpdatedBy { get; set; }

        public string? Updater { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

    }
}
