namespace Squalr.Source.HeapVisualizer
{
    using Squalr.Engine.Common;
    using Squalr.Engine.Common.Logging;
    using Squalr.Engine.Memory;
    using Squalr.Source.Docking;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// View model for the Manual Scanner.
    /// </summary>
    public class HeapVisualizerViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="HeapVisualizerViewModel" /> class.
        /// </summary>
        private static Lazy<HeapVisualizerViewModel> heapVisualizerViewModelInstance = new Lazy<HeapVisualizerViewModel>(
                () => { return new HeapVisualizerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        static readonly List<UInt32> HeapAddresses = new List<UInt32> { 0x80526020, 0x8112FF80, 0x812EFFA0, 0x8138E1E0, 0x81800000 /* End address */ };
        static readonly Int32 HeapCount = 4;
        static readonly UInt32 HeapTableAddress = 0x80340698;
        static readonly Int32 HeapTableEntrySize = 20;
        static readonly Int32 HeapBlockSize = 28;

        static readonly UInt32 BikeAddress = 0x81174BC0;

        static readonly Int32 HeapImageWidth = 65536;
        static readonly Int32 HeapImageHeight = 1;
        static readonly Int32 DPI = 72;

        /// <summary>
        /// Prevents a default instance of the <see cref="HeapVisualizerViewModel" /> class from being created.
        /// </summary>
        private HeapVisualizerViewModel() : base("Heap Visualizer")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);

            this.HeapBitmaps = new List<WriteableBitmap>();
            this.HeapBitmapBuffers = new List<Byte[]>();

            try
            {
                for (Int32 index = 0; index < HeapCount; index++)
                {
                    WriteableBitmap HeapBitmap = new WriteableBitmap(HeapImageWidth, HeapImageHeight, DPI, DPI, PixelFormats.Bgr24, null);
                    Byte[] HeapBuffer = new Byte[HeapBitmap.BackBufferStride * HeapImageHeight];

                    this.HeapBitmaps.Add(HeapBitmap);
                    this.HeapBitmapBuffers.Add(HeapBuffer);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error initializing heap bitmap", ex);
            }

            this.RunUpdateLoop();
        }

        /// <summary>
        /// Gets the list of visualization bitmaps.
        /// </summary>
        public List<WriteableBitmap> HeapBitmaps { get; private set; }

        /// <summary>
        /// Gets the list of buffers used to update the visualization bitmaps.
        /// </summary>
        public List<Byte[]> HeapBitmapBuffers { get; private set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="HeapVisualizerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static HeapVisualizerViewModel GetInstance()
        {
            return HeapVisualizerViewModel.heapVisualizerViewModelInstance.Value;
        }

        /// <summary>
        /// Begin the update loop for visualizing the heap.
        /// </summary>
        private void RunUpdateLoop()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                            {
                                Array.Clear(this.HeapBitmapBuffers[heapIndex], 0, this.HeapBitmapBuffers[heapIndex].Length);
                            }

                            bool success = false;

                            Byte[] heapStruct = MemoryReader.Instance.ReadBytes(
                                SessionManager.Session.OpenedProcess,
                                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, HeapTableAddress, EmulatorType.Dolphin),
                                HeapCount * HeapTableEntrySize,
                                out success);

                            if (!success)
                            {
                                return;
                            }

                            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                            {
                                Int32 baseIndex = heapIndex * HeapTableEntrySize;
                                Int32 bytesPerPixel = this.HeapBitmaps[heapIndex].Format.BitsPerPixel / 8;
                                Int32 heapSize = (Int32)(HeapAddresses[heapIndex + 1] - HeapAddresses[heapIndex]);

                                UInt32 totalSize = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(heapStruct, baseIndex));
                                UInt32 usedSize = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(heapStruct, baseIndex + 4));
                                UInt32 totalBlocks = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(heapStruct, baseIndex + 8));
                                UInt32 usedBlocks = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(heapStruct, baseIndex + 12));
                                UInt32 blockBaseAddress = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(heapStruct, baseIndex + 16));

                                for (int blockIndex = 0; blockIndex < usedBlocks; blockIndex++)
                                {
                                    UInt32 nextBlockAddress = blockBaseAddress + (UInt32)(blockIndex * HeapBlockSize);

                                    Byte[] blockStruct = MemoryReader.Instance.ReadBytes(
                                        SessionManager.Session.OpenedProcess,
                                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, nextBlockAddress, EmulatorType.Dolphin),
                                        HeapBlockSize,
                                        out success);

                                    if (!success)
                                    {
                                        continue;
                                    }

                                    UInt32 address = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(blockStruct, 0));
                                    UInt32 size = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(blockStruct, 4));
                                    UInt32 tag = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(blockStruct, 16));
                                    UInt32 id = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(blockStruct, 24));

                                    Int32 offset = (Int32)(address - HeapAddresses[heapIndex]);

                                    if (offset < 0 || address > HeapAddresses[heapIndex + 1])
                                    {
                                        throw new Exception("Address out of range!");
                                    }

                                    Color color = Color.FromArgb(255, 0, 0, 0);
                                    
                                    switch(tag)
                                    {
                                        case 0x5: color = Colors.Cyan; break;       // Map blocks
                                        case 0x6: color = Colors.Green; break;      // Texture
                                        case 0x9: color = Colors.Yellow; break;     // Model data
                                        case 0xA: color = Colors.Blue; break;       // Models
                                        case 0xB: color = Colors.RoyalBlue; break;  // Audio?
                                        case 0xE: color = Colors.Red; break;        // Objects
                                        case 0x10: color = Colors.Teal; break;      // VOX
                                        case 0x11: color = Colors.DarkViolet; break;    // Stack data type
                                        case 0x17: color = Colors.Purple; break;        // Texture points
                                        case 0x18: color = Colors.Magenta; break;       // Vec3 array
                                        case 0x1A: color = Colors.LimeGreen; break;     // Model struct
                                        case 0xFF: color = Colors.Gray; break;          // Byte buffer
                                        case 0x7D7D7D7D: color = Colors.DarkGray; break;    // Data file
                                        case 0x7F7F7F7F: color = Colors.LightGray; break;   // Compressed file
                                        case 0x8002BE20: color = Colors.IndianRed; break;   // Object definition
                                        case 0x8002D91C: color = Colors.DarkRed; break;     // Object instance
                                        case 0xFFFF00FF: color = Colors.HotPink; break;     // Intersect point / savegame
                                        default: color = Colors.Brown; break;           
                                    }

                                    if (address == BikeAddress)
                                    {
                                        color = Colors.HotPink;
                                    }

                                    Int32 pixelStart = (Int32)((double)offset * (double)HeapImageWidth / (double)heapSize);
                                    Int32 pixelEnd = (Int32)((double)(offset + size) * (double)HeapImageWidth / (double)heapSize);

                                    for (Int32 pixelIndex = pixelStart; pixelIndex < pixelEnd; pixelIndex++)
                                    {
                                        if (pixelIndex >= HeapImageWidth)
                                        {
                                            throw new Exception("Address out of bitmap range!");
                                        }

                                        this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel] = color.B;
                                        this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 1] = color.G;
                                        this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 2] = color.R;
                                    }
                                }

                                this.HeapBitmaps[heapIndex].WritePixels(
                                    new Int32Rect(0, 0, HeapImageWidth, HeapImageHeight),
                                    this.HeapBitmapBuffers[heapIndex],
                                    this.HeapBitmaps[heapIndex].PixelWidth * bytesPerPixel,
                                    0
                                );
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
    }
    //// End class
}
//// End namespace