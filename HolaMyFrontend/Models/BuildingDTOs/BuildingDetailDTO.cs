using HolaMyFrontend.Models.BuildingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolaMy.Core.DTOs.BuildingDTOs
{
    public class BuildingDetailDTO
    {
        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string? AddressDetail { get; set; }
        public string? Description { get; set; }
        public WardDTO Ward { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int UpdatedHoursBefore { get; set; }
        public List<BuildingImageDTO> BuildingImages { get; set; } = new List<BuildingImageDTO>();
        public List<RoomDTO> Rooms { get; set; } = new List<RoomDTO>();
        public ProviderDTO Provider { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
    public class ProviderDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime JoinDate { get; set; }
        public int PostCount { get; set; }
        public string PhoneNumber { get; set; }

        public string Avatar { get; set; }
    }
    public class BuildingImageDTO
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool? IsThumbnail { get; set; }
    }

    public class RoomDTO
    {
        public int RoomId { get; set; }
        public RoomTypeDTO RoomType { get; set; } = new RoomTypeDTO();
        public float? AreaM2 { get; set; }
        public decimal? Price { get; set; }
        public bool? IsAvailable { get; set; }
        public string? Description { get; set; }
        public List<RoomImageDTO> ImageUrl { get; set; } = new List<RoomImageDTO>();
        public List<AmenityDTO> Amenities { get; set; } = new List<AmenityDTO>();
        public int? Status { get; set; }
    }
    public class RoomImageDTO
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool? IsThumbnail { get; set; }
    }
    public class RoomTypeDTO
    {
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
    }
    public class AmenityDTO
    {
        public int AmenityId { get; set; }
        public string? IconUrl { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
