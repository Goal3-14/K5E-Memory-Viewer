namespace K5E.Source.Controls
{
    using K5E.Engine.Common.Extensions;
    using System;
    using System.ComponentModel;

    public class SortedCategory : CategoryAttribute
    {
        public enum CategoryType
        {
            [Description("Frame (read-only)")]
            Frame = 0,

            [Description("Buttons")]
            Buttons = 1,

            [Description("Triggers")]
            Triggers = 2,

            [Description("Control Sticks")]
            ControlSticks = 3,

            [Description("D-Pad")]
            DPad = 4,
        }

        private const Char NonPrintableChar = '\t';

        public SortedCategory(CategoryType category)
            : base(category.GetDescription().PadLeft(category.GetDescription().Length + Enum.GetNames(typeof(CategoryType)).Length - (Int32)category, SortedCategory.NonPrintableChar))
        {
        }
    }
    //// End class
}
//// End namespace
///