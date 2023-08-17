namespace StreamGlass.Controls
{
    public enum ScrollPanelDisplayType
    {
        TOP_TO_BOTTOM,
        REVERSED_TOP_TO_BOTTOM,
        BOTTOM_TO_TOP,
        REVERSED_BOTTOM_TO_TOP
    }

    public class ScrollPanelDisplayTypeComboBox : EnumComboBox<ScrollPanelDisplayType>
    {
        public ScrollPanelDisplayTypeComboBox() : base() {}
        public ScrollPanelDisplayTypeComboBox(ScrollPanelDisplayType type) : base(type) { }

        protected override string GetName(ScrollPanelDisplayType value) => value switch
        {
            ScrollPanelDisplayType.TOP_TO_BOTTOM => "To bottom",
            ScrollPanelDisplayType.REVERSED_TOP_TO_BOTTOM => "Reversed to bottom",
            ScrollPanelDisplayType.BOTTOM_TO_TOP => "To top",
            ScrollPanelDisplayType.REVERSED_BOTTOM_TO_TOP => "Reversed to top",
            _ => ""
        };
    }
}
