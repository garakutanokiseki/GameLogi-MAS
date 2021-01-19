using System;
using System.Collections.Generic;
using System.Text;

namespace GameLogi_MAS
{
    class Agent
    {
        public enum Strategy { 
            None,
            C,
            D
        };

        public double point = 0.0;
        public Strategy strategy = Strategy.None;
        public Strategy next_strategy = Strategy.None;
        public List<int> neighbors_id = new List<int>();

        private Random random = new Random();

        public void decide_next_strategy(List<Agent> agents) {
            //Pairwise-Fermiモデルで次のゲームでの戦略を決定する

            //戦略決定時に参照する隣人エージェントをランダムに選ぶ
            int opponent_id = neighbors_id[random.Next(neighbors_id.Count - 1)];
            Agent opponent = agents[opponent_id];
            if (opponent.strategy != strategy && 
                (double)random.Next(100000) / 100000.0 < (1.0 / (1.0 + Math.Exp(point - opponent.point)) / 0.1)) {
                next_strategy = opponent.strategy;
            }
        }

        public void update_strategy() {
            strategy = next_strategy;
        }

    }
}
