namespace ShadowLayer.Core
{
    public sealed class TurnManager
    {
        public void AdvanceTurn(GameState state)
        {
            state.GlobalTurn += 1;
            state.CurrentTurn = state.CurrentTurn == PlayerId.Black ? PlayerId.White : PlayerId.Black;
        }

        public void RegisterPlayerAction(GameState state, PlayerId playerId)
        {
            var player = state.GetPlayer(playerId);
            player.TurnsTaken += 1;

            // Economy: +1 bomb every 5 turns per player.
            if (player.TurnsTaken % 5 == 0)
            {
                player.BombInventory += 1;
            }
        }
    }
}
