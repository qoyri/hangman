using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using HangmanApp.Models;
using HangmanApp.Services;
using HangmanApp.Views;

namespace HangmanApp.ViewModels
{
    public class GameViewModel : ObservableObject
    {
        private HangmanGame _currentGame;
        private DispatcherTimer _timer;
        private TimeSpan _timeRemaining;
        private bool _isVictoryOverlayVisible;
        private readonly GameService _gameService;
        private readonly MainViewModel _mainViewModel;
        private TaskCompletionSource<bool> _continueTaskCompletionSource;
        private TaskCompletionSource<bool> _closeHintTaskCompletionSource;
        private int? _currentHintWordId;

        public GameViewModel(MainViewModel mainViewModel, string difficulty)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _gameService = new GameService();
            GuessCommand = new RelayCommand(OnGuess);
            ReturnCommand = new RelayCommand(OnReturn);
            ContinueCommand = new RelayCommand(OnContinue);
            ShowHintCommand = new RelayCommand(async _ => await OnShowHint());

            Difficulty = difficulty;

            InitializeFaultLimits(difficulty);
            InitializeTimer(difficulty);

            MaskedWord = "Loading...";
            Points = 0;
            ComboMultiplier = 1.0;

            UsedLetters = new ObservableCollection<char>();
            WrongLetters = new ObservableCollection<char>();

            VictoryControlViewModel = new VictoryControlViewModel(ContinueCommand);
            HintOverlayViewModel = new HintOverlayViewModel(new RelayCommand(CloseHintOverlay));

            Task.Run(async () => await StartGameAsync(difficulty));
        }

        public ObservableCollection<char> UsedLetters { get; }
        public ObservableCollection<char> WrongLetters { get; }

        public ICommand GuessCommand { get; }
        public ICommand ReturnCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand ShowHintCommand { get; }


        private string _maskedWord;
        private string _difficulty;
        private int _points;
        private int _faults;
        private int _faultsLimit;
        private double _comboMultiplier;

