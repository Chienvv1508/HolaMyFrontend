namespace HolaMyFrontend.Models.BuildingDTOs
{
    public class SavedBuildingRequestDTO
    {
        public int BuildingId { get; set; }
        public int? RoomId { get; set; }
    }
    public class SavedBuildingResponseDTO
    {
        public int SavedId { get; set; }
        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string? RoomTypeName { get; set; }
        public decimal? RoomPrice { get; set; }
        public string? AddressDetail { get; set; }
        public WardDTO Ward { get; set; } = new WardDTO();
        public string? ThumbnailUrl { get; set; }
    }
}
