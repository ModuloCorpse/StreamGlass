using CorpseLib.Translation;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace StreamGlass.Core.Controls
{
    public partial class Window : System.Windows.Window
    {
        [Flags]
        private enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_LAST
        }

        [Flags]
        private enum DWMNCRenderingPolicy : uint
        {
            UseWindowStyle,
            Disabled,
            Enabled,
            Last
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwmAttribute, IntPtr pvAttribute, uint cbAttribute);

        protected void UpdateAeroPeek()
        {
            var helper = new WindowInteropHelper(this);
            helper.EnsureHandle();
            if (DwmIsCompositionEnabled())
            {
                var status = Marshal.AllocHGlobal(sizeof(int));
                if (ShowInAero)
                    Marshal.WriteInt32(status, 0); // false
                else
                    Marshal.WriteInt32(status, 1); // true
                _ = DwmSetWindowAttribute(helper.Handle, DwmWindowAttribute.DWMWA_EXCLUDED_FROM_PEEK, status, sizeof(int));
            }
        }

        #region ShowInAero
        public static readonly DependencyProperty ShowInAeroProperty = Helper.NewProperty<Window, bool>("ShowInAero", true);
        [Description("The brush palette key of the window's background"), Category("Common Properties")]
        public bool ShowInAero
        {
            get => (bool)GetValue(ShowInAeroProperty);
            set
            {
                SetValue(ShowInAeroProperty, value);
                UpdateAeroPeek();
            }
        }
        #endregion ShowInAero

        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<Window, string>("BrushPaletteKey", "background");
        [Description("The brush palette key of the window's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<Window, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the window's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        private readonly BrushPaletteManager m_Palette;

        public Window(BrushPaletteManager palette)
        {
            m_Palette = palette;
            MouseDown += Window_MouseDown;
            Loaded += Window_Loaded;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAeroPeek();
            Update();
        }

        public BrushPaletteManager GetBrushPalette() => m_Palette;

        public void SetCurrentPalette(string id)
        {
            m_Palette.SetCurrentPalette(id);
            Update();
        }

        public void SetCurrentTranslation(CultureInfo cultureInfo)
        {
            Translator.SetLanguage(cultureInfo);
            Update();
        }

        public void Update()
        {
            if (Owner is Window parent)
                parent.Update();
            if (m_Palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            if (m_Palette.TryGetColor(TextBrushPaletteKey, out var foreground))
                Foreground = foreground;
            if (Content is IUIElement updatable)
                updatable.Update(m_Palette);
            OnUpdate(m_Palette);
        }

        protected virtual void OnUpdate(BrushPaletteManager palette) {}
    }
}