        public TimeSpan TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
            }
        }

        private void InitializeTimer(string difficulty)
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Décrémenter chaque seconde
            };
            _timer.Tick += Timer_Tick;

            switch (difficulty.ToLower())
            {
                case "easy":
                    TimeRemaining = TimeSpan.FromMinutes(3);
                    break;
                case "medium":
                    TimeRemaining = TimeSpan.FromMinutes(1);
                    break;
                case "hard":
                    TimeRemaining = TimeSpan.FromSeconds(30);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(difficulty), "Invalid difficulty level");
            }
        }

        private void InitializeFaultLimits(string difficulty)
        {
            switch (difficulty.ToLower())
            {
                case "easy":
                    FaultsLimit = 10;
                    break;
                case "medium":
                    FaultsLimit = 5;
                    break;
                case "hard":
                    FaultsLimit = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(difficulty), "Invalid difficulty level");
            }

            Faults = 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (TimeRemaining > TimeSpan.Zero)
            {
                TimeRemaining = TimeRemaining.Subtract(TimeSpan.FromSeconds(1));
            }
            else
            {
                _timer.Stop();
                DisplayGameOver();
            }
        }

        public string MaskedWord
        {
            get => _maskedWord;
            set
            {
                _maskedWord = value;
                OnPropertyChanged();
                Console.WriteLine($"MaskedWord updated: {_maskedWord}");
            }
        }

        public string Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                OnPropertyChanged();
                Console.WriteLine($"Difficulty updated: {_difficulty}");
            }
        }

        public int Points
        {
            get => _points;
            set
            {
                _points = value;
                OnPropertyChanged();
                Console.WriteLine($"Points updated: {_points}");
            }
        }

        public double ComboMultiplier
        {
            get => _comboMultiplier;
            set
            {
                _comboMultiplier = value;
                OnPropertyChanged();
                Console.WriteLine($"ComboMultiplier updated: {_comboMultiplier}");
            }
        }

        public int TotalScore
        {
            get => _mainViewModel?.TotalScore ?? 0; // Vérifiez nullité
            set
            {
                if (_mainViewModel != null)
                {
                    _mainViewModel.TotalScore = value;
                    OnPropertyChanged();
                    Console.WriteLine($"TotalScore updated: {_mainViewModel.TotalScore}");
                }
            }
        }

        public int Faults
        {
            get => _faults;
            set
            {
                _faults = value;
                OnPropertyChanged();
                Console.WriteLine($"Faults updated: {_faults}");
            }
        }

        public int FaultsLimit
        {
            get => _faultsLimit;
            private set
            {
                _faultsLimit = value;
                OnPropertyChanged();
                Console.WriteLine($"FaultsLimit updated: {_faultsLimit}");
            }
        }

        private bool _isHintOverlayVisible;

        public bool IsHintOverlayVisible
        {
            get => _isHintOverlayVisible;
            set
            {
                _isHintOverlayVisible = value;
                OnPropertyChanged();
            }
        }

        private async Task StartGameAsync(string difficulty)
        {
            try
            {
                var token = SecureStorage.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    MaskedWord = "Erreur: Token non trouvé.";
                    Console.WriteLine("Erreur: Token non trouvé.");
                    return;
                }

                string word = await _gameService.GetNewWordAsync(difficulty, token);
                if (word == null)
                {
                    MaskedWord = "Erreur: Mot non reçu.";
                    Console.WriteLine("Erreur: Mot non reçu.");
                    return;
                }

                _currentGame = new HangmanGame(word);
                MaskedWord = _currentGame?.MaskedWord ?? "Erreur: jeu non initialisé";

                ComboMultiplier = 1.0;

                _timer.Start(); // Démarrer le timer après la récupération du mot
            }
            catch (Exception ex)
            {
                MaskedWord = "Erreur lors de la récupération du mot.";
                Console.WriteLine($"Erreur lors de la récupération du mot : {ex.Message}");
            }
        }

        private async void OnGuess(object parameter)
        {
            if (_currentGame == null) return;

            if (parameter is char guessedLetter)
            {
                if (UsedLetters.Contains(guessedLetter))
                    return;

                UsedLetters.Add(guessedLetter);
                OnPropertyChanged(nameof(UsedLetters));

                bool found = _currentGame.GuessLetter(guessedLetter);
                MaskedWord = _currentGame.MaskedWord;
                OnPropertyChanged(nameof(MaskedWord));

                if (!found)
                {
                    WrongLetters.Add(guessedLetter);
                    OnPropertyChanged(nameof(WrongLetters));

                    Faults++;
                    if (Faults >= FaultsLimit)
                    {
                        DisplayGameOver();
                        return;
                    }
                }

                if (_currentGame.IsWordFound())
                {
                    await HandleSuccessfulAction();
                }
            }
        }

        private void OnReturn(object parameter)
        {
            _mainViewModel.CurrentViewModel = new DifficultySelectionViewModel(_mainViewModel);
            _timer.Stop();
        }

        private async Task HandleSuccessfulAction()
        {
            try
            {
                var token = SecureStorage.GetToken();
                var (points, comboMultiplier, nextWord) = await _gameService.ConfirmWordAsync(_currentGame.Word, token);

                Points = points;
                ComboMultiplier = comboMultiplier;
                TotalScore += points;
                
                UsedLetters.Clear();
                WrongLetters.Clear();

                StartGameWithNextWordAsync(nextWord);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la confirmation du mot : {ex.Message}");
            }
        }
        
        public bool IsVictoryOverlayVisible
        {
            get => _isVictoryOverlayVisible;
            set
            {
                _isVictoryOverlayVisible = value;
                OnPropertyChanged();
            }
        }


        private async Task OnShowHint()
        {
            if (IsVictoryOverlayVisible)
            {
                return; // Do nothing if the victory overlay is visible
            }

            if (_currentHintWordId.HasValue)
            {
                // If hint already exists, just show the overlay
                DisplayHintOverlay(_currentHintWordId.Value);
                return;
            }

            try
            {
                // Fetch the hint
                var token = SecureStorage.GetToken();
                var (wordId, newComboMultiplier) = await _gameService.GetHintAsync(token);

                // Update the combo multiplier and round to one decimal place
                ComboMultiplier = Math.Max(newComboMultiplier, 0.1);

                // Save hint details
                _currentHintWordId = wordId;

                // Display the hint overlay
                DisplayHintOverlay(wordId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching hint: {ex.Message}");
            }
        }

        private void DisplayHintOverlay(int wordId)
        {
            HintOverlayViewModel.HintImageSource = $"https://4images1mot-solution.com/images_big/{wordId}.jpg";
            IsHintOverlayVisible = true;
        }

        public void CloseHintOverlay(object parameter)
        {
            if (_closeHintTaskCompletionSource != null && !_closeHintTaskCompletionSource.Task.IsCompleted)
            {
                _closeHintTaskCompletionSource.SetResult(true);
            }

            IsHintOverlayVisible = false;
        }

        public VictoryControlViewModel VictoryControlViewModel { get; }
        public HintOverlayViewModel HintOverlayViewModel { get; }

        public async Task StartGameWithNextWordAsync(string nextWord)
        {
            _timer.Stop();
            // Display the Victory Overlay
            IsVictoryOverlayVisible = true;

            // Initialize the TaskCompletionSource and await it
            _continueTaskCompletionSource = new TaskCompletionSource<bool>();
            await _continueTaskCompletionSource.Task;

            // Hide the Victory Overlay
            IsVictoryOverlayVisible = false;

            // Clear the current hint
            _currentHintWordId = null;

            // Resume the game setup
            _currentGame = new HangmanGame(nextWord);
            MaskedWord = _currentGame.MaskedWord;
            OnPropertyChanged(nameof(MaskedWord));

            UsedLetters.Clear();
            WrongLetters.Clear();
            OnPropertyChanged(nameof(UsedLetters));
            OnPropertyChanged(nameof(WrongLetters));

            InitializeTimer(Difficulty); // Réinitialiser le timer en fonction de la difficulté
            _timer.Start();
        }



        private void OnContinue(object parameter)
        {
            if (_continueTaskCompletionSource != null && !_continueTaskCompletionSource.Task.IsCompleted)
            {
                // Set result to unblock the method awaiting it
                _continueTaskCompletionSource.SetResult(true);
            }
        }

        private void DisplayGameOver()
        {
            // Arrêter le timer lorsqu'il y a un game over
            _timer?.Stop();
            _mainViewModel.CurrentViewModel = new GameOverViewModel(_mainViewModel);
        }
    }
}