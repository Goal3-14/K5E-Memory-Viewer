namespace K5E.Source.PropertyViewer
{
    using K5E.Source.Controls;
    using K5E.Source.Docking;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;

    /// <summary>
    /// View model for the Property Viewer.
    /// </summary>
    public class PropertyViewerViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="PropertyViewerViewModel" /> class.
        /// </summary>
        private static Lazy<PropertyViewerViewModel> propertyViewerViewModelInstance = new Lazy<PropertyViewerViewModel>(
                () => { return new PropertyViewerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The property grid to display selected objects.
        /// </summary>
        private PropertyGrid propertyGrid = new PropertyGrid();

        /// <summary>
        /// The objects being viewed.
        /// </summary>
        private Object[] targetObjects;

        /// <summary>
        /// Prevents a default instance of the <see cref="PropertyViewerViewModel" /> class from being created.
        /// </summary>
        private PropertyViewerViewModel() : base("Property Viewer")
        {
            // Use reflection to set all propertygrid colors to dark, since some are otherwise not publically accessible
            PropertyInfo[] allProperties = this.propertyGrid.GetType().GetProperties();
            IEnumerable<PropertyInfo> colorProperties = allProperties.Select(x => x).Where(x => x.PropertyType == typeof(Color));

            foreach(PropertyInfo propertyInfo in colorProperties)
            {
                propertyInfo.SetValue(this.propertyGrid, DarkBrushes.K5EColorPanel, null);
            }

            this.propertyGrid.BackColor = DarkBrushes.K5EColorPanel;
            this.propertyGrid.CommandsBackColor = DarkBrushes.K5EColorPanel;
            this.propertyGrid.HelpBackColor = DarkBrushes.K5EColorPanel;
            this.propertyGrid.SelectedItemWithFocusBackColor = DarkBrushes.K5EColorPanel;
            this.propertyGrid.ViewBackColor = DarkBrushes.K5EColorPanel;

            this.propertyGrid.CommandsActiveLinkColor = DarkBrushes.K5EColorPanel;
            this.propertyGrid.CommandsDisabledLinkColor = DarkBrushes.K5EColorPanel;

            this.propertyGrid.CategorySplitterColor = DarkBrushes.K5EColorWhite;

            this.propertyGrid.CommandsBorderColor = DarkBrushes.K5EColorFrame;
            this.propertyGrid.HelpBorderColor = DarkBrushes.K5EColorFrame;
            this.propertyGrid.ViewBorderColor = DarkBrushes.K5EColorFrame;

            this.propertyGrid.CategoryForeColor = DarkBrushes.K5EColorWhite;
            this.propertyGrid.CommandsForeColor = DarkBrushes.K5EColorWhite;
            this.propertyGrid.DisabledItemForeColor = DarkBrushes.K5EColorWhite;
            this.propertyGrid.HelpForeColor = DarkBrushes.K5EColorWhite;
            this.propertyGrid.SelectedItemWithFocusForeColor = DarkBrushes.K5EColorWhite;
            this.propertyGrid.ViewForeColor = DarkBrushes.K5EColorWhite;

            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets the objects being viewed.
        /// </summary>
        public Object[] TargetObjects
        {
            get
            {
                return this.targetObjects;
            }

            private set
            {
                this.targetObjects = value?.Where(x => x != null)?.ToArray();

                ControlThreadingHelper.InvokeControlAction(
                    this.propertyGrid, () => { this.propertyGrid.SelectedObjects = targetObjects == null ? new Object[] { } : targetObjects; }
                );

                this.RaisePropertyChanged(nameof(this.TargetObjects));
            }
        }

        /// <summary>
        /// Hosting container for the property grid windows form object. This is done because there is no good WPF equivalent of this control.
        /// Fortunately, Windows Forms has a .Net Core implementation, so we do not rely on .Net Framework at all for this.
        /// </summary>
        public WindowsFormsHost WindowsFormsHost
        {
            get
            {
                return new WindowsFormsHost() { Child = this.propertyGrid };
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="PropertyViewerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static PropertyViewerViewModel GetInstance()
        {
            return PropertyViewerViewModel.propertyViewerViewModelInstance.Value;
        }

        /// <summary>
        /// Sets the objects being viewed.
        /// </summary>
        /// <param name="targetObjects">The objects to view.</param>
        public void SetTargetObjects(params Object[] targetObjects)
        {
            this.TargetObjects = targetObjects;
        }
    }
    //// End class
}
//// End namespace