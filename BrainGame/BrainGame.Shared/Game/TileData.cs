using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using BrainGame.DataModel;
using Newtonsoft.Json;
using PropertyChanged;

namespace BrainGame.Game
{
    [ImplementPropertyChanged]
    public class TileData
    {
        public TileData(XY pos, int value, GameDefinition gameDefinition)
        {
            Pos = pos;
            Value = value;
            this.gameDefinition = gameDefinition;
        }

        private GameDefinition gameDefinition;

        [AlsoNotifyFor("BackgroundBrush", "ForegroundBrush", "DisplayValue")]
        public int Value { get; set; }

        public string DisplayValue
        {
            get
            {
                if (gameDefinition.Style == "ABC")
                {
                    switch (Value)
                    {
                        case 2: return "A";
                        case 4: return "B";
                        case 8: return "C";
                        case 16: return "D";
                        case 32: return "E";
                        case 64: return "F";
                        case 128: return "G";
                        case 256: return "H";
                        case 1024: return "I";
                        case 2048: return "J";
                        case 4096: return "K";
                        case 8192: return "L";
                        case 16384: return "M";
                        default:
                            break;
                    }
                }
                return Value.ToString();
            }
        }

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