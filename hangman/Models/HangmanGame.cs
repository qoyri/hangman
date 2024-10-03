namespace HangmanApp.Models
{
    public class HangmanGame
    {
        public string Word { get; private set; }
        public string MaskedWord { get; private set; }
        public HashSet<char> GuessedLetters { get; private set; }

        public HangmanGame(string word)
        {
            Word = word.ToUpper();
            MaskedWord = new string('_', Word.Length);
            GuessedLetters = new HashSet<char>();

            Console.WriteLine($"Initial MaskedWord: {MaskedWord}, Word: {Word}");
        }

        public bool GuessLetter(char letter)
        {
            letter = char.ToUpper(letter);
            Console.WriteLine($"Guessing letter: {letter}");

            if (GuessedLetters.Contains(letter))
            {
                Console.WriteLine($"Letter {letter} already guessed.");
                return false;
            }

            GuessedLetters.Add(letter);
            bool found = false;
            var maskedChars = MaskedWord.ToCharArray();

            for (int i = 0; i < Word.Length; i++)
            {
                if (Word[i] == letter)
                {
                    maskedChars[i] = letter;
                    found = true;
                }
            }

            MaskedWord = new string(maskedChars);
            Console.WriteLine($"Updated MaskedWord: {MaskedWord}");

            return found;
        }

        public bool IsWordFound()
        {
            return !MaskedWord.Contains('_');
        }
    }
}