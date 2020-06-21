using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibMPlayerCommon;

namespace Ywxt.Opera.MPlayer
{
    public sealed class Player : IDisposable
    {
        private Config _config;
        private List<string> _files;
        private LibMPlayerCommon.MPlayer _mPlayer;
        private int _current;
        private bool _canPlay;
        private BlockingCollection<Config> _queue = new BlockingCollection<Config>(500);

        public Player()
        {
            var handle = (int) System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            _mPlayer = new LibMPlayerCommon.MPlayer(handle, MplayerBackends.SDL,
                @"mplayer");
            _mPlayer.CurrentPosition += OnCurrentPosition;
            // _mPlayer.VideoExited += OnVideoExited;
            _mPlayer.MplayerOutput += OnVideoExited;


            RefreshFiles();
            SaveProgress();
        }

        private void OnVideoExited(object sender, DataReceivedEventArgs e)
        {
            if (!e.Data.Equals("EOF code: 1  ", StringComparison.Ordinal)) return;
            Console.WriteLine("======================");
            Console.WriteLine("播放结束");
            PlayNext();
        }

        // private void OnOutput(object sender, DataReceivedEventArgs e)
        // {
        //     if (e.Data.Contains("exit", StringComparison.InvariantCultureIgnoreCase) ||
        //         e.Data.Contains("eof", StringComparison.InvariantCultureIgnoreCase))
        //     {
        //         Console.WriteLine(e.Data);
        //     }
        // }

        private void SaveProgress()
        {
            var task = new Task(() =>
            {
                while (true)
                {
                    var config = _queue.Take();
                    config.Save();
                }
            }, TaskCreationOptions.LongRunning);
            task.Start();
        }

        private void OnCurrentPosition(object sender, MplayerEvent e)
        {
            _config.CurrentPosition = (int) e.Value;
            _queue.Add(_config);
        }

        public void RefreshFiles()
        {
            _config?.Save();
            _files = GetVideoFiles("/media/pi");
            _canPlay = _files.Count != 0;
            _config = Config.Load();
            if (!_canPlay)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_config.CurrentFile))
            {
                _current = 0;
            }
            else
            {
                var index = _files.IndexOf(_config.CurrentFile);
                _current = index < 0 ? 0 : index;
            }

            _config.CurrentFile = _files[_current];
            foreach (var file in _files)
            {
                Console.WriteLine($"文件:{file}");
            }
        }

        private List<string> GetVideoFiles(string path)
        {
            var dirs = Directory.GetDirectories(path);
            var extension = new[] {".mp4", ".flv"};
            return dirs.SelectMany(Directory.GetFiles)
                .Where(file => extension.Contains(Path.GetExtension(file)))
                .ToList();
        }

        public void Play(string file)
        {
            if (!_canPlay)
            {
                return;
            }

            Console.WriteLine("=======================");
            Console.WriteLine($"播放{file}");
            _mPlayer.Play(file);
            Console.WriteLine("=======================");
            Console.WriteLine($"音量{_config.Volume}");
            _mPlayer.Volume(_config.Volume);
            Console.WriteLine("=======================");
            Console.WriteLine($"位置{_config.CurrentPosition}");
            _mPlayer.MovePosition(_config.CurrentPosition);
            Console.WriteLine("=======================");
            Console.WriteLine("全屏");
            _mPlayer.FullScreen = true;
        }

        public void Play()
        {
            var file = _config.CurrentFile;
            Play(file);
        }

        public void Pause()
        {
            if (!_canPlay || _mPlayer.CurrentStatus == MediaStatus.Stopped)
            {
                return;
            }

            _mPlayer.Pause();
        }

        public void IncreaseVolume()
        {
            if (!_canPlay || _mPlayer.CurrentStatus == MediaStatus.Stopped)
            {
                return;
            }

            _config.Volume += 5;
            if (_config.Volume >= 100)
            {
                _config.Volume = 100;
            }

            _mPlayer.Volume(_config.Volume);
        }

        public void SubVolume()
        {
            if (!_canPlay || _mPlayer.CurrentStatus == MediaStatus.Stopped)
            {
                return;
            }

            _config.Volume -= 5;
            if (_config.Volume <= 0)
            {
                _config.Volume = 0;
            }

            _mPlayer.Volume(_config.Volume);
        }

        public void PlayNext()
        {
            if (!_canPlay)
            {
                return;
            }

            if (_current == _files.Count - 1)
            {
                _current = 0;
            }
            else
            {
                _current++;
            }

            _config.CurrentFile = _files[_current];
            _config.CurrentPosition = 0;
            Play();
        }

        public void PlayPrevious()
        {
            if (!_canPlay)
            {
                return;
            }

            if (_current == 0)
            {
                _current = _files.Count - 1;
            }
            else
            {
                _current--;
            }

            _config.CurrentFile = _files[_current];
            _config.CurrentPosition = 0;
            Play();
        }

        public void Finish()
        {
            _mPlayer.Quit();
        }

        public void Dispose()
        {
            _mPlayer?.Dispose();
            _queue?.Dispose();
        }
    }
}