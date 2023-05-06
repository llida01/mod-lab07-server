using System;
using System.Threading;
using System.IO;

namespace Lab07
{
    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }
    class Client
    {
        public event EventHandler<procEventArgs> request;
        private Server server;
        public Client(Server server)
        {
            this.server = server;
            this.request += server.Proc;
        }
        protected virtual void OnProc(procEventArgs e)
        {
            EventHandler<procEventArgs> handler = request;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public void Send(int id)
        {
            procEventArgs args = new procEventArgs();
            args.id = id;
            OnProc(args);
        }
    }
    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }
    class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public int sleepTime = 0;

        public Server(int poolCount, int sleepTime)
        {
            pool = new PoolRecord[poolCount];
            this.sleepTime = sleepTime;
        }
        public void Proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < pool.Length; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(i);
                        Console.WriteLine(e.id + " принята");
                        processedCount++;
                        return;
                    }
                }
                Console.WriteLine(e.id + " отклонена");
                rejectedCount++;
            }
        }
        public void Answer(object arg)
        {
            int id = (int)arg;
            Thread.Sleep(sleepTime);
            pool[id].in_use = false;
        }
    }
    class Program
    {
        static int Factorial(int n)
        {
            int answer = 1;
            for (int i = 1; i <= n; i++)
            {
                answer *= i;
            }
            return answer;
        }
        static void Main()
        {
            int poolCount = 5;
            int requests = 15;
            int lambda = 10;
            int mu = 2;
            int sleepTime = 5;
            Server server = new Server(poolCount, sleepTime);
            Client client = new Client(server);

            for (int id = 1; id <= requests; id++)
            {
                client.Send(id);
            }
            StreamWriter writer = new StreamWriter("results.txt");
            Console.WriteLine("\nВсего заявок: {0}", server.requestCount);
            writer.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            writer.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            writer.WriteLine("Отклонено заявок: {0}", server.rejectedCount);

            Console.WriteLine("\nТеоретические значения");
            writer.WriteLine("\nТеоретические значения");

            double r = lambda / mu;
            double Po = 0;
            for (int i = 0; i <= poolCount; i++)
            {
                Po += (Math.Pow(r, i) / Factorial(i));
            }
            Po = 1 / Po;
            Console.WriteLine("\nВероятность простоя системы Po: {0}", Po);
            writer.WriteLine("\nВероятность простоя системы Po: {0}", Po);
            double Pn = Math.Pow(r, poolCount) / Factorial(poolCount) * Po;
            Console.WriteLine("Вероятность отказа системы Pn: {0}", Pn);
            writer.WriteLine("Вероятность отказа системы Pn: {0}", Pn);
            double Q = 1 - Pn;
            Console.WriteLine("Относительная пропускная способность Q: {0}", Q);
            writer.WriteLine("Относительная пропускная способность Q: {0}", Q);
            double A = lambda * Q;
            Console.WriteLine("Абсолютная пропускная способность A: {0}", A);
            writer.WriteLine("Абсолютная пропускная способность A: {0}", A);
            double k = A / mu;
            Console.WriteLine("Среднее число занятых каналов k: {0}", k);
            writer.WriteLine("Среднее число занятых каналов k: {0}", k);

            Console.WriteLine("\nФактические значения");
            writer.WriteLine("\nФактические значения");

            lambda = server.requestCount / 1;
            mu = server.processedCount / poolCount;

            r = lambda / mu;
            Po = 0;
            for (int i = 0; i <= poolCount; i++)
            {
                Po += (Math.Pow(r, i) / Factorial(i));
            }
            Po = 1 / Po;
            Console.WriteLine("\nВероятность простоя системы Po: {0}", Po);
            writer.WriteLine("\nВероятность простоя системы Po: {0}", Po);
            Pn = Math.Pow(r, poolCount) / Factorial(poolCount) * Po;
            Console.WriteLine("Вероятность отказа системы Pn: {0}", Pn);
            writer.WriteLine("Вероятность отказа системы Pn: {0}", Pn);
            Q = 1 - Pn;
            Console.WriteLine("Относительная пропускная способность Q: {0}", Q);
            writer.WriteLine("Относительная пропускная способность Q: {0}", Q);
            A = lambda * Q;
            Console.WriteLine("Абсолютная пропускная способность A: {0}", A);
            writer.WriteLine("Абсолютная пропускная способность A: {0}", A);
            k = A / mu;
            Console.WriteLine("Среднее число занятых каналов k: {0}", k);
            writer.WriteLine("Среднее число занятых каналов k: {0}", k);
            writer.Close();
        }
    }
}
