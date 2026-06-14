namespace Task_Manager_Care.DTOs
{
    public class CreateClientDto
    {
        public string ClientName { get; set; } 
        public int ClientCategoryId { get; set; }
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public int CreatedById { get; set; }
    }
}
