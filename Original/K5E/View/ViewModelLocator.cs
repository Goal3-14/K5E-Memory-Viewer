namespace K5E.View
{
    using Source.Main;
    using K5E.Source.Controls;
    using K5E.Source.Docking;
    using K5E.Source.HeapVisualizer;
    using K5E.Source.Output;
    using K5E.Source.ProcessSelector;
    using K5E.Source.Tasks;
    using K5E.Source.DTMEditor;
    using K5E.Source.PropertyViewer;
    using K5E.Source.FlagRecorder;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
        }

        /// <summary>
        /// Gets the Docking view model.
        /// </summary>
        public DockingViewModel DockingViewModel
        {
            get
            {
                return DockingViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Action Scheduler view model.
        /// </summary>
        public TaskTrackerViewModel TaskTrackerViewModel
        {
            get
            {
                return TaskTrackerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Process Selector view model.
        /// </summary>
        public ProcessSelectorViewModel ProcessSelectorViewModel
        {
            get
            {
                return ProcessSelectorViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets a Output view model.
        /// </summary>
        public OutputViewModel OutputViewModel
        {
            get
            {
                return OutputViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Main view model.
        /// </summary>
        public MainViewModel MainViewModel
        {
            get
            {
                return MainViewModel.GetInstance();
            }
        }


        /// <summary>
        /// Gets the Property Editor view model.
        /// </summary>
        public PropertyViewerViewModel PropertyViewerViewModel
        {
            get
            {
                return PropertyViewerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the DTM Editor view model.
        /// </summary>
        public DTMEditorViewModel DTMEditorViewModel
        {
            get
            {
                return DTMEditorViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Heap Visualizer view model.
        /// </summary>
        public HeapVisualizerViewModel HeapVisualizerViewModel
        {
            get
            {
                return HeapVisualizerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Flag Recorder view model.
        /// </summary>
        public FlagRecorderViewModel FlagRecorderViewModel
        {
            get
            {
                return FlagRecorderViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets a HexDec box view model.
        /// </summary>
        public HexDecBoxViewModel HexDecBoxViewModel
        {
            get
            {
                return new HexDecBoxViewModel();
            }
        }
    }
    //// End class
}
//// End namespace