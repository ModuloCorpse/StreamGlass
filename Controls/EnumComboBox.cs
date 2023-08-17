using System;
using System.Windows.Controls;

namespace StreamGlass.Controls
{
    public abstract class EnumComboBox<T> : ComboBox where T : struct, Enum
    {
        public EnumComboBox()
        {
            foreach (T value in Enum.GetValues<T>())
                Items.Add(GetName(value));
            SelectedIndex = 0;
        }

        public EnumComboBox(T selectedValue)
        {
            int i = 0;
            int idx = 0;
            foreach (T value in Enum.GetValues<T>())
            {
                Items.Add(GetName(value));
                if (value.Equals(selectedValue))
                    idx = i;
                ++i;
            }
            SelectedIndex = idx;
        }

        public void SetSelectedEnumValue(T selectedValue)
        {
            int i = 0;
            foreach (T value in Enum.GetValues<T>())
            {
                if (value.Equals(selectedValue))
                {
                    SelectedIndex = i;
                    return;
                }
                ++i;
            }
        }

        public T SelectedEnumValue => (SelectedIndex >= 0) ? Enum.GetValues<T>()[SelectedIndex] : Enum.GetValues<T>()[0];

        protected abstract string GetName(T value);
    }
}
