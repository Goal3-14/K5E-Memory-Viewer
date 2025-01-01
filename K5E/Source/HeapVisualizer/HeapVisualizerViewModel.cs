namespace K5E.Source.HeapVisualizer
{
    using K5E.Engine.Common;
    using K5E.Engine.Common.DataStructures;
    using K5E.Engine.Common.Logging;
    using K5E.Engine.Memory;
    using K5E.Source.Docking;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;


    using System.Diagnostics;



    //Shared Memory stuff
    using System.IO.MemoryMappedFiles;
    using System.Text;
    using System.Threading;
    using System.IO;
    using System.Collections;

    /// <summary>
    /// View model for the Heap Visualizer.
    /// </summary>
    public class HeapVisualizerViewModel : ToolViewModel
    {

        const string mapName = "MySharedMemory";
        const string FmapName = "FrameMemory";
        const string FoxName = "FoxMemory";
        const string BikeEswName = "BikeESWMemory";

        const int size = 32; // Size for a 32-digit number (or 34 with null terminator)
        const int Fsize = 8;
        UInt32 BufferPointer;
        int Cycle = 0;

        static readonly List<UInt32> HeapAddresses = new List<UInt32> { 0x80526020, 0x8112FF80, 0x812EFFA0, 0x8138E1E0, 0x81800000 /* End address */ };
        static readonly Int32 HeapCount = 4;
        static readonly UInt32 HeapTableAddress = 0x80340698;

        static readonly UInt32 FoxPointerAddress = 0x803428F8;
        static readonly UInt32 FoxXOffset = 0xC;
        static readonly UInt32 FoxYOffset = 0x10;
        static readonly UInt32 FoxZOffset = 0x14;

        static readonly UInt32 MountPointerAddress = 0x803428F8;
        static readonly UInt32 MountOffset = 0x908;
        static readonly UInt32 BikeXOffset = 0x18;
        static readonly UInt32 BikeYOffset = 0x1C;
        static readonly UInt32 BikeZOffset = 0x20;
        static readonly Int32 BikeSize = 3104;

        static readonly UInt32 ESWPointerAddress = 0x803DD49C;
        static readonly UInt32 ESWXOffset = 0x694;
        static readonly UInt32 ESWYOffset = 0x698;
        static readonly UInt32 ESWZOffset = 0x69C;

        static readonly Int32 HeapImageWidth = 4096;
        static readonly Int32 HeapImageHeight = 1;
        static readonly Int32 DPI = 72;

        UInt32 Buffer;

        /// <summary>
        /// Singleton instance of the <see cref="HeapVisualizerViewModel" /> class.
        /// </summary>
        private static HeapVisualizerViewModel heapVisualizerViewModelInstance = new HeapVisualizerViewModel();

        /// <summary>
        /// Prevents a default instance of the <see cref="HeapVisualizerViewModel" /> class from being created.
        /// </summary>
        private HeapVisualizerViewModel() : base("Heap Visualizer")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);

            this.HeapViews = new FullyObservableCollection<HeapViewInfo>();

            try
            {
                for (Int32 index = 0; index < HeapCount; index++)
                {
                    this.HeapViews.Add(new HeapViewInfo());
                    this.HeapViews[index].HeapBitmap = new WriteableBitmap(HeapImageWidth, HeapImageHeight, DPI, DPI, PixelFormats.Bgr24, null);
                    this.HeapViews[index].HeapBitmapBuffer = new Byte[this.HeapViews[index].HeapBitmap.BackBufferStride * HeapImageHeight];
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error initializing heap bitmap", ex);
            }

            Application.Current.Exit += this.OnAppExit;

            this.RunUpdateLoop();
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            this.CanUpdate = false;
        }

        /// <summary>
        /// Gets the list of visualization bitmaps.
        /// </summary>
        public FullyObservableCollection<HeapViewInfo> HeapViews { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the heap visualizer update loop can run.
        /// </summary>
        private bool CanUpdate { get; set; }

        private MD5 md5 = MD5.Create();

        /// <summary>
        /// Gets a singleton instance of the <see cref="HeapVisualizerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static HeapVisualizerViewModel GetInstance()
        {
            return HeapVisualizerViewModel.heapVisualizerViewModelInstance;
        }

        /// <summary>
        /// Begin the update loop for visualizing the heap.
        /// </summary>
        private void RunUpdateLoop()
        {
            string CurrentMem = "X";
            string MemText = "X";
            this.CanUpdate = true;

            Task.Run(async () =>
            {
                while (this.CanUpdate)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            bool success = false;
                            UInt32 mountPointer = MemoryReader.Instance.Read<UInt32>(
                                SessionManager.Session.OpenedProcess,
                                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, MountPointerAddress, EmulatorType.Dolphin),
                                out success);

                            if (success)
                            {
                                mountPointer = BinaryPrimitives.ReverseEndianness(mountPointer) + MountOffset;
                                mountPointer = MemoryReader.Instance.Read<UInt32>(
                                    SessionManager.Session.OpenedProcess,
                                    MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, mountPointer, EmulatorType.Dolphin),
                                    out success);

                                if (success)
                                {
                                    mountPointer = BinaryPrimitives.ReverseEndianness(mountPointer);

                                }
                            }

                            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                            {
                                Array.Clear(this.HeapViews[heapIndex].HeapBitmapBuffer, 0, this.HeapViews[heapIndex].HeapBitmapBuffer.Length);
                            }

                            Int32 heapStructSize = typeof(SFAHeap).StructLayoutAttribute.Size;
                            Byte[] heapArrayRaw = MemoryReader.Instance.ReadBytes(
                                SessionManager.Session.OpenedProcess,
                                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, HeapTableAddress, EmulatorType.Dolphin),
                                HeapCount * typeof(SFAHeap).StructLayoutAttribute.Size,
                                out success);

                            if (!success)
                            {
                                return;
                            }

                            SFAHeap[] heaps = new SFAHeap[4];

                            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                            {
                                Byte[] heapData = new Byte[heapStructSize];
                                Array.Copy(heapArrayRaw, heapIndex * heapStructSize, heapData, 0, heapStructSize);
                                heaps[heapIndex] = SFAHeap.FromByteArray(heapData);
                            }

                            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                            {
                                Int32 bytesPerPixel = this.HeapViews[heapIndex].HeapBitmap.Format.BitsPerPixel / 8;
                                // UInt32 heapSize = heaps[heapIndex].totalSize == 0 ? (UInt32)(heaps[heapIndex + 1].heapPtr - heaps[heapIndex].heapPtr) : heaps[heapIndex].totalSize;
                                UInt32 heapSize = HeapAddresses[heapIndex + 1] - HeapAddresses[heapIndex];

                                this.HeapViews[heapIndex].HeapTotalSize = heaps[heapIndex].totalSize;
                                this.HeapViews[heapIndex].HeapUsedSize = heaps[heapIndex].usedSize;
                                this.HeapViews[heapIndex].HeapTotalBlocks = heaps[heapIndex].totalBlocks;
                                this.HeapViews[heapIndex].HeapUsedBlocks = heaps[heapIndex].usedBlocks;
                                this.HeapViews[heapIndex].HeapBaseAddress = heaps[heapIndex].heapPtr;
                                this.HeapViews[heapIndex].HeapHash = Convert.ToHexString(md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(this.HeapViews[heapIndex].HeapBaseAddress.ToString())));

                                // Color the bike memory as flashing red. Will be overwritten if something is allocated there.
                                if (mountPointer > heaps[heapIndex].heapPtr && mountPointer < heaps[heapIndex].heapPtr + heapSize)
                                {
                                    this.ColorBikeMemory(heaps, mountPointer, heapIndex, Color.FromRgb((Byte)(DateTime.Now.Ticks % Byte.MaxValue), 0, 0));
                                }

                                Int32 heapEntrySize = typeof(SFAHeapEntry).StructLayoutAttribute.Size;

                                if (!success)
                                {
                                    continue;
                                }

                                this.HeapViews[heapIndex].HeapMountBlockStart = 0;
                                this.HeapViews[heapIndex].HeapMountBlockEnd = 0;
                                this.HeapViews[heapIndex].HeapMountStatus = "";

                                for (int blockIndex = 0; blockIndex < heaps[heapIndex].totalBlocks; blockIndex++)
                                {
                                    UInt32 nextBlockAddress = heaps[heapIndex].heapEntryPtr + (UInt32)(blockIndex * heapEntrySize);
                                    Byte[] heapEntryDataRaw = MemoryReader.Instance.ReadBytes(
                                        SessionManager.Session.OpenedProcess,
                                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, nextBlockAddress, EmulatorType.Dolphin),
                                        heapEntrySize,
                                        out success);

                                    if (!success)
                                    {
                                        continue;
                                    }

                                    SFAHeapEntry heapEntry = SFAHeapEntry.FromByteArray(heapEntryDataRaw);

                                    Int32 offset = (Int32)(heapEntry.entryPtr - heaps[heapIndex].heapPtr);

                                    this.HeapViews[heapIndex].HeapHash = Convert.ToHexString(md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(this.HeapViews[heapIndex].HeapHash + heapEntry.entryPtr.ToString())));
                                    this.HeapViews[heapIndex].HeapHash = Convert.ToHexString(md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(this.HeapViews[heapIndex].HeapHash + heapEntry.size.ToString())));













                                    //////////////////////////////////////////////////////////////////////////////////////////////////////
                                    //////////////

                                    // Get Mem Values
                                    try
                                    {

                                        if (this.HeapViews != null && this.HeapViews.Count > 1 && this.HeapViews[1] != null)
                                        {
                                            if (this.HeapViews[1].HeapHash != null)
                                            {


                                                string IncMem = this.HeapViews[1].HeapHash?.ToString();
                                                if (CurrentMem == IncMem)
                                                {
                                                    MemText = IncMem;

                                                }


                                                CurrentMem = IncMem;

                                            }


                                        }
                                        else
                                        {

                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }




                                    // Send Mem
                                    if (MemText != "X")
                                    {

                                        MemoryMappedFile mmf;

                                        try
                                        {
                                            mmf = MemoryMappedFile.OpenExisting(mapName);
                                        }
                                        catch (IOException)
                                        {
                                            mmf = MemoryMappedFile.CreateNew(mapName, size);
                                        }

                                        using (var accessor = mmf.CreateViewAccessor())
                                        {

                                            //string str = "95C99029EE2F5684DADC658F79FF51BA".PadRight(size, '\0');

                                            byte[] strBytes = Encoding.UTF8.GetBytes(MemText);
                                            accessor.WriteArray(0, strBytes, 0, strBytes.Length);

                                        }




                                    }



                                    // Get Frame
                                    string frame = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<UInt32>(
                                        SessionManager.Session.OpenedProcess,
                                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, 0x803DCB1C, EmulatorType.Dolphin),
                                        out success)).ToString().PadLeft(8, '0');



                                    // Send Frame
                                    MemoryMappedFile mmfFrame;

                                    try
                                    {
                                        mmfFrame = MemoryMappedFile.OpenExisting(FmapName);
                                    }
                                    catch (IOException)
                                    {
                                        mmfFrame = MemoryMappedFile.CreateNew(FmapName, Fsize);
                                    }

                                    using (var accessor = mmfFrame.CreateViewAccessor())
                                    {



                                        byte[] strBytes = Encoding.UTF8.GetBytes(frame);
                                        accessor.WriteArray(0, strBytes, 0, strBytes.Length);

                                    }










                                    ///////////////////
                                    ///////////////////////////////////////////////////////////////////////////////////////////////////////






                                    if (offset < 0 || heapEntry.entryPtr > heaps[heapIndex].heapPtr + heapSize)
                                    {
                                        // throw new Exception("Address out of range!");
                                        continue;
                                    }

                                    if (mountPointer >= heapEntry.entryPtr && mountPointer <= heapEntry.entryPtr + heapEntry.size)
                                    {
                                        this.HeapViews[heapIndex].HeapMountBlockStart = heapEntry.entryPtr;
                                        this.HeapViews[heapIndex].HeapMountBlockEnd = heapEntry.entryPtr + heapEntry.size;
                                        this.HeapViews[heapIndex].HeapMountStatus = heapEntry.entryType == SFAHeapEntryType.Free ? "AVAILABLE" : "OVERWRITTEN!";
                                    }

                                    Color color = Color.FromArgb(255, 0, 0, 0);

                                    if (heapEntry.entryType == SFAHeapEntryType.Free)
                                    {
                                        color = Colors.Gray;
                                    }
                                    else
                                    {
                                        switch (heapEntry.allocTag)
                                        {
                                            case SFAAllocTag.ZERO: color = Colors.LightSkyBlue; break;
                                            case SFAAllocTag.LISTS_COL: color = Colors.Orange; break;
                                            case SFAAllocTag.SCREEN_COL: color = Colors.DarkCyan; break;
                                            case SFAAllocTag.CODE_COL: color = Colors.DarkGoldenrod; break;
                                            case SFAAllocTag.DLL_COL: color = Colors.Blue; break;
                                            case SFAAllocTag.TRACK_COL: color = Colors.DarkKhaki; break;
                                            case SFAAllocTag.TEX_COL: color = Colors.DarkSeaGreen; break;
                                            case SFAAllocTag.TRACKTEX_COL: color = Colors.DarkOliveGreen; break;
                                            case SFAAllocTag.SPRITETEX_COL: color = Colors.Pink; break;
                                            case SFAAllocTag.MODELS_COL: color = Colors.DarkSalmon; break;
                                            case SFAAllocTag.ANIMS_COL: color = Colors.Red; break;
                                            case SFAAllocTag.AUDIO_COL: color = Colors.RoyalBlue; break;
                                            case SFAAllocTag.SEQ_COL: color = Colors.DarkSlateBlue; break;
                                            case SFAAllocTag.SFX_COL: color = Colors.DarkSlateGray; break;
                                            case SFAAllocTag.OBJECTS_COL: color = Colors.Green; break;
                                            case SFAAllocTag.CAM_COL: color = Colors.DarkTurquoise; break;
                                            case SFAAllocTag.VOX_COL: color = Colors.Teal; break;
                                            case SFAAllocTag.ANIMSEQ_COL: color = Colors.DarkViolet; break;
                                            case SFAAllocTag.LFX_COL: color = Colors.LightBlue; break;
                                            case SFAAllocTag.GFX_CO: color = Colors.LightCoral; break;
                                            case SFAAllocTag.EXPGFX_COL: color = Colors.LightCyan; break;
                                            case SFAAllocTag.MODGFX_COL: color = Colors.LightGoldenrodYellow; break;
                                            case SFAAllocTag.PROJGFX_COL: color = Colors.LightGreen; break;
                                            case SFAAllocTag.SKY_COL: color = Colors.LightPink; break;
                                            case SFAAllocTag.SHAD_COL: color = Colors.LightSalmon; break;
                                            case SFAAllocTag.GAME_COL: color = Colors.LightSeaGreen; break;
                                            case SFAAllocTag.TEST_COL: color = Colors.LimeGreen; break;
                                            case SFAAllocTag.BLACK: color = Colors.Black; break;
                                            case SFAAllocTag.RED: color = Colors.DarkRed; break;
                                            case SFAAllocTag.GREEN: color = Colors.DarkGreen; break;
                                            case SFAAllocTag.BLUE: color = Colors.DarkBlue; break;
                                            case SFAAllocTag.CYAN: color = Colors.Cyan; break;
                                            case SFAAllocTag.MAGENTA: color = Colors.Magenta; break;
                                            case SFAAllocTag.YELLOW: color = Colors.Yellow; break;
                                            case SFAAllocTag.WHITE: color = Colors.White; break;
                                            case SFAAllocTag.GREY: color = Colors.DarkGray; break;
                                            case SFAAllocTag.ORANGE: color = Colors.DarkOrange; break;
                                            case SFAAllocTag.OBJECTS: color = Colors.DarkMagenta; break;
                                            case SFAAllocTag.VOX: color = Colors.LightSlateGray; break;
                                            case SFAAllocTag.ANIMS: color = Colors.LightSteelBlue; break;
                                            case SFAAllocTag.TRACK: color = Colors.LightYellow; break;
                                            case SFAAllocTag.MODELS: color = Colors.Lime; break;
                                            case SFAAllocTag.GAME: color = Colors.LimeGreen; break;
                                            case SFAAllocTag.ANIMSEQ: color = Colors.AliceBlue; break;
                                            case SFAAllocTag.FILE: color = Colors.BlanchedAlmond; break;
                                            case SFAAllocTag.CODE: color = Colors.IndianRed; break;
                                            case SFAAllocTag.SHAD: color = Colors.Aqua; break;
                                            case SFAAllocTag.COMPRESSED_FILE: color = Colors.AntiqueWhite; break;
                                            case SFAAllocTag.CAM: color = Colors.Azure; break;
                                            case SFAAllocTag.DLL: color = Colors.SandyBrown; break;
                                            case SFAAllocTag.LISTS: color = Colors.SeaGreen; break;
                                            case SFAAllocTag.SFX: color = Colors.SeaShell; break;
                                            case SFAAllocTag.SEQ: color = Colors.Chocolate; break;
                                            case SFAAllocTag.AUDIO: color = Colors.SaddleBrown; break;
                                            case SFAAllocTag.SPRITETEX: color = Colors.MediumAquamarine; break;
                                            case SFAAllocTag.TRACKTEX: color = Colors.MediumBlue; break;
                                            case SFAAllocTag.TEX: color = Colors.MediumOrchid; break;
                                            case SFAAllocTag.SCREEN: color = Colors.MediumPurple; break;
                                            case SFAAllocTag.FACEFEED: color = Colors.MediumSeaGreen; break;
                                            case SFAAllocTag.SKY: color = Colors.MediumSlateBlue; break;
                                            case SFAAllocTag.PROJGFX: color = Colors.MediumSpringGreen; break;
                                            case SFAAllocTag.MODGFX: color = Colors.MediumTurquoise; break;
                                            case SFAAllocTag.EXPGFX: color = Colors.MediumVioletRed; break;
                                            case SFAAllocTag.GFX: color = Colors.Olive; break;
                                            case SFAAllocTag.LFX: color = Colors.OliveDrab; break;
                                            case SFAAllocTag.TEST: color = Colors.PeachPuff; break;
                                            case SFAAllocTag.INTERSECT_POINT: color = Colors.CornflowerBlue; break;
                                            case SFAAllocTag.SAVEGAME: color = Colors.HotPink; break;
                                            default: color = Colors.Brown; break;
                                        }
                                    }

                                    Int32 pixelStart = (Int32)((double)offset * (double)HeapImageWidth / (double)heapSize);
                                    Int32 pixelEnd = (Int32)((double)(offset + heapEntry.size) * (double)HeapImageWidth / (double)heapSize);

                                    for (Int32 pixelIndex = pixelStart; pixelIndex < pixelEnd; pixelIndex++)
                                    {
                                        if (pixelIndex >= HeapImageWidth)
                                        {
                                            // throw new Exception("Address out of bitmap range!");
                                            break;
                                        }

                                        this.HeapViews[heapIndex].HeapBitmapBuffer[pixelIndex * bytesPerPixel] = color.B;
                                        this.HeapViews[heapIndex].HeapBitmapBuffer[pixelIndex * bytesPerPixel + 1] = color.G;
                                        this.HeapViews[heapIndex].HeapBitmapBuffer[pixelIndex * bytesPerPixel + 2] = color.R;
                                    }

                                    // Recolor the memory as flashing green if it is being overlapped
                                    if (heapEntry.entryPtr <= (mountPointer + BikeSize) && mountPointer <= (heapEntry.entryPtr + heapEntry.size))
                                    {
                                        this.ColorBikeMemory(heaps, mountPointer, heapIndex, Color.FromRgb(0, (Byte)(DateTime.Now.Ticks % Byte.MaxValue), 0));
                                    }
                                }




                                







                                this.HeapViews[heapIndex].HeapBitmap.WritePixels(
                                    new Int32Rect(0, 0, HeapImageWidth, HeapImageHeight),
                                    this.HeapViews[heapIndex].HeapBitmapBuffer,
                                    this.HeapViews[heapIndex].HeapBitmap.PixelWidth * bytesPerPixel,
                                    0
                                );
                            }


                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            ////////////////
                            Cycle++;
                            //Fox
                            if (Cycle % 2 == 0)
                            {
                                UInt32 MainPointer = MemoryReader.Instance.Read<UInt32>(
                                SessionManager.Session.OpenedProcess,
                                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, FoxPointerAddress, EmulatorType.Dolphin),
                                out success);

                                // X
                                BufferPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + FoxXOffset;
                                string foxX = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<Int32>(
                                    SessionManager.Session.OpenedProcess,
                                    MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, BufferPointer, EmulatorType.Dolphin),
                                    out success)).ToString();//.PadLeft(8, '0');
                                                             //Debug.WriteLine(foxX);
                                                             //Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(foxX)), 0));

                                // Y
                                BufferPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + FoxYOffset;
                                string foxY = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<Int32>(
                                    SessionManager.Session.OpenedProcess,
                                    MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, BufferPointer, EmulatorType.Dolphin),
                                    out success)).ToString();//.PadLeft(8, '0');
                                                             //Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(foxY)), 0));

                                // Z
                                BufferPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + FoxZOffset;
                                string foxZ = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<Int32>(
                                    SessionManager.Session.OpenedProcess,
                                    MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, BufferPointer, EmulatorType.Dolphin),
                                    out success)).ToString();//.PadLeft(8, '0');
                                                             //Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(foxZ)), 0));

                                string FoxCoords = foxX +" "+ foxY +" "+ foxZ;

                                MemoryMappedFile mmfFox;

                                try
                                {
                                    mmfFox = MemoryMappedFile.OpenExisting(FoxName);
                                }
                                catch (IOException)
                                {
                                    mmfFox = MemoryMappedFile.CreateNew(FoxName, 35);
                                }

                                using (var accessor = mmfFox.CreateViewAccessor())
                                {
                                    byte[] strBytes = Encoding.UTF8.GetBytes(FoxCoords);
                                    accessor.WriteArray(0, strBytes, 0, strBytes.Length);

                                }

                                //Bike

                                if (Cycle >= 6)
                                {


                                    MainPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + MountOffset;
                                    MainPointer = MemoryReader.Instance.Read<UInt32>(
                                    SessionManager.Session.OpenedProcess,
                                    MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, MainPointer, EmulatorType.Dolphin),
                                    out success);


                                    // X
                                    BufferPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + BikeXOffset;
                                    string BikeX = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<Int32>(
                                        SessionManager.Session.OpenedProcess,
                                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, BufferPointer, EmulatorType.Dolphin),
                                        out success)).ToString();
                                    //Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(BikeX)), 0));

                                    // Y
                                    BufferPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + BikeYOffset;
                                    string BikeY = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<Int32>(
                                        SessionManager.Session.OpenedProcess,
                                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, BufferPointer, EmulatorType.Dolphin),
                                        out success)).ToString();
                                    //Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(BikeY)), 0));

                                    // Z
                                    BufferPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + BikeZOffset;
                                    string BikeZ = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<Int32>(
                                        SessionManager.Session.OpenedProcess,
                                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, BufferPointer, EmulatorType.Dolphin),
                                        out success)).ToString();
                                    //Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(BikeZ)), 0));
                                    //Debug.WriteLine("");
                                    Cycle = 0;

                                    

                                    //ESW

                                    MainPointer = MemoryReader.Instance.Read<UInt32>(
                                    SessionManager.Session.OpenedProcess,
                                    MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ESWPointerAddress, EmulatorType.Dolphin),
                                    out success);

                                    // X
                                    BufferPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + ESWXOffset;
                                    string ESWX = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<Int32>(
                                        SessionManager.Session.OpenedProcess,
                                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, BufferPointer, EmulatorType.Dolphin),
                                        out success)).ToString();
                                    //Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(ESWX)), 0));

                                    BufferPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + ESWYOffset;
                                    string ESWY = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<Int32>(
                                        SessionManager.Session.OpenedProcess,
                                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, BufferPointer, EmulatorType.Dolphin),
                                        out success)).ToString();
                                    //Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(ESWY)), 0));

                                    BufferPointer = BinaryPrimitives.ReverseEndianness(MainPointer) + ESWZOffset;
                                    string ESWZ = BinaryPrimitives.ReverseEndianness(MemoryReader.Instance.Read<Int32>(
                                        SessionManager.Session.OpenedProcess,
                                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, BufferPointer, EmulatorType.Dolphin),
                                        out success)).ToString();
                                    //Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(ESWZ)), 0));

                                    string BikeESW = BikeX + " " + BikeY + " " + BikeZ +" "+ ESWX+" "+ESWY+" "+ESWZ; //70

                                    MemoryMappedFile mmfBikeESW;

                                    try
                                    {
                                        mmfBikeESW = MemoryMappedFile.OpenExisting(BikeEswName);
                                    }
                                    catch (IOException)
                                    {
                                        mmfBikeESW = MemoryMappedFile.CreateNew(BikeEswName, 70);
                                    }

                                    using (var accessor = mmfBikeESW.CreateViewAccessor())
                                    {
                                        byte[] strBytes = Encoding.UTF8.GetBytes(BikeESW);
                                        accessor.WriteArray(0, strBytes, 0, strBytes.Length);

                                    }
                                }

                                
                                /////////////////
                                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            }
                            

                        });
                    }
                    catch(Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error updating the Heap Visualizer", ex);
                    }

                    await Task.Delay(50);
                }
            });
        }

        private void ColorBikeMemory(SFAHeap[] heaps, UInt32 mountPointer, Int32 heapIndex, Color color)
        {
            UInt32 heapSize = heaps[heapIndex].totalSize == 0 ? (UInt32)(heaps[heapIndex + 1].heapPtr - heaps[heapIndex].heapPtr) : heaps[heapIndex].totalSize;
            Int32 bytesPerPixel = this.HeapViews[heapIndex].HeapBitmap.Format.BitsPerPixel / 8;

            Int32 bikeOffset = (Int32)(mountPointer - heaps[heapIndex].heapPtr);
            Int32 bikeStart = (Int32)((double)bikeOffset * (double)HeapImageWidth / (double)heapSize);
            Int32 bikeEnd = (Int32)((double)(bikeOffset + BikeSize) * (double)HeapImageWidth / (double)heapSize);

            for (Int32 pixelIndex = bikeStart; pixelIndex < bikeEnd; pixelIndex++)
            {
                if (pixelIndex < 0 || pixelIndex >= HeapImageWidth)
                {
                    // throw new Exception("Address out of bitmap range!");
                    continue;
                }

                this.HeapViews[heapIndex].HeapBitmapBuffer[pixelIndex * bytesPerPixel] = color.B;
                this.HeapViews[heapIndex].HeapBitmapBuffer[pixelIndex * bytesPerPixel + 1] = color.G;
                this.HeapViews[heapIndex].HeapBitmapBuffer[pixelIndex * bytesPerPixel + 2] = color.R;
            }
        }
    }
    //// End class
}
//// End namespace