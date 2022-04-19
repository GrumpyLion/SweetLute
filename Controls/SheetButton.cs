using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Controls;
using SweetLute.Notation.Persistance;
using Blish_HUD;
using Blish_HUD.Input;

namespace SweetLute.Controls
{
    public class SheetButton : DetailsButton {

        private const int SHEETBUTTON_WIDTH = 339;
        private const int SHEETBUTTON_HEIGHT = 48;

        public string Artist { get; set; }
        public string User { get; set; }

        #region Textures

        private readonly Texture2D PlaySprite;
        private readonly Texture2D GlowPlaySprite;
        private readonly Texture2D StopSprite;
        private readonly Texture2D GlowStopSprite;
        private readonly Texture2D BackgroundSprite;

        #endregion

        private RawMusicSheet _musicSheet;
        public RawMusicSheet MusicSheet
        {
            get => _musicSheet;
            set
            {
                if (_musicSheet == value) return;

                _musicSheet = value;
                OnPropertyChanged();
            }
        }

        public SheetButton()
        {
            StopSprite = StopSprite ?? SweetLuteModule.ModuleInstance.ContentsManager.GetTexture("stop.png");
            GlowStopSprite = GlowStopSprite ?? SweetLuteModule.ModuleInstance.ContentsManager.GetTexture("glow_stop.png");
            PlaySprite = PlaySprite ?? SweetLuteModule.ModuleInstance.ContentsManager.GetTexture("play.png");
            GlowPlaySprite = GlowPlaySprite ?? SweetLuteModule.ModuleInstance.ContentsManager.GetTexture("glow_play.png");
            BackgroundSprite = BackgroundSprite ?? ContentService.Textures.Pixel;

            MouseMoved += SheetButton_MouseMoved;
            MouseLeft += SheetButton_MouseLeft;
            BottomSectionHeight = 0;
            Size = new Point(SHEETBUTTON_WIDTH, SHEETBUTTON_HEIGHT);
        }

        #region Mouse Interaction

        private bool _mouseOverPlay = false;
        public bool MouseOverPlay
        {
            get => _mouseOverPlay;
            set
            {
                if (_mouseOverPlay == value) return;
                _mouseOverPlay = value;
                Invalidate();
            }
        }

        private bool _mouseOverButton = false;
        public bool MouseOverButton
        {
            get => _mouseOverButton;
            set
            {
                if (_mouseOverButton == value) return;
                _mouseOverButton = value;
                Invalidate();
            }
        }

        private void SheetButton_MouseLeft(object sender, MouseEventArgs e)
        {
            MouseOverPlay = false;
            MouseOverButton = false;
        }

        private void SheetButton_MouseMoved(object sender, MouseEventArgs e)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var relPos = e.MouseState.Position - AbsoluteBounds.Location;
#pragma warning restore CS0618 // Type or member is obsolete

            if (MouseOver && relPos.Y > 0)
                MouseOverPlay = relPos.X < SHEETBUTTON_WIDTH && relPos.X > SHEETBUTTON_WIDTH - 32;
            else
                MouseOverPlay = false;

            if (MouseOver && relPos.Y > 0)
                MouseOverButton = relPos.X < SHEETBUTTON_WIDTH && relPos.X > 0;
            else
                MouseOverButton = false;

            if (MouseOverPlay)
                BasicTooltipText = "Play";
            else
                BasicTooltipText = Title;
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse | CaptureType.Filter;
        }

        #endregion

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_mouseOverButton)
            {
                if (_mouseOverPlay)
                    spriteBatch.DrawOnCtrl(this, GlowPlaySprite, new Rectangle(SHEETBUTTON_WIDTH - 32, 6, 23, 32), Color.White);
                else
                    spriteBatch.DrawOnCtrl(this, PlaySprite, new Rectangle(SHEETBUTTON_WIDTH - 32, 6, 23, 32), Color.White);
            }
            spriteBatch.DrawOnCtrl(this, BackgroundSprite, bounds, Color.Black * 0.25f);

            // Draw instrument icon
            if (Icon != null) {
                spriteBatch.DrawOnCtrl(this, this.Icon, new Rectangle(0, 0, 48, 48), Color.White);
            }

            // Wrap text
            string track = Title + @" - " + Artist;
            string wrappedText = Blish_HUD.DrawUtil.WrapText(Content.DefaultFont14, track, SHEETBUTTON_WIDTH - 50 - SHEETBUTTON_HEIGHT - 20);
            spriteBatch.DrawStringOnCtrl(this, wrappedText, Content.DefaultFont14, new Rectangle(58, 0, 216, this.Height), Color.White, false, true, 2, HorizontalAlignment.Left, VerticalAlignment.Middle);
        }
    }
}