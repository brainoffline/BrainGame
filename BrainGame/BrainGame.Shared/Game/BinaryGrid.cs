using System;
using System.Collections.Generic;
using Brain.Utils;
using BrainGame.DataModel;
using PropertyChanged;

namespace BrainGame.Game
{
    [ImplementPropertyChanged]
    public class BinaryGrid
    {
        public BinaryGrid(GameDefinition gameDefinition)
        {
            this.gameDefinition = gameDefinition;

            Width = gameDefinition.Width;
            Height = gameDefinition.Height;
            Tiles = new TileData[gameDefinition.Width, gameDefinition.Height];
        }

        private GameDefinition gameDefinition;
        public TileData[,] Tiles { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public void ForEachTile(Action<TileData> action)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Tiles[x,y] != null)
                        action(Tiles[x, y]);
                }
            }
        }

        public void Setup()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y] = null;
                }
            }
        }


        public XY RandomAvailablePosition()
        {
            var available = AvailablePositions();
            if (available.Count <= 0) return null;

            var index = RandomManager.Next(available.Count);
            return available[index];
        }

        public List<XY> AvailablePositions()
        {
            var list = new List<XY>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Tiles[x, y] == null)
                        list.Add(new XY(x,y));
                }
            }
            return list;
        }

        public int AvailablePositionCount()
        {
            return AvailablePositions().Count;
        }

        public TileData InsertTile(XY pos, int value)
        {
            var tile = new TileData(pos, value, gameDefinition);
            Tiles[pos.X, pos.Y] = tile;
            return tile;
        }

        public void InsertTile(TileData tile)
        {
            Tiles[tile.Pos.X, tile.Pos.Y] = tile;
        }

        public void RemoveTile(XY pos)
        {
            Tiles[pos.X, pos.Y] = null;
        }

        public void MoveTile(TileData tile, XY pos)
        {
            Tiles[tile.Pos.X, tile.Pos.Y] = null;
            Tiles[pos.X, pos.Y] = tile;
            tile.Pos = pos;
        }

        public bool WithinBounds(XY pos)
        {
            return ((pos.X >= 0 && pos.X < Width) &&
                    (pos.Y >= 0 && pos.Y < Height));
        }

        public bool PosAvailable(XY pos)
        {
            return Tiles[pos.X, pos.Y] == null;
        }

        public TileData TileAt(XY pos)
        {
            if (WithinBounds(pos))
                return Tiles[pos.X, pos.Y];
            return null;
        }

    }
}