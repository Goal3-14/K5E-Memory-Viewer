namespace K5E.Source.HeapVisualizer
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    public class HeapViewInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        private WriteableBitmap heapBitmap;

        /// <summary>
        /// </summary>
        private Byte[] heapBitmapBuffer;

        /// <summary>
        /// </summary>
        private UInt32 heapTotalSize;

        /// <summary>
        /// </summary>
        private UInt32 heapUsedSize;

        /// <summary>
        /// </summary>
        private UInt32 heapTotalBlocks;

        /// <summary>
        /// </summary>
        private UInt32 heapUsedBlocks;

        /// <summary>
        /// </summary>
        private UInt32 heapMountBlockStart;

        /// <summary>
        /// </summary>
        private UInt32 heapMountBlockEnd;

        /// <summary>
        /// </summary>
        private string heapMountStatus;

        /// <summary>
        /// </summary>
        private UInt32 heapBaseAddress;

        /// <summary>
        /// </summary>
        private string heapHash;

        /// <summary>
        /// 
        /// </summary>
        public WriteableBitmap HeapBitmap
        {
            get
            {
                return this.heapBitmap;
            }

            set
            {
                this.heapBitmap = value;
                this.RaisePropertyChanged(nameof(this.HeapBitmap));
            }
        }

        /// <summary>
        /// </summary>
        public Byte[] HeapBitmapBuffer
        {
            get
            {
                return this.heapBitmapBuffer;
            }

            set
            {
                this.heapBitmapBuffer = value;
                this.RaisePropertyChanged(nameof(this.HeapBitmapBuffer));
            }
        }

        /// <summary>
        /// </summary>
        public UInt32 HeapTotalSize
        {
            get
            {
                return this.heapTotalSize;
            }

            set
            {
                this.heapTotalSize = value;
                this.RaisePropertyChanged(nameof(this.HeapTotalSize));
            }
        }

        /// <summary>
        /// </summary>
        public UInt32 HeapUsedSize
        {
            get
            {
                return this.heapUsedSize;
            }

            set
            {
                this.heapUsedSize = value;
                this.RaisePropertyChanged(nameof(this.HeapUsedSize));
            }
        }

        /// <summary>
        /// </summary>
        public UInt32 HeapTotalBlocks
        {
            get
            {
                return this.heapTotalBlocks;
            }

            set
            {
                this.heapTotalBlocks = value;
                this.RaisePropertyChanged(nameof(this.HeapTotalBlocks));
            }
        }

        /// <summary>
        /// </summary>
        public UInt32 HeapUsedBlocks
        {
            get
            {
                return this.heapUsedBlocks;
            }

            set
            {
                this.heapUsedBlocks = value;
                this.RaisePropertyChanged(nameof(this.HeapUsedBlocks));
            }
        }

        /// <summary>
        /// </summary>
        public UInt32 HeapMountBlockStart
        {
            get
            {
                return this.heapMountBlockStart;
            }

            set
            {
                this.heapMountBlockStart = value;
                this.RaisePropertyChanged(nameof(this.HeapMountBlockStart));
            }
        }

        /// <summary>
        /// </summary>
        public UInt32 HeapMountBlockEnd
        {
            get
            {
                return this.heapMountBlockEnd;
            }

            set
            {
                this.heapMountBlockEnd = value;
                this.RaisePropertyChanged(nameof(this.HeapMountBlockEnd));
            }
        }

        /// <summary>
        /// </summary>
        public string HeapMountStatus
        {
            get
            {
                return this.heapMountStatus;
            }

            set
            {
                this.heapMountStatus = value;
                this.RaisePropertyChanged(nameof(this.HeapMountStatus));
                this.RaisePropertyChanged(nameof(this.HeapMountStatusColor));
            }
        }

        /// <summary>
        /// </summary>
        public SolidColorBrush HeapMountStatusColor
        {
            get
            {
                return this.HeapMountStatus == "AVAILABLE" ? new SolidColorBrush(Colors.LawnGreen) : new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// </summary>
        public UInt32 HeapBaseAddress
        {
            get
            {
                return this.heapBaseAddress;
            }

            set
            {
                this.heapBaseAddress = value;
                this.RaisePropertyChanged(nameof(this.HeapBaseAddress));
            }
        }

        /// <summary>
        /// </summary>
        public string HeapHash
        {
            get
            {
                return this.heapHash;
            }

            set
            {
                this.heapHash = value;
                this.RaisePropertyChanged(nameof(this.HeapHash));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);

            if (dispatcher != null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }));
            }
        }
    }
}
