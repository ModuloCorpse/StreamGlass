﻿using StreamFeedstock.Controls;

namespace StreamGlass.Settings
{
    public class NumericUpDownUserControlLink : UserControlLink
    {
        private readonly NumericUpDown m_NumericUpDown;
        public NumericUpDownUserControlLink(NumericUpDown numericUpDown) => m_NumericUpDown = numericUpDown;
        protected override void Load() => m_NumericUpDown.Value = double.Parse(GetSettings());
        protected override void Save() => SetSettings(m_NumericUpDown.Value.ToString());
    }
}