namespace ShadowLayer.Core
{
    public enum PlayerId
    {
        Black = 0,
        White = 1
    }

    public enum TurnActionType
    {
        PlacePiece = 0,
        PlaceBomb = 1,
        Forfeit = 2
    }

    public enum GameStatus
    {
        InProgress = 0,
        Finished = 1
    }

    public enum Winner
    {
        None = 0,
        Black = 1,
        White = 2,
        Draw = 3
    }
}
