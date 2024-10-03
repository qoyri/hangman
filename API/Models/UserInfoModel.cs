namespace HangmanApi.Models
{
    public class UserInfoModel
    {
        public string UserName { get; set; }
        public int TotalScore { get; set; }
        public string CurrentWord { get; set; }
        public int CurrentWordScore { get; set; }
        public double ComboMultiplier { get; set; }
        public string Difficulty { get; set; } // Ajout de la difficulté
        public byte[] ProfilePicture { get; set; } // Ajout de l'image de profil
    }
}