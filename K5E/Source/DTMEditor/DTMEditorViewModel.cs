namespace K5E.Source.DTMEditor
{
    using GalaSoft.MvvmLight.Command;
    using K5E.Engine.Common.Logging;
    using K5E.Source.Controls;
    using K5E.Source.Docking;
    using K5E.Source.PropertyViewer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;
    using System.Windows.Input;

    /// <summary>
    /// View model for the DTM Editor.
    /// </summary>
    public class DTMEditorViewModel : ToolViewModel
    {
        /// <summary>
        /// The list view to display frame information.
        /// </summary>
        private ListView frameInputsListView = new ListView();

        private Dictionary<int, ListViewItem> frameListViewCache = new Dictionary<int, ListViewItem>();

        /// <summary>
        /// Singleton instance of the <see cref="DTMEditorViewModel" /> class.
        /// </summary>
        private static DTMEditorViewModel dtmEditorViewModelInstance = new DTMEditorViewModel();

        /// <summary>
        /// Prevents a default instance of the <see cref="DTMEditorViewModel" /> class from being created.
        /// </summary>
        private DTMEditorViewModel() : base("DTM Editor")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);

            this.OpenFileCommand = new RelayCommand(() => this.OpenFile(), () => true);
            this.SaveFileCommand = new RelayCommand(() => this.SaveFile(), () => true);

            // Use reflection to set all propertygrid colors to dark, since some are otherwise not publically accessible
            PropertyInfo[] allProperties = this.frameInputsListView.GetType().GetProperties();
            IEnumerable<PropertyInfo> colorProperties = allProperties.Select(x => x).Where(x => x.PropertyType == typeof(Color));

            foreach (PropertyInfo propertyInfo in colorProperties)
            {
                propertyInfo.SetValue(this.frameInputsListView, DarkBrushes.K5EColorWhite, null);
            }

            this.frameInputsListView.OwnerDraw = true;
            this.frameInputsListView.BackColor = DarkBrushes.K5EColorPanel;
            this.frameInputsListView.ForeColor = DarkBrushes.K5EColorWhite;
            this.frameInputsListView.Columns.Add("Frames", 160 - 22);
            this.frameInputsListView.FullRowSelect = true;
            this.frameInputsListView.VirtualMode = true;
            this.frameInputsListView.View = View.Details;
            this.frameInputsListView.RetrieveVirtualItem += this.FrameInputsListView_RetrieveVirtualItem;
            this.frameInputsListView.CacheVirtualItems += this.FrameInputsListView_CacheVirtualItems;
            this.frameInputsListView.DrawColumnHeader += this.FrameInputsListView_DrawColumnHeader;
            this.frameInputsListView.DrawItem += this.FrameInputsListView_DrawItem;
            this.frameInputsListView.VirtualItemsSelectionRangeChanged += this.FrameInputsListView_SelectedIndexChanged;
            this.frameInputsListView.HideSelection = false;
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="DTMEditorViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static DTMEditorViewModel GetInstance()
        {
            return DTMEditorViewModel.dtmEditorViewModelInstance;
        }

        /// <summary>
        /// Gets the command to open a DTM file.
        /// </summary>
        public ICommand OpenFileCommand { get; private set; }

        /// <summary>
        /// Gets the command to save the DTM file.
        /// </summary>
        public ICommand SaveFileCommand { get; private set; }

        /// <summary>
        /// Gets the command to 
        /// </summary>
        public ICommand InsertFramesCommand { get; private set; }

        /// <summary>
        /// Gets the command to 
        /// </summary>
        public ICommand GoToFrameCommand { get; private set; }


        /// <summary>
        /// Gets the command to 
        /// </summary>
        public ICommand DeleteSelectedFramesCommand { get; private set; }

        public int ActiveGoToFrame { get; set; }

        public int InsertFrameCount { get; set; }

        /// <summary>
        /// Hosting container for the property grid windows form object. This is done because there is no good WPF equivalent of this control.
        /// Fortunately, Windows Forms has a .Net Core implementation, so we do not rely on .Net Framework at all for this.
        /// </summary>
        public WindowsFormsHost WindowsFormsHost
        {
            get
            {
                return new WindowsFormsHost() { Child = this.frameInputsListView };
            }
        }

        public DTMFileInfo DtmFile { get; private set; }

        /// <summary>
        /// Prompts the user to open a DTM file.
        /// </summary>
        private void OpenFile()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

                openFileDialog.Filter = "DTM files (*.dtm)|*.dtm|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    this.DtmFile = new DTMFileInfo(openFileDialog.FileName);

                    this.frameInputsListView.VirtualListSize = this.DtmFile.FrameInfo.Count;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error opening DTM file.", ex);
            }
        }
        /// <summary>
        /// Prompts the user to save a DTM file.
        /// </summary>
        private void SaveFile()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

                saveFileDialog.Filter = "DTM files (*.dtm)|*.dtm|All files (*.*)|*.*";
                saveFileDialog.InitialDirectory = this.DtmFile.FilePath;

                if (saveFileDialog.ShowDialog() == true)
                {
                    this.DtmFile.Save(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error saving DTM file.", ex);
            }
        }

        private void FrameInputsListView_SelectedIndexChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            int selectionCount = e.EndIndex - e.StartIndex + 1;

            if (selectionCount < 0 || e.StartIndex < 0 || e.EndIndex < 0)
            {
                PropertyViewerViewModel.GetInstance().SetTargetObjects(null);
                return;
            }

            DTMFrameInfo[] frames = new DTMFrameInfo[selectionCount];

            for (int index = e.StartIndex; index < e.EndIndex; index++)
            {
                frames[index] = this.DtmFile.FrameInfo[index];
            }

            PropertyViewerViewModel.GetInstance().SetTargetObjects(frames);
        }

        private void FrameInputsListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (e.Item.Selected)
            {
                Rectangle rowBounds = e.Bounds;
                int leftMargin = e.Item.GetBounds(ItemBoundsPortion.Label).Left;
                Rectangle bounds = new Rectangle(leftMargin, rowBounds.Top, rowBounds.Width - leftMargin, rowBounds.Height);
                e.Graphics.FillRectangle(SystemBrushes.Highlight, bounds);
                e.DrawText();
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void FrameInputsListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(DarkBrushes.K5EColorPanel))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            using (SolidBrush foreBrush = new SolidBrush(DarkBrushes.K5EColorWhite))
            {
                e.Graphics.DrawString(e.Header.Text, e.Font, foreBrush, e.Bounds);
            }
        }

        private void FrameInputsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (frameListViewCache.ContainsKey(e.ItemIndex))
            {
                e.Item = frameListViewCache[e.ItemIndex];
                return;
            }

            frameListViewCache[e.ItemIndex] = new ListViewItem(String.Format("Frame {0}", e.ItemIndex));
            e.Item = frameListViewCache[e.ItemIndex];
        }

        private void FrameInputsListView_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            for (int index = e.StartIndex; index < e.EndIndex; index++)
            {
                if (!frameListViewCache.ContainsKey(index))
                {
                    frameListViewCache[index] = new ListViewItem(String.Format("Frame {0}", index));
                }
            }
        }
    }
    //// End class
}
//// End namespace
