using System.Collections.Generic;

namespace ShadowLayer.Core
{
    public sealed class BoardManager
    {
        public bool IsInBounds(GameState state, BoardPosition position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < state.BoardSize && position.Y < state.BoardSize;
        }

        public IEnumerable<BoardPosition> GetNeighbors3x3(GameState state, BoardPosition center)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    var p = new BoardPosition(center.X + dx, center.Y + dy);
                    if (IsInBounds(state, p))
                    {
                        yield return p;
                    }
                }
            }
        }

        public TileState GetTile(GameState state, BoardPosition position) => state.Board[position.X, position.Y];
    }
}
