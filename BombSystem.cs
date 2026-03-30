using System.Collections.Generic;

namespace ShadowLayer.Core
{
    public sealed class BombSystem
    {
        private readonly BoardManager _boardManager;

        public BombSystem(BoardManager boardManager)
        {
            _boardManager = boardManager;
        }

        public bool PlaceBomb(GameState state, PlayerId playerId, BoardPosition position)
        {
            if (!_boardManager.IsInBounds(state, position))
            {
                return false;
            }

            var player = state.GetPlayer(playerId);
            if (player.BombInventory <= 0)
            {
                return false;
            }

            var tile = _boardManager.GetTile(state, position);
            if (playerId == PlayerId.Black)
            {
                tile.HasBlackBomb = true;
            }
            else
            {
                tile.HasWhiteBomb = true;
            }

            player.BombInventory -= 1;
            return true;
        }

        public bool HasEnemyBomb(TileState tile, PlayerId triggeringPlayer)
        {
            return triggeringPlayer == PlayerId.Black ? tile.HasWhiteBomb : tile.HasBlackBomb;
        }

        public int TriggerExplosion(GameState state, PlayerId triggeringPlayer, BoardPosition center)
        {
            var destroyed = 0;
            var triggeringOwner = state.GetPlayer(triggeringPlayer);

            // Atomic explosion transaction:
            // 1) Gather affected triggering player's pieces
            // 2) Remove them together so follow-up recalculations see a consistent snapshot
            var toDestroy = new List<BoardPosition>();
            foreach (var pos in _boardManager.GetNeighbors3x3(state, center))
            {
                var tile = _boardManager.GetTile(state, pos);
                if (tile.Occupant != null && tile.Occupant.Owner == triggeringPlayer)
                {
                    toDestroy.Add(pos);
                }
            }

            foreach (var pos in toDestroy)
            {
                var tile = _boardManager.GetTile(state, pos);
                tile.Occupant = null;
                if (triggeringOwner.PiecePositions.Remove(pos))
                {
                    destroyed += 1;
                }
            }

            return destroyed;
        }

        public void ResolveBombTriggerAndReward(GameState state, PlayerId triggeringPlayer, BoardPosition position, out bool triggered, out int destroyedCount)
        {
            var tile = _boardManager.GetTile(state, position);
            triggered = HasEnemyBomb(tile, triggeringPlayer);
            destroyedCount = 0;
            if (!triggered)
            {
                return;
            }

            // Bomb is consumed when triggered.
            if (triggeringPlayer == PlayerId.Black)
            {
                tile.HasWhiteBomb = false;
                state.WhitePlayer.BombInventory += 1; // reward to bomb owner
            }
            else
            {
                tile.HasBlackBomb = false;
                state.BlackPlayer.BombInventory += 1; // reward to bomb owner
            }

            destroyedCount = TriggerExplosion(state, triggeringPlayer, position);
        }

        public int CountEnemyBombsIn3x3(GameState state, PlayerId pieceOwner, BoardPosition center)
        {
            var bombs = 0;
            foreach (var p in _boardManager.GetNeighbors3x3(state, center))
            {
                var tile = _boardManager.GetTile(state, p);
                bombs += pieceOwner == PlayerId.Black
                    ? (tile.HasWhiteBomb ? 1 : 0)
                    : (tile.HasBlackBomb ? 1 : 0);
            }

            return bombs;
        }
    }
}
