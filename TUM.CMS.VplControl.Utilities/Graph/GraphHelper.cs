using System.Collections.Generic;
using System.Drawing;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;

namespace TUM.CMS.VplControl.Utilities.Graph
{
    public class GraphHelper
    {
        public GraphHelper()
        {

            AdjacencyGraph<Point, Edge<Point>> graph = new AdjacencyGraph<Point, Edge<Point>>(true);

            // Add some vertices to the graph
            var point1 = new Point(0, 0);
            var point2 = new Point(0, 0);
            var point3 = new Point(0, 0);
            var point4 = new Point(0, 0);

            graph.AddVertex(point1);
            graph.AddVertex(point2);
            graph.AddVertex(point3);
            graph.AddVertex(point4);

            // Create the edges
            Edge<Point> a_b = new Edge<Point>(point1, point2);
            Edge<Point> b_c = new Edge<Point>(point2, point3);
            Edge<Point> c_d = new Edge<Point>(point3, point4);
            Edge<Point> d_a = new Edge<Point>(point4, point1);

            // Add the edges
            graph.AddEdge(a_b);
            graph.AddEdge(b_c);
            graph.AddEdge(c_d);
            graph.AddEdge(d_a);

            // Define some weights to the edges
            var edgeCost = new Dictionary<Edge<Point>, double>(graph.EdgeCount);
            edgeCost.Add(a_b, 4);
           
            // // We want to use Dijkstra on this graph
            // var dijkstra = new DijkstraShortestPathAlgorithm<Point, Edge<Point>>(graph, edgeCost);
            // 
            // // attach a distance observer to give us the shortest path distances
            // // VertexDistanceRecorderObserver<string, Edge<string>> distObserver = new VertexDistanceRecorderObserver<string, Edge<string>>();
            // // distObserver.Attach(dijkstra);
            // 
            // // Attach a Vertex Predecessor Recorder Observer to give us the paths
            // VertexPredecessorRecorderObserver<string, Edge<string>> predecessorObserver = new VertexPredecessorRecorderObserver<string, Edge<string>>();
            // predecessorObserver.Attach(dijkstra);
            // 
            // // Run the algorithm with A set to be the source
            // dijkstra.Compute("A");
            // 
            // // foreach (KeyValuePair<string, int> kvp in distObserver.Distances)
            // //     Console.WriteLine("Distance from root to node {0} is {1}", kvp.Key, kvp.Value);
            // 
            // foreach (KeyValuePair<string, Edge<string>> kvp in predecessorObserver.VertexPredecessors)
            //     Console.WriteLine("If you want to get to {0} you have to enter through the in edge {1}", kvp.Key, kvp.Value);
            // 
            // // Remember to detach the observers
            // // distObserver.Detach(dijkstra);
            // // predecessorObserver.Detach(dijkstra);
        }
    }
}
