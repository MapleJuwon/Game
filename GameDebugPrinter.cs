using System.Text;

namespace ShadowLayer.Core
{
    public static class GameDebugPrinter
    {
        public static string BuildBoardSnapshot(GameState state)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Turn: {state.GlobalTurn} Current: {state.CurrentTurn} Status: {state.Status} Winner: {state.Winner}");
            sb.AppendLine($"Black Score={state.BlackPlayer.Score} Bombs={state.BlackPlayer.BombInventory} Pieces={state.BlackPlayer.PiecePositions.Count}");
            sb.AppendLine($"White Score={state.WhitePlayer.Score} Bombs={state.WhitePlayer.BombInventory} Pieces={state.WhitePlayer.PiecePositions.Count}");

            for (var y = 0; y < state.BoardSize; y++)
            {
                for (var x = 0; x < state.BoardSize; x++)
                {
                    var tile = state.Board[x, y];
                    if (tile.Occupant == null)
                    {
                        sb.Append('.');
                    }
                    else
                    {
                        sb.Append(tile.Occupant.Owner == PlayerId.Black ? 'B' : 'W');
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
