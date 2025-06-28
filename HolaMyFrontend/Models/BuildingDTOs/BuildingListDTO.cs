namespace HolaMyFrontend.Models.BuildingDTOs
{
    public class BuildingListDTO
    {
        public int BuildingId { get; set; }
        public int ProviderId { get; set; }
        public string BuildingName { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public decimal? DisplayPrice { get; set; } // Giá thấp nhất của phòng trong tòa nhà
        public string WardName { get; set; }
        public int UpdatedHoursBefore { get; set; }
    }

    public class BuildingFilterDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? RoomTypeId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? WardId { get; set; }
        public string? SortBy { get; set; }
        public string? Search { get; set; }
    }
    public class WardDTO
    {
        public int WardId { get; set; }
        public string WardName { get; set; } = string.Empty;
    }
}
