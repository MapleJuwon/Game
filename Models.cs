using System;
using System.Collections.Generic;

namespace ShadowLayer.Core
{
    public readonly struct BoardPosition : IEquatable<BoardPosition>
    {
        public int X { get; }
        public int Y { get; }

        public BoardPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(BoardPosition other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is BoardPosition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X},{Y})";
    }

    public sealed class Piece
    {
        public PlayerId Owner { get; }
        public BoardPosition Position { get; }
        public int EnemyBombHintCount { get; set; }

        public Piece(PlayerId owner, BoardPosition position)
        {
            Owner = owner;
            Position = position;
            EnemyBombHintCount = 0;
        }
    }

    public sealed class TileState
    {
        public Piece Occupant { get; set; }
        public bool VisibleToBlack { get; set; }
        public bool VisibleToWhite { get; set; }
        public int BlackAreaLevel { get; set; }
        public int WhiteAreaLevel { get; set; }
        public bool HasBlackBomb { get; set; }
        public bool HasWhiteBomb { get; set; }
    }

    public sealed class PlayerState
    {
        public PlayerId Id { get; }
        public int TurnsTaken { get; set; }
        public int BombInventory { get; set; }
        public bool HasForfeited { get; set; }
        public int Score { get; set; }
        public HashSet<BoardPosition> PiecePositions { get; }

        public PlayerState(PlayerId id)
        {
            Id = id;
            TurnsTaken = 0;
            BombInventory = 0;
            HasForfeited = false;
            Score = 0;
            PiecePositions = new HashSet<BoardPosition>();
        }
    }

    public sealed class GameState
    {
        public int BoardSize { get; }
        public TileState[,] Board { get; }
        public PlayerState BlackPlayer { get; }
        public PlayerState WhitePlayer { get; }
        public PlayerId CurrentTurn { get; set; }
        public int GlobalTurn { get; set; }
        public GameStatus Status { get; set; }
        public Winner Winner { get; set; }

        public GameState(int boardSize)
        {
            BoardSize = boardSize;
            Board = new TileState[boardSize, boardSize];
            for (var x = 0; x < boardSize; x++)
            {
                for (var y = 0; y < boardSize; y++)
                {
                    Board[x, y] = new TileState();
                }
            }

            BlackPlayer = new PlayerState(PlayerId.Black);
            WhitePlayer = new PlayerState(PlayerId.White);
            CurrentTurn = PlayerId.Black;
            GlobalTurn = 0;
            Status = GameStatus.InProgress;
            Winner = Winner.None;
        }

        public PlayerState GetPlayer(PlayerId playerId) => playerId == PlayerId.Black ? BlackPlayer : WhitePlayer;
        public PlayerState GetOpponent(PlayerId playerId) => playerId == PlayerId.Black ? WhitePlayer : BlackPlayer;
    }
}
