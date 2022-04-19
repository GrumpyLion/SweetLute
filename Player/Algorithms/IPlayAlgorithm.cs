using SweetLute.Controls.Instrument;
using SweetLute.Domain.Values;
using SweetLute.Controls;
namespace SweetLute.Player.Algorithms
{
    public interface IPlayAlgorithm
    {
        void Play(Instrument instrument, MetronomeMark metronomeMark, ChordOffset[] melody);
        void Dispose();
    }
}