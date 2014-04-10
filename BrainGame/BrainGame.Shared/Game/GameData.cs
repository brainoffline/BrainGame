using PropertyChanged;

namespace BinaryJr
{
    [ImplementPropertyChanged]
    public class GameData
    {
        public int BestScore { get; set; }
        public int BestPiece { get; set; }

        public int MoveCount { get; set; }
        public int GameCount { get; set; }
        public bool HasDonated { get; set; }

        public void Reset()
        {
            BestScore = 0;
            BestPiece = 0;
        }
    }
}