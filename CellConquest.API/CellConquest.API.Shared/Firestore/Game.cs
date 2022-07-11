using System.Collections.Generic;
using System.Drawing;
using Google.Cloud.Firestore;

namespace CellConquest.API.Shared
{
    [FirestoreData]
    public class Game
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public GameState GameState { get; set; }

        [FirestoreProperty]
        public (PointF[] outline, Cell[] cells, Membrane[] membranes) Board { get; set; }

        [FirestoreProperty]
        public string Owner { get; set; }

        [FirestoreProperty]
        public string CurrentPlayerTurn { get; set; }

        [FirestoreProperty]
        public List<string> Players { get; set; }
    }

    public enum GameState
    {
        NotSet,
        WaitForPlayers,
        Playing,
        Paused,
        Finished
    }

    [FirestoreData]
    public class Cell
    {
        [FirestoreProperty]
        public Point[] Coordinates { get; set; }

        [FirestoreProperty]
        public string ConqueredBy { get; set; }
    }

    [FirestoreData]
    public class Membrane
    {
        [FirestoreProperty]
        public PointF[] Coordinates { get; set; }

        [FirestoreProperty]
        public string TouchedBy { get; set; }
    }
}