namespace K5E.Source.FlagRecorder
{
    using K5E.Engine.Common;
    using K5E.Engine.Common.Logging;
    using K5E.Engine.Memory;
    using K5E.Source.Docking;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// View model for the Flag Recorder.
    /// </summary>
    public class FlagRecorderViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="FlagRecorderViewModel" /> class.
        /// </summary>
        private static FlagRecorderViewModel FlagRecorderViewModelInstance = new FlagRecorderViewModel();

        /// <summary>
        /// Prevents a default instance of the <see cref="FlagRecorderViewModel" /> class from being created.
        /// </summary>
        private FlagRecorderViewModel() : base("Flag Recorder")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);

            Application.Current.Exit += this.OnAppExit;

            // this.RunUpdateLoop();
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            this.CanUpdate = false;
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="FlagRecorderViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static FlagRecorderViewModel GetInstance()
        {
            return FlagRecorderViewModel.FlagRecorderViewModelInstance;
        }

        bool CanUpdate = false;

        Byte[] GameBits;
        Byte[] PuzzleBytes;

        /// <summary>
        /// Blacklist for gamebits that change extremely frequently (ie timers)
        /// </summary>
        HashSet<int> BlackList = new HashSet<int>
        {
            // Health
            12,

            // Mana
            16,

            // Zoom / Aim
            261,
            262,

            // Timer
            1376, 1377, 1378, 1379,
            
            // Timer
            1704, 1705, 1706, 1707
        };

        string EventLog = Path.Join(AppDomain.CurrentDomain.BaseDirectory, string.Format("EventLog_{0}.csv", StaticRandom.Next(0, 255)));
        string PuzzleLog = Path.Join(AppDomain.CurrentDomain.BaseDirectory, string.Format("PuzzleLog_{0}.csv", StaticRandom.Next(0, 255)));

        /// <summary>
        /// Begin the update loop for visualizing the heap.
        /// </summary>
        private void RunUpdateLoop()
        {
            this.CanUpdate = true;

            if (!File.Exists(EventLog))
            {
                File.Create(EventLog);
            }

            if (!File.Exists(PuzzleLog))
            {
                File.Create(PuzzleLog);
            }

            Task.Run(async () =>
            {
                while (this.CanUpdate)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // this.RecordGameFlagChanges();
                            this.RecordPuzzleChanges();
                        });
                        await Task.Delay(1);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error updating the Flag Recorder", ex);
                    }
                }
            });
        }

        void RecordGameFlagChanges()
        {
            using (StreamWriter eventWriter = File.AppendText(EventLog))
            {
                bool success = false;
                Byte[] gameBits = MemoryReader.Instance.ReadBytes(
                    SessionManager.Session.OpenedProcess,
                    MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, 0x803A32A8, EmulatorType.Dolphin),
                    1772,
                    out success);

                if (!success)
                {
                    return;
                }

                UInt32 frame = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<UInt32>(
                    SessionManager.Session.OpenedProcess,
                    MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, 0x803DCB1C, EmulatorType.Dolphin),
                    out success));

                if (success && gameBits != null)
                {
                    bool hasDiff = false;

                    for (int index = 0; index < gameBits.Length; index++)
                    {
                        if (!BlackList.Contains(index) && (this.GameBits == null || gameBits[index] != this.GameBits[index]))
                        {
                            // Difference detected (or first run). Fire an event with the value / frame for each changed gamebit.
                            eventWriter.WriteLine(String.Format("{0},{1},{2}", index, gameBits[index], frame));
                            hasDiff = true;
                        }
                    }

                    if (hasDiff)
                    {
                        eventWriter.Flush();
                    }

                    this.GameBits = gameBits;
                }
            }
        }

        void RecordPuzzleChanges()
        {
            using (StreamWriter puzzleWriter = File.AppendText(PuzzleLog))
            {
                bool success = false;
                Byte[] puzzleBytes = MemoryReader.Instance.ReadBytes(
                    SessionManager.Session.OpenedProcess,
                    MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, 0x8032A489, EmulatorType.Dolphin), 9 * 2, out success);

                if (success && puzzleBytes != null)
                {
                    if (puzzleBytes == null || this.PuzzleBytes == null || !puzzleBytes.SequenceEqual(this.PuzzleBytes))
                    {
                        puzzleWriter.WriteLine(String.Format("{0},{1},{2},{3},{4},{5}", puzzleBytes[0], puzzleBytes[2], puzzleBytes[4],
                            puzzleBytes[6], puzzleBytes[8], puzzleBytes[10],
                            puzzleBytes[12], puzzleBytes[14], puzzleBytes[16]));
                        puzzleWriter.Flush();

                        this.PuzzleBytes = puzzleBytes;
                    }
                }
            }
        }
    }
    //// End class
}
//// End namespace