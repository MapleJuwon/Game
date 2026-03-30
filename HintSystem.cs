namespace ShadowLayer.Core
{
    public sealed class HintSystem
    {
        private readonly BoardManager _boardManager;
        private readonly BombSystem _bombSystem;

        public HintSystem(BoardManager boardManager, BombSystem bombSystem)
        {
            _boardManager = boardManager;
            _bombSystem = bombSystem;
        }

        public void Recalculate(GameState state)
        {
            UpdatePlayerHints(state, PlayerId.Black);
            UpdatePlayerHints(state, PlayerId.White);
        }

        private void UpdatePlayerHints(GameState state, PlayerId playerId)
        {
            var player = state.GetPlayer(playerId);
            foreach (var pos in player.PiecePositions)
            {
                var tile = _boardManager.GetTile(state, pos);
                if (tile.Occupant == null)
                {
                    continue;
                }

                tile.Occupant.EnemyBombHintCount = _bombSystem.CountEnemyBombsIn3x3(state, playerId, pos);
            }
        }
    }
}
