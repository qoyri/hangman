using HangmanApi.Models;

public class GameState
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int WordId { get; set; }
    public Word Word { get; set; }
    public string Difficulty { get; set; }
    public double ComboMultiplier { get; set; } // Pas de valeur par défaut ici
    public int Score { get; set; }
    public bool IsActive { get; set; }
}