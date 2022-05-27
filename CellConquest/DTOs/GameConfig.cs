using System.Drawing;

namespace CellConquest.DTOs;

public record GameConfig(string GameId, string Owner, PointF[] Outline);