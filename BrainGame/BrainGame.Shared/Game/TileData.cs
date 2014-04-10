using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using BinaryJr;
using Newtonsoft.Json;
using PropertyChanged;

namespace BrainGame.Game
{
    [ImplementPropertyChanged]
    public class TileData
    {
        public TileData(XY pos, int value)
        {
            Pos = pos;
            Value = value;
        }

        [AlsoNotifyFor("BackgroundBrush", "ForegroundBrush")]
        public int Value { get; set; }

        public XY Pos { get; set; }
        public XY PreviousPos { get; set; }

        [JsonIgnore]
        public TileData MergedFrom { get; set; }

        [JsonIgnore]
        public Brush BackgroundBrush
        {
            get { return Application.Current.Resources["TileBackgroundBrush" + Value] as Brush; }
        }

        [JsonIgnore]
        public Brush ForegroundBrush
        {
            get { return Application.Current.Resources["TileForegroundBrush" + Value] as Brush; }
        }
    }
}