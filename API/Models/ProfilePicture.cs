public class ProfilePicture
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public byte[] Picture { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Assurer que `CreatedAt` est en UTC
    public User User { get; set; }
}