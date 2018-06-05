using System;
using System.Collections.Generic;

namespace Treasury.WebUtility
{

    public class SelectOption
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class RadioButton
    {
        public RadioButton()
        {
            Id = string.Empty;
            Name = string.Empty;
            Text = string.Empty;
            Value = string.Empty;
        }

        public string Name { get; set; }
        public bool Checked { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public string Id { get; set; }
    }

    public class CheckBoxListInfo
    {
        public string DisplayText { get; set; }
        public bool IsChecked { get; set; }
        public string Value { get; set; }
    }

    public class FormateTitle
    {
        public string OldTitle { get; set; }
        public string NewTitle { get; set; }
    }

}