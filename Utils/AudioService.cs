using System;
using LibVLCSharp.Shared;

namespace ConnectDotsGame.Utils
{
    public class AudioService : IDisposable
    {
        private static AudioService? _instance;
        public static AudioService Instance => _instance ??= new AudioService();

        private LibVLC _libVLC;
        private MediaPlayer? _player;
        private string? _currentFilePath;

        private AudioService()
        {
            Core.Initialize();
            _libVLC = new LibVLC();
        }

        public void PlayBackgroundMusic(string filePath)
        {
            if (_player != null && _currentFilePath == filePath && _player.IsPlaying)
                return;

            StopBackgroundMusic();

            var media = new Media(_libVLC, filePath, FromType.FromPath);
            _player = new MediaPlayer(media);

            _player.EndReached += (sender, e) =>
            {
                _player?.Stop();
                _player?.Play();
            };

            _player.Play();
            _currentFilePath = filePath;
        }

        public void StopBackgroundMusic()
        {
            if (_player != null)
            {
                _player.Stop();
                _player.Dispose();
                _player = null;
                _currentFilePath = null;
            }
        }

        public void Dispose()
        {
            StopBackgroundMusic();
            _libVLC.Dispose();
        }
    }
}