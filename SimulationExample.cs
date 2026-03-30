namespace ShadowLayer.Core
{
    public static class SimulationExample
    {
        public static string RunSample()
        {
            var controller = new GameStateController();
            var state = controller.CreateNewGame();

            // Minimal deterministic sequence for validation/testing.
            controller.ExecuteTurn(state, TurnAction.PlacePiece(1, 0));  // Black
            controller.ExecuteTurn(state, TurnAction.PlacePiece(17, 18)); // White
            controller.ExecuteTurn(state, TurnAction.PlacePiece(1, 1));  // Black
            controller.ExecuteTurn(state, TurnAction.PlacePiece(17, 17)); // White
            controller.ExecuteTurn(state, TurnAction.PlacePiece(2, 1));  // Black (gets first bomb after 5th black action)

            return GameDebugPrinter.BuildBoardSnapshot(state);
        }
    }
}
