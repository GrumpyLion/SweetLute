using System.Linq;
using System.Threading;
using SweetLute.Controls.Instrument;
using SweetLute.Domain;
using SweetLute.Player.Algorithms;
using System;

namespace SweetLute.Player
{
    public class MusicPlayer : IDisposable
    {
        public Thread Worker { get; private set; }
        public IPlayAlgorithm Algorithm { get; private set; }

        public MusicPlayer(MusicSheet musicSheet, Instrument instrument, IPlayAlgorithm algorithm)
        {
            Algorithm = algorithm;
            Worker = new Thread(() => algorithm.Play(instrument, musicSheet.MetronomeMark, musicSheet.Melody.ToArray()));
        }

        public void Dispose() {
            Algorithm.Dispose();
        }
    }
}