namespace IconFilers.Application.DTOs
{
    public class AdminDashboardDto
    {
        public IEnumerable<object> DocumentsCount { get; set; } = new List<object>();
        public IEnumerable<object> VerifiedDocumentsCount { get; set; } = new List<object>();
        public IEnumerable<object> PendingDocumentsCount { get; set; } = new List<object>();
        public IEnumerable<object> RejectedDocumentsCount { get; set; } = new List<object>();
        public IEnumerable<IdNameDto> Users { get; set; } = new List<IdNameDto>();
    }

    public class UserDashboardDto
    {
        public MyAssignmentsDto Assignments { get; set; } = new MyAssignmentsDto();
        public IEnumerable<object> DocumentsCount { get; set; } = new List<object>();
        public IEnumerable<object> VerifiedDocumentsCount { get; set; } = new List<object>();
        public IEnumerable<object> PendingDocumentsCount { get; set; } = new List<object>();
        public IEnumerable<object> RejectedDocumentsCount { get; set; } = new List<object>();
    }
}
