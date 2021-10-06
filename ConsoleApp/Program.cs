using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CellConquest;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using System.Windows.Forms;
using EpForceDirectedGraph.cs;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Newtonsoft.Json;
using Color = System.Drawing.Color;

List<PointF> polygon = new()
{
	new PointF(10, 10),
	new PointF(12, 10),
	new PointF(12, 12),
	new PointF(10, 12),
	// new PointF(10, 10),
	// new PointF(15, 13),
	// new PointF(30, 10),
	// new PointF(18, 20),
	// new PointF(15, 25),
	// new PointF(1, 30)
};
BoardGenerator boardGenerator = new();
Board board = boardGenerator.GenerateBoard(polygon);
// Board board = new(polygon);

var bla = Newtonsoft.Json.JsonConvert.SerializeObject(
	board.Cells,
	new JsonSerializerSettings
	{
		PreserveReferencesHandling = PreserveReferencesHandling.Objects
	}
);

const int width = 1000;
const int height = 1000;
const int scale = 35;


// Create a drawing target
Bitmap bitmap = new(width, height);

Graphics graphics = Graphics.FromImage(bitmap);
graphics.DrawRectangle(Pens.Black, 0, 0, 1 * scale, 1 * scale);
graphics.DrawPolygon(Pens.Black, board.Polygon.Select(point => new PointF(point.X * scale, point.Y * scale)).ToArray());
var newGraph = new BidirectionalGraph<string, Edge<string>>();
Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
EpForceDirectedGraph.cs.Graph m_fdgGraph = new();
foreach (var cell in board.Cells)
{
	string cellName = $"{cell.Id.ToString()}";
	// var cellNode = m_fdgGraph.GetNode(cellName);

	if (graph.FindNode(cellName) == null)
	{
		// cellNode = new EpForceDirectedGraph.cs.Node(cellName);
		// m_fdgGraph.AddNode(cellNode);
		graph.AddNode(cellName);
	}
	else
	{
	}

	foreach (var membrane in cell.Membranes)
	{
		var color = membrane.TouchedBy == StaticGameValues.Board ? new Pen(Color.Black, 3) :
			membrane.TouchedBy == StaticGameValues.NoOne ? new Pen(Color.Aqua, 3) : new Pen(Color.Coral, 3);
		graphics.DrawLine(
			color,
			new PointF(membrane.P1.X * scale, membrane.P1.Y * scale),
			new PointF(membrane.P2.X * scale, membrane.P2.Y * scale)
		);
		string membraneName = $"{membrane.Id.ToString()}";
		// var membraneNode = m_fdgGraph.GetNode(membraneName);
		if (graph.FindNode(membraneName) == null)
		{
			// membraneNode = new EpForceDirectedGraph.cs.Node(membraneName);
			// m_fdgGraph.AddNode(membraneNode);
			graph.AddNode(membraneName);
		}
		else
		{
		}

		// m_fdgGraph.AddEdge(new EpForceDirectedGraph.cs.Edge("", cellNode, membraneNode, null));
		graph.AddEdge(cellName, membraneName);
	}
}

float stiffness = 81.76f;
float repulsion = 40000.0f;
float damping = 0.5f;
ForceDirected2D m_fdgPhysics = new(m_fdgGraph, stiffness, repulsion, damping);

Microsoft.Msagl.GraphViewerGdi.GraphRenderer renderer = new(graph);
renderer.CalculateLayout();
int width1 = 5000;
Bitmap image = new((int)graph.Width, (int)(graph.Height), PixelFormat.Format32bppPArgb);
renderer.Render(image);
image.Save(@"C:\Users\svend\source\repos\CellConquest\test.png");


var graph2 = new UndirectedBidirectionalGraph<string, Edge<string>>(newGraph);

Action<GraphvizAlgorithm<string, Edge<string>>> InitAlgorithm()
{
	return algorithm =>
	{
		// Custom init example
		algorithm.CommonVertexFormat.Shape = GraphvizVertexShape.Circle;
		algorithm.CommonEdgeFormat.ToolTip = "Edge tooltip";
		algorithm.FormatVertex += (sender, args) =>
		{
			args.VertexFormat.Label = $"{args.Vertex}";
			if (args.Vertex.Contains("membrane"))
			{
				args.VertexFormat.Shape = GraphvizVertexShape.Rectangle;
			}
		};
	};
}

// Save
bitmap.Save(@"C:\Users\svend\source\repos\CellConquest\img.png");
Console.WriteLine("bordlayout");