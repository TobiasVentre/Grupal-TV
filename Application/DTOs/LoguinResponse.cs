namespace Application.DTOs
{
    public class LoginResponse
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool IsActive { get; set; }

        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
