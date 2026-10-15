namespace EggMerge.Merge
{
    public enum CellKind
    {
        Empty,
        Egg
    }

    public struct BoardCell
    {
        public CellKind Kind;
        public int Tier;

        public static BoardCell Empty => new BoardCell { Kind = CellKind.Empty, Tier = 0 };

        public static BoardCell Egg(int tier) => new BoardCell
        {
            Kind = CellKind.Egg,
            Tier = tier
        };

        public bool IsEmpty => Kind == CellKind.Empty;
    }
}
