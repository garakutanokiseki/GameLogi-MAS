using System;

namespace GameLogi_MAS
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            //Console.WriteLine("Hello World!");
            var listEdges = BarabasiAlbertModelGenerator.Generator(100);

            for (int i = 0; i < listEdges.Count; ++i) {
                Console.Write(String.Format("{0} : ", i));
                foreach (var index in listEdges[i]) {
                    Console.Write(String.Format("{0}, ", index));
                }
                Console.Write("\n");
            }
            */
            int population = 10000;//          # エージェント数
            int average_degree = 8;//          # 社会ネットワークの平均次数
            int num_episode = 1;//             # シミュレーションの試行回数
            var simulation = new Simulation(population, average_degree);

            for (int i = 0; i < num_episode; ++i) {
                simulation.run_one_episode(i);
            }
        }
    }
}
