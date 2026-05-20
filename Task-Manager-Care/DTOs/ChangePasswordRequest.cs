namespace Task_Manager_Care.DTOs
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
