namespace ShadowLayer.Core
{
    public readonly struct TurnAction
    {
        public TurnActionType Type { get; }
        public BoardPosition Position { get; }

        private TurnAction(TurnActionType type, BoardPosition position)
        {
            Type = type;
            Position = position;
        }

        public static TurnAction PlacePiece(int x, int y) => new TurnAction(TurnActionType.PlacePiece, new BoardPosition(x, y));
        public static TurnAction PlaceBomb(int x, int y) => new TurnAction(TurnActionType.PlaceBomb, new BoardPosition(x, y));
        public static TurnAction Forfeit() => new TurnAction(TurnActionType.Forfeit, new BoardPosition(-1, -1));
    }

    public sealed class TurnResolution
    {
        public bool IsValid { get; set; }
        public string Reason { get; set; }
        public bool TriggeredBomb { get; set; }
        public int DestroyedPiecesCount { get; set; }
    }
}
