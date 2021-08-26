namespace Squalr.View
{
    using Source.Main;
    using Squalr.Source.Controls;
    using Squalr.Source.Docking;
    using Squalr.Source.HeapVisualizer;
    using Squalr.Source.Output;
    using Squalr.Source.ProcessSelector;
    using Squalr.Source.Tasks;

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
        /// Gets the Manual Scanner view model.
        /// </summary>
        public HeapVisualizerViewModel HeapVisualizerViewModel
        {
            get
            {
                return HeapVisualizerViewModel.GetInstance();
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