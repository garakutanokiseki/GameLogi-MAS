using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GameLogi_MAS
{
    class Simulation
    {
        List<Agent> agents;
        List<int> initial_cooperators;

        public Simulation(int population, int average_degree) {
            agents = generate_agents(population, average_degree);
            initial_cooperators = choose_initial_cooperators();
        }

        public List<Agent> generate_agents(int population, int average_degree) {
            int rearange_edges = average_degree / 2;

            List<Agent> agents = new List<Agent>();

            //ネットワークを生成する
            var network = BarabasiAlbertModelGenerator.Generator(population);

            //agentを作成する
            for (int i = 0; i < population; ++i) {
                Agent agent = new Agent();
                agent.neighbors_id = network[i];
                agents.Add(agent);
            }

            return agents;
        }

        //最初のゲームでC戦略を取るエージェントをランダムに選ぶ
        public List<int> choose_initial_cooperators() {
            List<int> initial_cooperators = new List<int>();
            Random random = new Random();

            int create_count = agents.Count / 2;
            int population = agents.Count - 1;

            while (initial_cooperators.Count < create_count)
            {
                int id = random.Next(0, population);
                if (!initial_cooperators.Contains(id))
                {
                    initial_cooperators.Add(id);
                }
            }

            return initial_cooperators;
        }

        //全エージェントの戦略を初期化
        public void initialize_strategy() {
            for (int i = 0; i < agents.Count; ++i) {
                if (initial_cooperators.Contains(i))
                    agents[i].strategy = Agent.Strategy.C;
                else
                    agents[i].strategy = Agent.Strategy.D;
            }
        }

        //利得表に基づいて全エージェントが獲得する利得を計算
        //@param in Dg:社会におけるジレンマ強さ(チキン型ジレンマの強さ)
        //@param in Dr:社会におけるジレンマ強さ(スタグハント型ジレンマの強さ)
        public void count_payoff(double Dg, double Dr) {
            const double R = 1; // Reward
            double S = -Dr;     // Sucker
            double T = 1 + Dg;  // Temptation
            double P = 0;       // Punishment

            for (int i = 0; i < agents.Count; ++i) {
                Agent focal = agents[i];
                focal.point = 0.0;
                foreach (int nb_id in focal.neighbors_id)
                {
                    Agent neighbor = agents[nb_id];
                    if (focal.strategy == Agent.Strategy.C && neighbor.strategy == Agent.Strategy.C)
                        focal.point += R;
                    else if (focal.strategy == Agent.Strategy.C && neighbor.strategy == Agent.Strategy.D)
                        focal.point += S;
                    else if (focal.strategy == Agent.Strategy.D && neighbor.strategy == Agent.Strategy.C)
                        focal.point += T;
                    else if (focal.strategy == Agent.Strategy.D && neighbor.strategy == Agent.Strategy.D)
                        focal.point += P;
                }
            }
        }

        //全エージェントに戦略を更新させる
        public void update_strategy() {
            for (int i = 0; i < agents.Count; ++i)
            {
                Agent focal = agents[i];
                focal.decide_next_strategy(agents);
            }

            for (int i = 0; i < agents.Count; ++i)
            {
                Agent focal = agents[i];
                focal.update_strategy();
            }
        }

        //C戦略エージェントの割合を計算
        public double count_fc() {
            double fc = (double)agents.Count(x => x.strategy == Agent.Strategy.C) / (double)agents.Count;
            return fc;
        }

        //一つのパラメータ設定で協調率が収束するまで計算
        public double play_game(int episode, double Dg, double Dr)
        {
            initialize_strategy();
            double initial_fc = count_fc();
            var fc_hist = new List<double>();
            fc_hist.Add(initial_fc);

            Console.WriteLine(String.Format("Episode:{0}, Dr:{1:0.000}, Dg:{2:0.000}, Time: 0, Fc:{3:0.000}", episode, Dr, Dg, initial_fc));

            const int tmax = 3000;
            double fc_converged = 0.0;
            double[] hist = new double[100];

            for (int i = 0; i < tmax; ++i) {
                count_payoff(Dg, Dr);
                update_strategy();
                double fc = count_fc();
                fc_hist.Add(fc);
                Console.WriteLine(String.Format("Episode:{0}, Dr:{1:0.000}, Dg:{2:0.000}, Time: 0, Fc:{3:0.000}", episode, Dr, Dg, fc));

                //収束判定
                if (i >= 100) {
                    fc_hist.CopyTo(0, hist, fc_hist.Count - 100, 100);
                }
                if ((i >= 100 && Math.Abs(hist.Average() - fc) / fc < 0.001) || i == tmax - 1) {
                    //100回以上戦略更新を繰り返し、過去100回のゲームで得られた協調率の平均値と次のゲームでの協調率の差が十分小さくなったら計算を打ち切る
                    fc_converged = hist.Average();//過去100回分のゲームで得られた協調率の平均値を取る
                    break;
                }
                else if (fc >= 0 && fc <= 1.0) {
                    //囚人のジレンマゲームでは全員C戦略 or 全員D戦略の状態に収束しやすいため、そうなったらすぐに計算を打ち切る
                    fc_converged = fc;
                    break;
                }
            }

            Console.WriteLine(String.Format("Episode:{0}, Dr:{1:0.000}, Dg:{2:0.000}, Time: 0, Fc:{3:0.000}", episode, Dr, Dg, fc_converged));
            return fc_converged;

        }

        public class Result {
            public Result(double Dr, double Dg, double fc)
            {
                this.Dr = Dr;
                this.Dg = Dg;
                this.fc = fc;

            }
            public double Dg;
            public double Dr;
            public double fc;
        }
        //全パラメータ領域でplay_gameを実行し、計算結果をCSVに書き出す
        public void run_one_episode(int episode) {
            List<Result> listResults = new List<Result>();

            choose_initial_cooperators();
            for (double Dr = 0.0; Dr < 1.1; Dr += 0.1)
            {
                for (double Dg = 0.0; Dg < 1.1; Dg += 0.1)
                {
                    double fc_converged = play_game(episode, Dg, Dr);
                    Result new_result = new Result(Dr, Dg, fc_converged);

                    listResults.Add(new_result);
                }
            }

            //CSVに出力する
            foreach (Result result in listResults)
            {
                Console.WriteLine(String.Format("{0:0.000}, {1:0.000}, {2:0.000}", result.Dg, result.Dr, result.fc));
            }
        }
    }
}
