using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolaMy.Core.DTOs.BuildingDTOs
{
    public class AmenityBuildingDTO
    {
        public int AmenityId { get; set; }

        public string Name { get; set; } = null!;

        public string RoomCount { get; set; } = null!;
    }
    public class RoomTypeListBuildingDTO
    {
        public int RoomTypeId { get; set; }

        public string Name { get; set; } = null!;

        public string RoomCount { get; set; } = null!;
    }
}
