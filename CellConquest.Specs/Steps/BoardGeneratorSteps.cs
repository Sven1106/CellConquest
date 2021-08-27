using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;
using Xunit;
using static System.Data.DataSet;

namespace CellConquest.Tests
{
	[Binding]
	public class BoardGeneratorSteps
	{
		private BoardLayout boardLayout;
		private Size actualSize;
		private Point[] Polygon;

		[StepArgumentTransformation]
		public Point[] TransformToPoints(string stepArgument)
		{
			string[] coordinates = stepArgument.Trim().Split(" - ");
			var points = coordinates.Select(coordinate => coordinate.Split(",").Select(int.Parse).ToArray())
			                        .Select(x => new Point(x[0], x[1]))
			                        .ToArray();
			return points;
		}

		// [StepArgumentTransformation(@"a board with '(.*)' is created")]
		// public Size TransformToSize(string stepArgument)
		// {
		// 	int[] coordinates = stepArgument.Trim().Split(",").Select(int.Parse).ToArray();
		// 	return new Size(coordinates[0], coordinates[1]);
		// }


		[Given(@"a '(.*)' is provided")]
		public void GivenAIsProvided(Point[] polygon)
		{
			Polygon = polygon;
		}

		[When(@"the generation is started")]
		public void WhenTheGenerationIsStarted()
		{
			boardLayout = new BoardLayout(Polygon);
			var boundingBox = BoardLayout.CreateBoundingBox(Polygon);
			actualSize = BoardLayout.GetSize(boundingBox);
		}


		[Then(@"a board with '(.*)' is created")]
		public void ThenABoardWithIsCreated(string stepArgument)
		{
			int[] coordinates = stepArgument.Trim().Split(",").Select(int.Parse).ToArray();
			var expectedSize = new Size(coordinates[0], coordinates[1]);
			Assert.Equal(expectedSize.Width, actualSize.Width);
			Assert.Equal(expectedSize.Height, actualSize.Height);
		}
	}
}