using System.Drawing;

namespace CellConquest.Domain.ValueObjects;

public record GameConfig(GameId GameId, PlayerName Owner, PointF[] Outline);