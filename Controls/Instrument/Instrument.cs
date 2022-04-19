using System;
using SweetLute.Domain.Values;
using System.Threading;
using Blish_HUD.Controls.Intern;
using System.Collections.Generic;
using Blish_HUD.Controls.Extern;

namespace SweetLute.Controls.Instrument
{
    public enum InstrumentSkillType
    {
        None,
        Note,
        IncreaseOctave,
        DecreaseOctave,
        StopPlaying
    }
    public enum InstrumentMode
    {
        None,
        Emulate
    }

    public abstract class Instrument
    {
        protected static readonly Dictionary<GuildWarsControls, VirtualKeyShort> VirtualKeyShorts = new Dictionary<GuildWarsControls, VirtualKeyShort>
        {
            {GuildWarsControls.WeaponSkill1, VirtualKeyShort.KEY_1},
            {GuildWarsControls.WeaponSkill2, VirtualKeyShort.KEY_2},
            {GuildWarsControls.WeaponSkill3, VirtualKeyShort.KEY_3},
            {GuildWarsControls.WeaponSkill4, VirtualKeyShort.KEY_4},
            {GuildWarsControls.WeaponSkill5, VirtualKeyShort.KEY_5},
            {GuildWarsControls.HealingSkill, VirtualKeyShort.KEY_6},
            {GuildWarsControls.UtilitySkill1, VirtualKeyShort.KEY_7},
            {GuildWarsControls.UtilitySkill2, VirtualKeyShort.KEY_8},
            {GuildWarsControls.UtilitySkill3, VirtualKeyShort.KEY_9},
            {GuildWarsControls.EliteSkill, VirtualKeyShort.KEY_0}
        };

        public InstrumentMode Mode { get; set; }

        public bool IsInstrument(string instrument)
        {
            return string.Equals(GetType().Name, instrument, StringComparison.OrdinalIgnoreCase);
        }

        protected void PressKey(GuildWarsControls key)
        {
            Keyboard.Press(VirtualKeyShorts[key]);
            Thread.Sleep(TimeSpan.FromMilliseconds(1));
            Keyboard.Release(VirtualKeyShorts[key]);
        }

        public void ResetInputLocation()
        {
            for (int i = 0; i < 4; ++i)
            {
                PressKey(GuildWarsControls.UtilitySkill3);
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }
        }
        public abstract void ResetOctave();

        public abstract void PlayNote(Note note);
        public abstract void GoToOctave(Note note);
    }
}