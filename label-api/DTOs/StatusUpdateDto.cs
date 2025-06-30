using label_api.Models;

namespace label_api.DTOs;

public class StatusUpdateDto
{
    public int ReleaseId { get; set; }
    public ReleaseStatus Status { get; set; }
    public string RejectReason { get; set; }
} 