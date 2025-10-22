namespace Application.DTOs
{
    public class CreateUserResponse
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool IsActive { get; set; }
    }
}
