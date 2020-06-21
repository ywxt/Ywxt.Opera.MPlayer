namespace Ywxt.Opera.MPlayer
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;


    public sealed class NesParser : IDisposable
    {
        private CancellationTokenSource _cancellation = new CancellationTokenSource();

        public delegate void NesReceive(bool success, int user1, int user2, int data1, int data2);

        public event NesReceive Received;

        private void OnReceived(bool success, int user1, int user2, int data1, int data2)
        {
            Received?.Invoke(success, user1, user2, data1, data2);
        }

        private void Receive(CancellationToken cancellationToken)
        {
            using var lirc = new FileStream("/dev/lirc0", FileMode.Open);
            var count = 0;
            var data = new byte[4];
            var rt = new byte[4];
            var ok = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                lirc.ReadAsync(data, cancellationToken).GetAwaiter().GetResult();
                count++;

                if (data[3] == 3 && count < 70)
                {
                    ok = false;
                }

                // 第1，2个电平和68以后电平忽略
                if ((data[3] != 3 && count > 68) || count < 3)
                {
                    continue;
                }

                if (count == 3 && (data[1] < 30 || data[1] > 40 || data[2] != 0 || data[3] != 1))
                {
                    ok = false;
                    continue;
                }

                if (count == 4 && (data[1] < 12 || data[1] > 23 || data[2] != 0 || data[3] != 0))
                {
                    ok = false;
                    continue;
                }

                if (count == 3 || count == 4)
                {
                    continue;
                }


                // 第一个高电平
                if (data[1] > 0 && data[1] < 5 && data[2] == 0 && data[3] == 1)
                {
                    continue;
                }

                // 表示1
                if (data[1] > 0 && data[1] < 5 && data[2] == 0 && data[3] == 0)
                {
                    rt[(count - 5) / 16] |= (byte) (1 << (7 - ((count - 5) / 2) % 8));
                    continue;
                }

                // 表示0
                if (data[1] > 5 && data[1] < 10 && data[2] == 0 && data[3] == 0)
                {
                    continue;
                }


                if (data[3] != 3) continue;
                OnReceived(ok, rt[0], rt[1], rt[2], rt[3]);
                ok = true;
                count = 0;
                rt[0] = 0;
                rt[1] = 0;
                rt[2] = 0;
                rt[3] = 0;
            }
        }

        public void Start()
        {
            Task.Run(() => { Receive(_cancellation.Token); }, _cancellation.Token);
        }

        public void Stop()
        {
            _cancellation.Cancel();
        }

        public void Dispose()
        {
            _cancellation?.Dispose();
        }
    }
}