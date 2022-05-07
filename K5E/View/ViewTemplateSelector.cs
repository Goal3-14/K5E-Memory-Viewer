namespace K5E.View
{
    using K5E.Source.DTMEditor;
    using K5E.Source.FlagRecorder;
    using K5E.Source.HeapVisualizer;
    using K5E.Source.Output;
    using K5E.Source.ProcessSelector;
    using K5E.Source.PropertyViewer;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Provides the template required to view a pane.
    /// </summary>
    public class ViewTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// The template for the Process Selector.
        /// </summary>
        private DataTemplate processSelectorViewTemplate;

        /// <summary>
        /// The template for the Output.
        /// </summary>
        private DataTemplate outputViewTemplate;

        /// <summary>
        /// The template for the Property Viewer.
        /// </summary>
        private DataTemplate propertyViewerViewTemplate;

        /// <summary>
        /// The template for the Heap Visualizer.
        /// </summary>
        private DataTemplate heapVisualizerViewTemplate;

        /// <summary>
        /// The template for the Flag Recorder
        /// </summary>
        private DataTemplate flagRecorderViewTemplate;

        /// <summary>
        /// The template for the DTM Editor.
        /// </summary>
        private DataTemplate dtmEditorViewTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTemplateSelector" /> class.
        /// </summary>
        public ViewTemplateSelector()
        {
            this.DataTemplates = new Dictionary<Type, DataTemplate>();
        }

        /// <summary>
        /// Gets or sets the template for the Data Template Error display.
        /// </summary>
        public DataTemplate DataTemplateErrorViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the mapping for all data templates.
        /// </summary>
        protected Dictionary<Type, DataTemplate> DataTemplates { get; set; }

        /// <summary>
        /// Returns the required template to display the given view model.
        /// </summary>
        /// <param name="item">The view model.</param>
        /// <param name="container">The dependency object.</param>
        /// <returns>The template associated with the provided view model.</returns>
        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            if (item is ContentPresenter)
            {
                Object content = (item as ContentPresenter).Content;

                if (content != null && this.DataTemplates.ContainsKey(content.GetType()))
                {
                    return this.DataTemplates[content.GetType()];
                }
            }

            if (this.DataTemplates.ContainsKey(item.GetType()))
            {
                return this.DataTemplates[item.GetType()];
            }

            return this.DataTemplateErrorViewTemplate;
        }

        /// <summary>
        /// Gets or sets the template for the Process Selector.
        /// </summary>
        public DataTemplate ProcessSelectorViewTemplate
        {
            get
            {
                return this.processSelectorViewTemplate;
            }

            set
            {
                this.processSelectorViewTemplate = value;
                this.DataTemplates[typeof(ProcessSelectorViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Output.
        /// </summary>
        public DataTemplate OutputViewTemplate
        {
            get
            {
                return this.outputViewTemplate;
            }

            set
            {
                this.outputViewTemplate = value;
                this.DataTemplates[typeof(OutputViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Property Viewer.
        /// </summary>
        public DataTemplate PropertyViewerViewTemplate
        {
            get
            {
                return this.propertyViewerViewTemplate;
            }

            set
            {
                this.propertyViewerViewTemplate = value;
                this.DataTemplates[typeof(PropertyViewerViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Heap Visualizer.
        /// </summary>
        public DataTemplate HeapVisualizerViewTemplate
        {
            get
            {
                return this.heapVisualizerViewTemplate;
            }

            set
            {
                this.heapVisualizerViewTemplate = value;
                this.DataTemplates[typeof(HeapVisualizerViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Flag Recorder
        /// </summary>
        public DataTemplate FlagRecorderViewTemplate
        {
            get
            {
                return this.flagRecorderViewTemplate;
            }

            set
            {
                this.flagRecorderViewTemplate = value;
                this.DataTemplates[typeof(FlagRecorderViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the DTM Editor.
        /// </summary>
        public DataTemplate DTMEditorViewTemplate
        {
            get
            {
                return this.dtmEditorViewTemplate;
            }

            set
            {
                this.dtmEditorViewTemplate = value;
                this.DataTemplates[typeof(DTMEditorViewModel)] = value;
            }
        }
    }
    //// End class
}
//// End namespace