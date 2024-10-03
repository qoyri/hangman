using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using HangmanApp.Services;
using HangmanApp.ViewModels;

public class RankingViewModel : ObservableObject
{
    private readonly MainViewModel _mainViewModel;
    private readonly ApiService _apiService;
    private ObservableCollection<LeaderboardEntry> _leaderboard;

    public ObservableCollection<LeaderboardEntry> Leaderboard
    {
        get => _leaderboard;
        set
        {
            _leaderboard = value;
            OnPropertyChanged();
        }
    }

    public ICommand ReturnCommand { get; }

    public RankingViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        _apiService = new ApiService();
        LoadLeaderboard();
        ReturnCommand = new RelayCommand(OnReturn);
    }

    private async void LoadLeaderboard()
    {
        try
        {
            var token = SecureStorage.GetToken();
            // Remplacez par la méthode pour obtenir le token authentifié
            var leaderboard = await _apiService.GetLeaderboardAsync(token);
            Leaderboard = new ObservableCollection<LeaderboardEntry>(leaderboard);
        }
        catch (HttpRequestException ex)
        {
            // Gérer les erreurs de requête API
            Console.WriteLine(ex.Message);
        }
    }

    private void OnReturn(object parameter)
    {
        _mainViewModel.NavigateCommand.Execute("Home"); // Navigue vers la vue Home
    }
}