namespace K5E.View
{
    using K5E.Source.Controls;
    using K5E.Source.DTMEditor;
    using System;
    using System.ComponentModel;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for DTMEditor.xaml.
    /// </summary>
    public partial class DTMEditor : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Output" /> class.
        /// </summary>
        public DTMEditor()
        {
            this.InitializeComponent();

            this.InsertFramesHexDecBoxViewModel = this.InsertFramesHexDecBox.DataContext as HexDecBoxViewModel;
            this.InsertFramesHexDecBoxViewModel.PropertyChanged += this.InsertFramesValueChanged;
            this.InsertFramesHexDecBoxViewModel.SetValue(1);

            this.GoToFrameHexDecBoxViewModel = this.GoToFrameHexDecBox.DataContext as HexDecBoxViewModel;
            this.GoToFrameHexDecBoxViewModel.PropertyChanged += this.GoToFrameValueChanged;
            this.GoToFrameHexDecBoxViewModel.SetValue(0);
        }

        private HexDecBoxViewModel InsertFramesHexDecBoxViewModel { get; set; }
        private HexDecBoxViewModel GoToFrameHexDecBoxViewModel { get; set; }

        private void InsertFramesValueChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.InsertFramesHexDecBoxViewModel.Text) && this.InsertFramesHexDecBoxViewModel.IsValid)
            {
                DTMEditorViewModel.GetInstance().InsertFrameCount = (int)this.InsertFramesHexDecBoxViewModel.GetValue();
            }
        }

        private void GoToFrameValueChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.GoToFrameHexDecBoxViewModel.Text) && this.InsertFramesHexDecBoxViewModel.IsValid)
            {
                DTMEditorViewModel.GetInstance().ActiveGoToFrame = (int)this.InsertFramesHexDecBoxViewModel.GetValue();
            }
        }
    }
    //// End class
}
//// End namespace