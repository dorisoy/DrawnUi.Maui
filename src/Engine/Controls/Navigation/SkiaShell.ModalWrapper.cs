﻿using System.Security.Cryptography.X509Certificates;

namespace DrawnUi.Maui.Controls;

public partial class SkiaShell
{
    public class ModalWrapper : ContentLayout
    {
        private readonly bool _animated;
        private readonly SkiaShell _shell;

        public ModalWrapper(bool useGestures,
            bool animated,
            bool willFreeze,
            Color backgroundColor,
            SkiaShell shell)
        {
            _willFreeze = willFreeze;
            _animated = animated;
            _shell = shell;
            if (backgroundColor == null)
                backgroundColor = PopupBackgroundColor;
            _color = backgroundColor;
            _useGestures = useGestures;
        }

        public virtual void WrapContent(SkiaControl content)
        {
            var drawer = new SkiaDrawer()
            {
                RespondsToGestures = _useGestures,
                Animated = _animated,
                Tag = $"ModalDrawer_{content.GetType().Name}",
                Bounces = false,
                Direction = DrawerDirection.FromBottom,
                HeaderSize = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Content = content
            };

            this.Content = drawer;
        }

        public SkiaDrawer Drawer
        {
            get
            {
                return Content as SkiaDrawer; ;
            }
        }

        public SkiaBackdrop Backdrop { get; protected set; }

        protected override void SetContent(SkiaControl view)
        {
            base.SetContent(view);


            if (Backdrop == null)
            {
                Backdrop = new SkiaBackdrop()
                {
                    Tag = "backdrop",
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    ZIndex = -1,
                    InputTransparent = true,
                };
            }
            if (Views.All(x => x != Backdrop))
            {
                AddSubView(Backdrop);
            }

            if (Backdrop != null)
            {
                if (_willFreeze)
                {
                    Backdrop.Blur = 0;
                }
                else
                {
                    Backdrop.BackgroundColor = _color;
                    Backdrop.Blur = SkiaShell.PopupsBackgroundBlur;
                }
                //Backdrop.Brightness = 1 - _shell.FrozenBackgroundDim;
            }

            if (Content != null && _animated)
            {
                SetupAppearingAnimation(Content);
            }
        }

        protected override int DrawViews(SkiaDrawingContext context, SKRect destination, float scale, bool debug = false)
        {
            if (context.Superview == null || destination.Width <= 0 || destination.Height <= 0)
            {
                return 0;
            }

            var drawViews = GetOrderedSubviews();
            return RenderViewsList(drawViews, context, destination, scale);
        }

        public virtual void SetupAppearingAnimation(SkiaControl content)
        {
            //content.Scale = 0.5;
            //content.Opacity = 0.1;
        }

        //public async Task CloseAsync()
        //{
        //    if (Content != null && _animated && IsVisibleInViewTree())
        //    {
        //        await Task.WhenAll(
        //            Content.FadeToAsync(0, PopupsAnimationSpeed),
        //            Content.ScaleToAsync(0, 0, PopupsAnimationSpeed));
        //    }

        //    await _shell.Popups.Close(this, _animated);
        //}

        //protected override void OnLayoutReady()
        //{
        //    base.OnLayoutReady();

        //    if (Content != null && _animated)
        //    {
        //        Task.WhenAll(
        //            Content.FadeToAsync(1, PopupsAnimationSpeed),
        //            Content.ScaleToAsync(1, 1, PopupsAnimationSpeed));
        //    }


        //}

        bool frozen;
        private readonly Color _color;
        private readonly bool _willFreeze;
        private readonly bool _useGestures;

        public bool IsFrozen { get; set; }

        protected override void Draw(SkiaDrawingContext context, SKRect destination, float scale)
        {
            base.Draw(context, destination, scale);

            if (_willFreeze && !frozen && LayoutReady)
            {
                var screenshot = Backdrop.GetImage();
                if (screenshot != null)
                {
                    IsFrozen = true;
                    frozen = true;
                    _shell.FreezeRootLayout(this, screenshot, _animated, _color, (float)SkiaShell.PopupsBackgroundBlur).ConfigureAwait(true);
                    Backdrop.IsVisible = false;
                }
            }
        }


    }
}