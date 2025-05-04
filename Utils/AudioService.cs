using System.Media;

namespace ConnectDotsGame.Utils
{
    public class AudioService
    {
        private SoundPlayer? _player;

        public void PlayBackgroundMusic(string filePath)
        {
            if (_player != null)
            {
                StopBackgroundMusic();
            }

            _player = new SoundPlayer(filePath);
            _player.PlayLooping();
        }

        public void StopBackgroundMusic()
        {
            _player?.Stop();
            _player = null;
        }
    }
}