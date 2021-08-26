namespace K5E.Engine
{
    using K5E.Engine.Processes;
    using K5E.Engine.Scanning.Snapshots;
    using System.Diagnostics;

    /// <summary>
    /// Contains session information, including the target process in addition to snapshot history.
    /// </summary>
    public class Session : ProcessSession
    {
        public Session(Process processToOpen) : base(processToOpen)
        {
            this.SnapshotManager = new SnapshotManager();
        }

        public SnapshotManager SnapshotManager { get; private set; }
    }
    //// End class
}
//// End namespace
