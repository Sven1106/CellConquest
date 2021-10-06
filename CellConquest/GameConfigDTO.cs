using System.Collections.Generic;
using System.Drawing;

namespace CellConquest
{
	public class GameConfigDTO
	{
		public List<PointF> Polygon { get; }

		public GameConfigDTO(List<PointF> polygon)
		{
			Polygon = polygon;
		}
	}

	public class Player
	{
		public string Name { get; }
		public string Color { get; }

		public Player(string name, string color)
		{
			Name = name;
			Color = color;
		}
	}
}