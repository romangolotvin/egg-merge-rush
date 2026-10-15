using System;
using System.Collections.Generic;
using EggMerge.Core;
using UnityEngine;

namespace EggMerge.Merge
{
    public class MergeBoard
    {
        public readonly int Width = GameConfig.BoardWidth;
        public readonly int Height = GameConfig.BoardHeight;

        readonly BoardCell[] _cells;
        readonly System.Random _rng;

        public int Score { get; private set; }
        public int MovesLeft { get; private set; }
        public int HighestTier { get; private set; } = 1;
        public event Action Changed;

        public MergeBoard(int moves, int seed)
        {
            _cells = new BoardCell[Width * Height];
            _rng = new System.Random(seed);
            MovesLeft = moves;
            FillSparse();
        }

        public BoardCell Get(int x, int y) => _cells[Index(x, y)];

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public bool TryMergeOrSwap(int x1, int y1, int x2, int y2)
        {
            if (!InBounds(x1, y1) || !InBounds(x2, y2))
                return false;
            if (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) != 1)
                return false;

            var a = Get(x1, y1);
            var b = Get(x2, y2);

            if (a.IsEmpty && b.IsEmpty)
                return false;

            if (!a.IsEmpty && !b.IsEmpty && a.Kind == CellKind.Egg && b.Kind == CellKind.Egg && a.Tier == b.Tier)
            {
                var newTier = Mathf.Min(GameConfig.MaxEggTier, a.Tier + 1);
                Set(x2, y2, BoardCell.Egg(newTier));
                Set(x1, y1, BoardCell.Empty);
                HighestTier = Mathf.Max(HighestTier, newTier);
                Score += newTier * 10;
                MovesLeft--;
                DropAndSpawn();
                Changed?.Invoke();
                return true;
            }

            // Swap / move into empty
            Set(x1, y1, b);
            Set(x2, y2, a);
            MovesLeft--;
            DropAndSpawn();
            Changed?.Invoke();
            return true;
        }

        public void AddMoves(int amount)
        {
            MovesLeft += amount;
            Changed?.Invoke();
        }

        public bool HasMoves => MovesLeft > 0;

        void FillSparse()
        {
            for (var i = 0; i < _cells.Length; i++)
                _cells[i] = BoardCell.Empty;

            var spawns = Width * 2;
            for (var n = 0; n < spawns; n++)
            {
                var empty = EmptySlots();
                if (empty.Count == 0) break;
                var slot = empty[_rng.Next(empty.Count)];
                _cells[slot] = BoardCell.Egg(RollTier());
            }

            Changed?.Invoke();
        }

        void DropAndSpawn()
        {
            for (var x = 0; x < Width; x++)
            {
                var write = 0;
                for (var y = 0; y < Height; y++)
                {
                    var c = Get(x, y);
                    if (!c.IsEmpty)
                    {
                        Set(x, write, c);
                        if (write != y)
                            Set(x, y, BoardCell.Empty);
                        write++;
                    }
                }

                for (var y = write; y < Height; y++)
                {
                    if (_rng.NextDouble() < 0.55)
                        Set(x, y, BoardCell.Egg(RollTier()));
                    else
                        Set(x, y, BoardCell.Empty);
                }
            }
        }

        int RollTier()
        {
            var roll = _rng.NextDouble();
            if (roll < 0.70) return 1;
            if (roll < 0.92) return 2;
            return 3;
        }

        List<int> EmptySlots()
        {
            var list = new List<int>();
            for (var i = 0; i < _cells.Length; i++)
                if (_cells[i].IsEmpty) list.Add(i);
            return list;
        }

        int Index(int x, int y) => y * Width + x;

        void Set(int x, int y, BoardCell cell) => _cells[Index(x, y)] = cell;
    }
}
