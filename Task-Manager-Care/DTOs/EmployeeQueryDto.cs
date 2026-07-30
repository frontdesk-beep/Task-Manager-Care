namespace Task_Manager_Care.DTOs
{
    public class EmployeeQueryDto
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? Role { get; set; }
        public string? SortBy { get; set; } = "Name";
        public string? SortOrder { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }
}
