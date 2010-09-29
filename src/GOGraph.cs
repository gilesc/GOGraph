using System;
using System.IO;
using System.Collections.Generic;
using QuickGraph;
using QuickGraph.Algorithms;

namespace GOGraph
{
	public class GOGraph
	{
		public GOGraph ()
		{
		}
		
		private Dictionary<String, AdjacencyGraph<Int32, Edge<Int32>>> graphs = 
			new Dictionary<String, AdjacencyGraph<Int32, Edge<Int32>>>();
		private Dictionary<String,UndirectedGraph<Int32,UndirectedEdge<Int32>>> undirectedGraphs = 
			new Dictionary<String, UndirectedGraph<Int32, UndirectedEdge<Int32>>>();
		
		public void InitializeGraph(String goPath) {
			AdjacencyGraph<Int32, Edge<Int32>> graph = null;
			String line, goNamespace = null;
			Int32 currentTerm = 0;
			Int32 parentTerm = 0;
			
			using(StreamReader reader = new StreamReader(goPath)) {
				while (!reader.EndOfStream) {
					line = reader.ReadLine();
					if (line.StartsWith("[Typedef]")) {
						reader.ReadToEnd();
					} else if (line.StartsWith("id:")) {
						currentTerm  = Int32.Parse(line.Substring(7,7));
					} else if (line.StartsWith("namespace:")) {
						goNamespace = line.Substring(11);
					} else if (line.StartsWith("is_a:")) {
						if (!graphs.ContainsKey(goNamespace)) {
							graphs.Add(goNamespace, new AdjacencyGraph<Int32, Edge<Int32>>());
						}
						
						graph = graphs[goNamespace];
						graph.AddVertex(currentTerm);
						parentTerm = Int32.Parse(line.Substring(10,7));
						graph.AddVertex(parentTerm);
						graph.AddEdge(new Edge<Int32>(currentTerm,parentTerm));	
					}
				}
			}
			
			foreach(String ns in graphs.Keys) {
				var g = graphs[ns];
				var newGraph = new UndirectedGraph<Int32, UndirectedEdge<Int32>>();
				foreach (Edge<Int32> e in g.Edges) {
                    newGraph.AddVertex(e.Source);
                    newGraph.AddVertex(e.Target);
					newGraph.AddEdge(new UndirectedEdge<Int32>(e.Source, e.Target));
				}
				undirectedGraphs.Add(ns, newGraph);
			}
			
		}
		
		public Int32[] FindAllAncestors(Int32 node) {
            HashSet<Int32> result = FindAncestorSet(node);
            Int32[] outArr = new Int32[result.Count];
            result.CopyTo(outArr);
            return outArr;
		}

        public Boolean IsAncestor(Int32 parent, Int32 child)
        {
            return FindAncestorSet(child).Contains(parent);
        }

        private HashSet<Int32> FindAncestorSet(Int32 node)
        {
            AdjacencyGraph<Int32, Edge<Int32>> graph = null;
            foreach (AdjacencyGraph<Int32, Edge<Int32>> g in graphs.Values)
            {
                if (g.ContainsVertex(node))
                {
                    graph = g;
                }
            }
            if (graph == null)
            {
                return new HashSet<Int32>();
            }
            return FindAncestorSet(graph, node);
        }

		private HashSet<Int32> FindAncestorSet(AdjacencyGraph<Int32,Edge<Int32>> graph, Int32 node) {
            HashSet<Int32> result = new HashSet<Int32>();
            foreach (Edge<Int32> outEdge in graph.OutEdges(node))
            {
                result.Add(outEdge.Target);
                foreach (Int32 i in FindAncestorSet(graph, outEdge.Target))
                    result.Add(i);
            }
            return result;
		}
		
		Func<UndirectedEdge<Int32>, double> costFn = e => 1;
		public Int32[] ShortestPath(int startNode, int endNode) {
			//Choose appropriate graph (or return nothing if terms aren't in same namespace)
            UndirectedGraph<Int32, UndirectedEdge<Int32>> graph = null;
            foreach (UndirectedGraph<Int32, UndirectedEdge<Int32>> g in undirectedGraphs.Values)
            {
                if (g.ContainsVertex(startNode) && g.ContainsVertex(endNode))
                    graph = g;
            }
            if (graph == null)
			    return new Int32[0];

            //
            IEnumerable<UndirectedEdge<Int32>> path;
            var nodes = new List<Int32>();
            
            if (graph.ShortestPathsDijkstra<Int32,UndirectedEdge<Int32>>(costFn, startNode)(endNode, out path))
            {
                nodes.Add(startNode);
                foreach (UndirectedEdge<Int32> e in path)
                    nodes.Add(e.GetOtherVertex<Int32,UndirectedEdge<Int32>>(nodes[nodes.Count - 1]));
            }
            return nodes.ToArray();
        }

        public static void Main(String[] args)
        {
            var graph = new GOGraph();
            graph.InitializeGraph("C:/data/gene_ontology_ext.obo");
            foreach (Int32 i in graph.FindAllAncestors(8285))
                Console.WriteLine(i);
            
            Console.WriteLine("-----");
            Console.WriteLine(graph.IsAncestor(8150, 8285));
            Console.WriteLine(graph.IsAncestor(8151, 8285));
            Console.WriteLine("-----");
            foreach (Int32 i in graph.ShortestPath(8150, 8285))
                Console.WriteLine(i);

            Console.ReadKey();
        }
	}
}

