using System;
using System.Threading;

namespace Ywxt.Opera.MPlayer
{
    class Program
    {
        private static string[] _args;

        static void Main(string[] args)
        {
            _args = args;
            using var nes = new NesParser();
            using var player = new Player();
            Console.WriteLine("=======================");
            Console.WriteLine("遥控器");
            nes.Start();
            Console.WriteLine("播放");
            player.Play();
            nes.Received += (success, user1, user2, data1, data2) =>
            {
                if (!success)
                {
                    return;
                }

                var key = NesUtil.GetKey(user1, user2, data1, data2);
                Control(player, nes, key);
            };

            Console.WriteLine("Press enter to exit...");
            Thread.Sleep(Timeout.Infinite);
        }

        private static void Control(Player player, NesParser nes, ControllerKey key)
        {
            try
            {
                switch (key)
                {
                    case ControllerKey.None:
                        break;
                    case ControllerKey.Zero:
                        break;
                    case ControllerKey.One:
                        break;
                    case ControllerKey.Two:
                        break;
                    case ControllerKey.Three:
                        break;
                    case ControllerKey.Four:
                        break;
                    case ControllerKey.Five:
                        break;
                    case ControllerKey.Six:
                        break;
                    case ControllerKey.Seven:
                        break;
                    case ControllerKey.Eight:
                        break;
                    case ControllerKey.Nine:
                        break;
                    case ControllerKey.Star:
                        break;
                    case ControllerKey.Sharp:
                        NesUtil.Shutdown(player, nes);
                        break;
                    case ControllerKey.Up:
                        player.PlayPrevious();
                        break;
                    case ControllerKey.Down:
                        player.PlayNext();
                        break;
                    case ControllerKey.Left:
                        player.SubVolume();
                        break;
                    case ControllerKey.Right:
                        player.IncreaseVolume();
                        break;
                    case ControllerKey.Ok:
                        player.Pause();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(key), key, null);
                }
            }
            catch (Exception)
            {
                player.Dispose();
                nes.Dispose();
                Main(_args);
            }
        }
    }
}