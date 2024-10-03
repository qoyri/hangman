using System;
using HangmanApp.Services;

namespace HangmanApp.Managers
{
    public class MusicManager
    {
        private static readonly Lazy<MusicManager> _instance = new Lazy<MusicManager>(() => new MusicManager());

        private MusicManager()
        {
        }

        public static MusicManager Instance => _instance.Value;

        public void PlayMusic(string resourceName)
        {
            MusicService.Instance.PlayMusic(resourceName);
        }

        public void StopMusic()
        {
            MusicService.Instance.StopMusic();
        }
    }
}