namespace ShadowLayer.Core
{
    public sealed class TerritoryCalculator
    {
        private readonly BoardManager _boardManager;

        public TerritoryCalculator(BoardManager boardManager)
        {
            _boardManager = boardManager;
        }

        public void Recalculate(GameState state)
        {
            // Territory is an accumulated field, so full recompute prevents drift.
            for (var x = 0; x < state.BoardSize; x++)
            {
                for (var y = 0; y < state.BoardSize; y++)
                {
                    var tile = state.Board[x, y];
                    tile.BlackAreaLevel = 0;
                    tile.WhiteAreaLevel = 0;
                }
            }

            ApplyTerritoryFromPieces(state, PlayerId.Black);
            ApplyTerritoryFromPieces(state, PlayerId.White);
        }

        private void ApplyTerritoryFromPieces(GameState state, PlayerId playerId)
        {
            var player = state.GetPlayer(playerId);
            foreach (var piecePos in player.PiecePositions)
            {
                foreach (var affected in _boardManager.GetNeighbors3x3(state, piecePos))
                {
                    var tile = _boardManager.GetTile(state, affected);
                    if (playerId == PlayerId.Black)
                    {
                        tile.BlackAreaLevel += 1;
                    }
                    else
                    {
                        tile.WhiteAreaLevel += 1;
                    }
                }
            }
        }
    }
}
