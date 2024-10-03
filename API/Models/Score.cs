namespace HangmanApi.Models
{
    public class Score
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TotalScore { get; set; }
        public DateTime LastUpdated { get; set; }

        public User User { get; set; } // Propriété de navigation vers l'utilisateur
    }
}