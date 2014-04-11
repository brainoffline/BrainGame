using System;
using System.Collections.Generic;
using Brain.Utils;
using PropertyChanged;

namespace BrainGame.Game
{

    public enum MoveType
    {
        Up, Right, Down, Left
    }

    [ImplementPropertyChanged]
    public class BinaryGame
    {
        private const int StartTiles = 2;

        public BinaryGame(int width = 4, int height = 4)
        {
            BinaryGrid = new BinaryGrid(width, height);
        }

        public void Setup()
        {
            Score = 0;
            IsGameOver = false;

            GameData.GameCount++;

            BinaryGrid.Setup();

            AddStartTiles();
        }

        public int Score { get; set; }

        private void AddStartTiles()
        {
            for (int i = 0; i < StartTiles; i++)
                AddRandomTile();
        }

        private void AddRandomTile()
        {
            if (BinaryGrid.AvailablePositionCount() > 0)
            {
                int value = (RandomManager.NextDouble() < 0.9) ? 2 : 4;
                var tile = BinaryGrid.InsertTile(BinaryGrid.RandomAvailablePosition(), value);

                RaiseTileAdded(tile);
            }
        }

        public void RestoreTile(TileData tile)
        {
            if (tile == null) return;

            BinaryGrid.InsertTile(tile);
            RaiseTileAdded(tile);
        }


        private void PrepareTiles()
        {
            BinaryGrid.ForEachTile((tile) =>
            {
                tile.MergedFrom = null;
                tile.PreviousPos = tile.Pos;
            });
        }

        public void Move(MoveType move)
        {
            if (IsGameOver) return;

            List<int> Xs;
            List<int> Ys;
            bool moved = false;
            var vector = GetVector(move);
            BuildTransversals(vector, out Xs, out Ys);

            PrepareTiles();
            foreach (var x in Xs)
            {
                foreach (var y in Ys)
                {
                    var pos = new XY(x, y);
                    var tile = BinaryGrid.TileAt(pos);

                    if (tile != null)
                    {
                        XY fathest, nextPos;
                        FindFarthestPosition(pos, vector, out fathest, out nextPos);
                        var nextTile = BinaryGrid.TileAt(nextPos);

                        if ((nextTile != null) &&
                            (nextTile.Value == tile.Value) &&
                            (nextTile.MergedFrom == null))
                        {
                            RaiseTileRemoved(nextTile);
                            BinaryGrid.RemoveTile(nextPos);

                            tile.MergedFrom = nextTile;
                            tile.Value *= 2;
                            BinaryGrid.MoveTile(tile, nextPos);
                            RaiseTileMoved(tile);

                            Score += tile.Value;

                            if (tile.Value >= 2048)
                            {
                                HasGameWon = true;
                                RaiseGameWon();
                            }
                            moved = true;
                        }
                        else
                        {
                            if ((pos.X != fathest.X) ||
                                (pos.Y != fathest.Y))
                            {
                                BinaryGrid.MoveTile(tile, fathest);
                                RaiseTileMoved(tile);
                                moved = true;
                            }
                        }
                    }
                }
            }

            if (moved)
            {
                GameData.MoveCount++;

                AddRandomTile();

                if (!MovesAvailable())
                {
                    IsGameOver = true;
                    RaiseGameOver();
                }
            }
        }

        private XY GetVector(MoveType move)
        {
            switch (move)
            {
                case MoveType.Left: return new XY(-1, 0);
                case MoveType.Up: return new XY(0, -1);
                case MoveType.Right: return new XY(1, 0);
                case MoveType.Down: return new XY(0, 1);
                default:
                    return null;
            }
        }

        private void BuildTransversals(XY vector, out List<int> Xs, out List<int> Ys)
        {
            Xs = new List<int>();
            Ys = new List<int>();

            for (int x = 0; x < BinaryGrid.Width; x++)
                Xs.Add(x);
            for (int y = 0; y < BinaryGrid.Width; y++)
                Ys.Add(y);

            if (vector.X == 1)
                Xs.Reverse();
            if (vector.Y == 1)
                Ys.Reverse();
        }

        private void FindFarthestPosition(XY pos, XY vector, out XY fathest, out XY next)
        {
            XY previous = null;

            do
            {
                previous = pos;
                pos = new XY(previous.X + vector.X, previous.Y + vector.Y);
            } while (BinaryGrid.WithinBounds(pos) && BinaryGrid.PosAvailable(pos));

            fathest = previous;
            next = pos;
        }

        private bool MovesAvailable()
        {
            if (BinaryGrid.AvailablePositionCount() > 0)
                return true;
            if (TileMatchesAvailable())
                return true;
            return false;
        }

        private bool TileMatchesAvailable()
        {
            for (var x = 0; x < BinaryGrid.Width; x++)
            {
                for (var y = 0; y < BinaryGrid.Height; y++)
                {
                    TileData tile = BinaryGrid.Tiles[x, y];
                    if (tile != null)
                    {
                        for (var direction = 0; direction < 4; direction++)
                        {
                            var vector = GetVector((MoveType)direction);
                            var pos = new XY(x + vector.X, y + vector.Y);

                            var other = BinaryGrid.TileAt(pos);
                            if ((other != null) && (other.Value == tile.Value))
                                return true;
                        }
                    }
                }
            }
            return false;
        }




        /**************************************************************/

        public event EventHandler<TileData> TileAdded;
        public event EventHandler<TileData> TileMoved;
        public event EventHandler<TileData> TileRemoved;
        public event EventHandler GameWon;
        public event EventHandler GameOver;

        protected virtual void RaiseTileAdded(TileData tile)
        {
            EventHandler<TileData> handler = TileAdded;
            if (handler != null) handler(this, tile);
        }
        protected virtual void RaiseTileMoved(TileData tile)
        {
            EventHandler<TileData> handler = TileMoved;
            if (handler != null) handler(this, tile);
        }
        protected virtual void RaiseTileRemoved(TileData tile)
        {
            EventHandler<TileData> handler = TileRemoved;
            if (handler != null) handler(this, tile);
        }
        protected virtual void RaiseGameWon()
        {
            EventHandler handler = GameWon;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        protected virtual void RaiseGameOver()
        {
            EventHandler handler = GameOver;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /**************************************************************/

        public bool IsGameOver { get; set; }
        public bool HasGameWon { get; set; }

        public BinaryGrid BinaryGrid { get; private set; }
        public GameData GameData { get; set; }

    }
}
