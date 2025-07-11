using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolaMy.Core.DTOs.ReportDTOs
{
    public class SubmitReportRequest
    {
        public int ReportedOwnerId { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public IFormFileCollection EvidenceFiles { get; set; }
        public bool IsAnonymous { get; set; }

    }
    
    public class ReportResponseDto
    {
        public int ReportId { get; set; }
        public int? ReporterUserId { get; set; }
        public string ReporterName { get; set; } 
        public int ReportedOwnerId { get; set; }
        public string ReportedOwnerName { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public List<string> EvidenceUrls { get; set; }
        public bool IsAnonymous { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ProviderResponse { get; set; }
        public string AdminNote { get; set; }
    }
    public enum ReportStatus
    {
        PENDING,          // Đang chờ xử lý
        PENDING_REVIEW,   // Đang chờ xem xét
        WAITING_RESPONSE, // Đang chờ phản hồi từ chủ trọ
        RESOLVED,         // Đã giải quyết
        REJECTED          // Đã từ chối
    }
}
