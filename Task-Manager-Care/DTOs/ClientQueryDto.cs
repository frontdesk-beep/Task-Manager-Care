namespace Task_Manager_Care.DTOs
{
    public class ClientQueryDto
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? SortBy { get; set; } = "ClientName";
        public string? SortOrder { get; set; } = "asc";

        public string? SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
