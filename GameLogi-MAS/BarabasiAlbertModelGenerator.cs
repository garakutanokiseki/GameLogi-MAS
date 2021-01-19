using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameLogi_MAS
{
    class BarabasiAlbertModelGenerator
    {

        const int InitverticesNumber = 3;

        //@param in max_vertices : 作成するネットワークのノード数
        //@return ノードのlist / リストの中身は接続先のノードのインデックス
        public static List<List<int>> Generator(int max_vertices)
        {
            if (max_vertices < InitverticesNumber) return null;
            IDictionary<int, String> vertices = new Dictionary<int, string>();
            IDictionary<int, IList<int>> edges = new Dictionary<int, IList<int>>();

            int MaxverticesNumber = max_vertices;

            GenerateInitGraphics(vertices, edges);

            for (int iCount = InitverticesNumber; iCount < MaxverticesNumber; iCount++)
            {
                vertices.Add(iCount, GenerateRandomString());

                int targetVertice = FindTheNextToConnect(edges);

                edges.Add(iCount, new List<int>() { targetVertice });
                if (edges[targetVertice] == null)
                {
                    edges[targetVertice] = new List<int>();
                }
                edges[targetVertice].Add(iCount);
            }

            //エッジリスト作る
            List<List<int>> listEdges = new List<List<int>>();
            for (int i = 0; i < MaxverticesNumber; ++i) {
                listEdges.Add(new List<int>());
            }

            //エッジリストにエッジを追加する
            foreach (var edge in edges)
            {
                if (edge.Value == null || edge.Value.Count == 0 || edge.Value.Count(x => x > edge.Key) == 0)
                    continue;

                List<int> values = listEdges[edge.Key];
                foreach (var target in edge.Value)
                {
                    if (target > edge.Key)
                    {
                        values.Add(target);
                    }
                }
            }

            //接続先を双方向に変換する
            for (int i = 0; i < MaxverticesNumber; ++i)
            {
                List<int> edge1 = listEdges[i];
                for (int j = 0; j < MaxverticesNumber; ++j) {
                    if (i == j) continue;
                    //他のエッジにedge1が参照されていないかを確認する
                    List<int> edge2 = listEdges[j];
                    if (edge2.Contains(i) && !edge1.Contains(j))
                    {
                        edge1.Add(j);
                    }
                }
            }

            return listEdges;
        }

        private static int FindTheNextToConnect(IDictionary<int, IList<int>> edges)
        {
            int next = randomSeed.Next(0, GetSumDegrees(edges) - 1);
            int lCount = 0;

            for (; lCount < edges.Count && next >= 0;)
            {
                next -= GetDegreesOfVertex(edges, lCount++);
            }

            return lCount >= 1 ? lCount - 1 : 0;
        }

        private static int GetSumDegrees(IDictionary<int, IList<int>> edges)
        {
            int sum = 0;
            foreach (var edge in edges)
            {
                if (edge.Value != null)
                    sum += edge.Value.Count;
            }
            return sum;
        }

        private static int GetDegreesOfVertex(IDictionary<int, IList<int>> edges, int vertex)
        {
            return edges[vertex] == null ? 0 : edges[vertex].Count;
        }

        #region Init Graphics

        private static void GenerateInitGraphics(IDictionary<int, String> vertices, IDictionary<int, IList<int>> edges)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (vertices.Count > 0)
                throw new ArgumentOutOfRangeException(nameof(vertices));

            for (int iCount = 0; iCount < InitverticesNumber; iCount++)
            {
                vertices.Add(iCount, GenerateRandomString());
            }
            for (int lCount = 0; lCount < InitverticesNumber; lCount++)
            {
                IList<int> fullEdges = new List<int>();
                for (int iCount = 0; iCount < InitverticesNumber; iCount++)
                {
                    if (iCount != lCount)
                        fullEdges.Add(iCount);
                }
                edges.Add(lCount, fullEdges);
            }

        }

        #endregion

        #region Generate Random String

        private static String GenerateRandomString()
        {
            StringBuilder randomSb = new StringBuilder();
            int length = randomSeed.Next(MinNameLength, MaxNameLength);
            for (int lCount = 0; lCount < length; lCount++)
            {
                int position = randomSeed.Next(0, charSets.Count - 1);
                randomSb.Append(charSets[position]);
            }
            return randomSb.ToString();
        }

        private static Random randomSeed = new Random(DateTime.Now.Millisecond);
        private static readonly List<Char> charSets = new List<char>() { ' ', 'a', 'b', 'c', 'q', 'r', 's', 't', 'u', 'v', 'w' };
        private static readonly int MaxNameLength = 8;
        private static readonly int MinNameLength = 6;

        #endregion

    }
}

