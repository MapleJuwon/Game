namespace ShadowLayer.Core
{
    public sealed class FogOfWarSystem
    {
        private readonly BoardManager _boardManager;

        public FogOfWarSystem(BoardManager boardManager)
        {
            _boardManager = boardManager;
        }

        public void Recalculate(GameState state)
        {
            // Visibility is rebuilt from scratch each update to guarantee deterministic state.
            for (var x = 0; x < state.BoardSize; x++)
            {
                for (var y = 0; y < state.BoardSize; y++)
                {
                    var tile = state.Board[x, y];
                    tile.VisibleToBlack = false;
                    tile.VisibleToWhite = false;
                }
            }

            RevealFromPlayerPieces(state, PlayerId.Black);
            RevealFromPlayerPieces(state, PlayerId.White);
        }

        private void RevealFromPlayerPieces(GameState state, PlayerId playerId)
        {
            var player = state.GetPlayer(playerId);
            foreach (var piecePos in player.PiecePositions)
            {
                foreach (var neighbor in _boardManager.GetNeighbors3x3(state, piecePos))
                {
                    var tile = _boardManager.GetTile(state, neighbor);
                    if (playerId == PlayerId.Black)
                    {
                        tile.VisibleToBlack = true;
                    }
                    else
                    {
                        tile.VisibleToWhite = true;
                    }
                }
            }
        }

        public bool IsVisibleToPlayer(GameState state, BoardPosition position, PlayerId playerId)
        {
            var tile = _boardManager.GetTile(state, position);
            return playerId == PlayerId.Black ? tile.VisibleToBlack : tile.VisibleToWhite;
        }
    }
}
