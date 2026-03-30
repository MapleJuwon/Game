using System;

namespace ShadowLayer.Core
{
    public sealed class GameStateController
    {
        private readonly BoardManager _boardManager;
        private readonly FogOfWarSystem _fogOfWarSystem;
        private readonly TerritoryCalculator _territoryCalculator;
        private readonly BombSystem _bombSystem;
        private readonly HintSystem _hintSystem;
        private readonly TurnManager _turnManager;

        public GameStateController()
        {
            _boardManager = new BoardManager();
            _bombSystem = new BombSystem(_boardManager);
            _fogOfWarSystem = new FogOfWarSystem(_boardManager);
            _territoryCalculator = new TerritoryCalculator(_boardManager);
            _hintSystem = new HintSystem(_boardManager, _bombSystem);
            _turnManager = new TurnManager();
        }

        public GameState CreateNewGame(int boardSize = 19)
        {
            if (boardSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(boardSize), "Board size must be positive.");
            }

            var state = new GameState(boardSize);
            PlaceInitialPiece(state, PlayerId.Black, new BoardPosition(0, 0));
            PlaceInitialPiece(state, PlayerId.White, new BoardPosition(boardSize - 1, boardSize - 1));
            RecalculateDerivedState(state);
            return state;
        }

        public TurnResolution ExecuteTurn(GameState state, TurnAction action)
        {
            if (state.Status != GameStatus.InProgress)
            {
                return Invalid("Game has already finished.");
            }

            var actor = state.CurrentTurn;
            var result = action.Type switch
            {
                TurnActionType.PlacePiece => ExecutePlacePiece(state, actor, action.Position),
                TurnActionType.PlaceBomb => ExecutePlaceBomb(state, actor, action.Position),
                TurnActionType.Forfeit => ExecuteForfeit(state, actor),
                _ => Invalid("Unknown action type.")
            };

            if (!result.IsValid)
            {
                return result;
            }

            _turnManager.RegisterPlayerAction(state, actor);
            RecalculateDerivedState(state);
            CheckWinCondition(state);

            if (state.Status == GameStatus.InProgress)
            {
                _turnManager.AdvanceTurn(state);
            }

            return result;
        }

        private TurnResolution ExecutePlacePiece(GameState state, PlayerId actor, BoardPosition target)
        {
            if (!_boardManager.IsInBounds(state, target))
            {
                return Invalid("Target tile is out of bounds.");
            }

            if (!_fogOfWarSystem.IsVisibleToPlayer(state, target, actor))
            {
                return Invalid("Target tile is not visible to current player.");
            }

            var tile = _boardManager.GetTile(state, target);
            if (tile.Occupant != null && tile.Occupant.Owner == actor)
            {
                return Invalid("Cannot place on your own piece.");
            }

            if (tile.Occupant != null && tile.Occupant.Owner != actor)
            {
                RemovePiece(state, target, tile.Occupant.Owner); // invasion
            }

            AddPiece(state, target, actor);

            // Trigger check occurs immediately after placement, before turn ends.
            _bombSystem.ResolveBombTriggerAndReward(state, actor, target, out var triggered, out var destroyed);

            return new TurnResolution
            {
                IsValid = true,
                Reason = triggered ? "Piece placed, enemy bomb triggered." : "Piece placed successfully.",
                TriggeredBomb = triggered,
                DestroyedPiecesCount = destroyed
            };
        }

        private TurnResolution ExecutePlaceBomb(GameState state, PlayerId actor, BoardPosition target)
        {
            if (!_boardManager.IsInBounds(state, target))
            {
                return Invalid("Target tile is out of bounds.");
            }

            if (!_fogOfWarSystem.IsVisibleToPlayer(state, target, actor))
            {
                return Invalid("Cannot place bomb on non-visible tile.");
            }

            if (!_bombSystem.PlaceBomb(state, actor, target))
            {
                return Invalid("No bomb inventory available.");
            }

            return new TurnResolution
            {
                IsValid = true,
                Reason = "Bomb placed successfully.",
                TriggeredBomb = false,
                DestroyedPiecesCount = 0
            };
        }

        private TurnResolution ExecuteForfeit(GameState state, PlayerId actor)
        {
            var player = state.GetPlayer(actor);
            player.HasForfeited = true;
            state.Status = GameStatus.Finished;
            state.Winner = actor == PlayerId.Black ? Winner.White : Winner.Black;

            return new TurnResolution
            {
                IsValid = true,
                Reason = "Player forfeited.",
                TriggeredBomb = false,
                DestroyedPiecesCount = 0
            };
        }

        private void RecalculateDerivedState(GameState state)
        {
            _fogOfWarSystem.Recalculate(state);
            _territoryCalculator.Recalculate(state);
            _hintSystem.Recalculate(state);
            RecalculateScores(state);
        }

        private void RecalculateScores(GameState state)
        {
            // Score model from PRD suggestion:
            // score = owned tiles + total territory levels.
            state.BlackPlayer.Score = 0;
            state.WhitePlayer.Score = 0;

            for (var x = 0; x < state.BoardSize; x++)
            {
                for (var y = 0; y < state.BoardSize; y++)
                {
                    var tile = state.Board[x, y];
                    if (tile.Occupant != null)
                    {
                        if (tile.Occupant.Owner == PlayerId.Black)
                        {
                            state.BlackPlayer.Score += 1;
                        }
                        else
                        {
                            state.WhitePlayer.Score += 1;
                        }
                    }

                    state.BlackPlayer.Score += tile.BlackAreaLevel;
                    state.WhitePlayer.Score += tile.WhiteAreaLevel;
                }
            }
        }

        private void CheckWinCondition(GameState state)
        {
            if (state.BlackPlayer.HasForfeited || state.WhitePlayer.HasForfeited)
            {
                return;
            }

            if (IsBoardFull(state))
            {
                state.Status = GameStatus.Finished;
                if (state.BlackPlayer.Score > state.WhitePlayer.Score)
                {
                    state.Winner = Winner.Black;
                }
                else if (state.WhitePlayer.Score > state.BlackPlayer.Score)
                {
                    state.Winner = Winner.White;
                }
                else
                {
                    state.Winner = Winner.Draw;
                }
            }
        }

        private bool IsBoardFull(GameState state)
        {
            for (var x = 0; x < state.BoardSize; x++)
            {
                for (var y = 0; y < state.BoardSize; y++)
                {
                    if (state.Board[x, y].Occupant == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void PlaceInitialPiece(GameState state, PlayerId owner, BoardPosition position)
        {
            AddPiece(state, position, owner);
        }

        private void AddPiece(GameState state, BoardPosition position, PlayerId owner)
        {
            var tile = _boardManager.GetTile(state, position);
            tile.Occupant = new Piece(owner, position);
            state.GetPlayer(owner).PiecePositions.Add(position);
        }

        private void RemovePiece(GameState state, BoardPosition position, PlayerId owner)
        {
            var tile = _boardManager.GetTile(state, position);
            tile.Occupant = null;
            state.GetPlayer(owner).PiecePositions.Remove(position);
        }

        private static TurnResolution Invalid(string reason)
        {
            return new TurnResolution
            {
                IsValid = false,
                Reason = reason,
                TriggeredBomb = false,
                DestroyedPiecesCount = 0
            };
        }
    }
}
