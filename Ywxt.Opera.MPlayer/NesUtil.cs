using System.Diagnostics;

namespace Ywxt.Opera.MPlayer
{
    public static class NesUtil
    {
        public static bool NesCheck(int user1, int user2, int data1, int data2)
        {
            return user1 + user2 == 255 && data1 + data2 == 255;
        }

        public static ControllerKey GetKey(int user, int user2, int data1, int data2)
        {
            if (!NesCheck(user, user2, data1, data2))
            {
                return ControllerKey.None;
            }

            if (user != 255)
            {
                return ControllerKey.None;
            }

            return data1 switch
            {
                199 => ControllerKey.Ok,
                239 => ControllerKey.Left,
                165 => ControllerKey.Right,
                231 => ControllerKey.Up,
                181 => ControllerKey.Down,
                151 => ControllerKey.Star,
                79 => ControllerKey.Sharp,
                103 => ControllerKey.Zero,
                31 => ControllerKey.Seven,
                87 => ControllerKey.Eight,
                111 => ControllerKey.Nine,
                221 => ControllerKey.Four,
                253 => ControllerKey.Five,
                61 => ControllerKey.Six,
                93 => ControllerKey.One,
                157 => ControllerKey.Two,
                29 => ControllerKey.Three,
                _ => ControllerKey.None

            };

        }

        public static void Shutdown(Player player,NesParser parser)
        {
            player.Finish();
            player.Dispose();
            parser.Dispose();
            Process.Start("sudo","shutdown -h now");
        }
    }
}