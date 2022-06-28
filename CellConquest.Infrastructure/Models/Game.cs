using Google.Cloud.Firestore;

namespace CellConquest.Infrastructure.Models;

[FirestoreData]
public class Game
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public Domain.Models.GameState GameState { get; set; }

    [FirestoreProperty]
    public Domain.Models.Board Board { get; set; }

    [FirestoreProperty]
    public string Owner { get; set; }

    [FirestoreProperty]
    public string CurrentPlayerTurn { get; set; }

    [FirestoreProperty]
    public List<string> Players { get; set; }
}

// [FirestoreData]
// public class Board
// {
//     public PointF[] Outline { get; }
//     public Cell[] Cells { get; init; }
//     public Membrane[] Membranes { get; init; }
//     public CellMembrane[] CellMembranes { get; init; }
// }