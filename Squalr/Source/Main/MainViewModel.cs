﻿namespace Squalr.Source.Main
{
    using Squalr.Engine.Common.Logging;
    using Squalr.Engine.Common.OS;
    using Squalr.Source.Docking;
    using Squalr.Source.HeapVisualizer;
    using Squalr.Source.Output;
    using System;
    using System.Numerics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Main view model.
    /// </summary>
    public class MainViewModel : WindowHostViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="MainViewModel" /> class
        /// </summary>
        private static Lazy<MainViewModel> mainViewModelInstance = new Lazy<MainViewModel>(
                () => { return new MainViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="MainViewModel" /> class from being created.
        /// </summary>
        private MainViewModel() : base()
        {
            // Attach the logger view model to the engine's output
            Logger.Subscribe(OutputViewModel.GetInstance());

            if (Vectors.HasVectorSupport)
            {
                Logger.Log(LogLevel.Info, "Hardware acceleration enabled (vector size: " + Vector<Byte>.Count + ")");
            }

            Logger.Log(LogLevel.Info, "K5E started");

            HeapVisualizerViewModel.GetInstance();
        }

        /// <summary>
        /// Gets the command to open the change log.
        /// </summary>
        public ICommand DisplayChangeLogCommand { get; private set; }

        /// <summary>
        /// Default layout file for browsing cheats.
        /// </summary>
        protected override String DefaultLayoutResource { get { return "DefaultLayout.xml"; } }

        /// <summary>
        /// The save file for the docking layout.
        /// </summary>
        protected override String LayoutSaveFile { get { return "Layout.xml"; } }

        /// <summary>
        /// Gets the singleton instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        /// <returns>The singleton instance of the <see cref="MainViewModel" /> class.</returns>
        public static MainViewModel GetInstance()
        {
            return mainViewModelInstance.Value;
        }

        /// <summary>
        /// Closes the main window.
        /// </summary>
        /// <param name="window">The window to close.</param>
        protected override void Close(Window window)
        {
            base.Close(window);
        }
    }
    //// End class
}
//// End namesapce