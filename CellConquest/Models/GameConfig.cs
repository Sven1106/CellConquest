using System.Collections.Generic;
using System.Drawing;

namespace CellConquest.Models;

public class GameConfig
{
	public System.Drawing.PointF[] Outline { get; }

	public GameConfig(System.Drawing.PointF[] outline)
	{
		Outline = outline;
	}
}