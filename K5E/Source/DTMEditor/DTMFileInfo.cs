namespace K5E.Source.DTMEditor
{
    using K5E.Engine.Common.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class DTMFileInfo
    {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		public DTMFileInfo(string filePath)
		{
			this.FilePath = filePath;
			this.Header = new byte[256];

			if (!File.Exists(this.FilePath))
			{
				throw new FileNotFoundException("Specified file does not exist.", this.FilePath);
			}

			FrameInfo = new List<DTMFrameInfo>();

			using (BinaryReader reader = new BinaryReader(File.OpenRead(this.FilePath)))
			{
				if (reader.BaseStream.Length < 256)
				{
					throw new Exception("Invalid file: Header too small. Should be 256 bytes but was " + reader.BaseStream.Length + " bytes.");
				}

				this.Header = reader.ReadBytes(256);

				// Jump to controller data and read it.
				reader.BaseStream.Position = 0x100;

				while (reader.BaseStream.Position != reader.BaseStream.Length)
				{
					FrameInfo.Add(new DTMFrameInfo(this, reader.ReadUInt64()));
				}
			}
		}

		public string FilePath { get; private set; }

		/// <summary>
		/// Raw representation of the controller data.
		/// </summary>
		public byte[] Header { get; private set; }

		/// <summary>
		/// Controller data. Each entry represents the controller data
		/// that is used within a certain frame.
		/// </summary>
		public List<DTMFrameInfo> FrameInfo { get; private set; }

		/// <summary>
		/// ID of the game that this movie was made for.
		/// </summary>
		public byte[] DTMIdentifier
		{
			get
			{
				return this.Header?.SubArray(0, 4);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 0, value.Length);
			}
		}

		/// <summary>
		/// ID of the game that this movie was made for.
		/// </summary>
		public byte[] GameID
        {
			get
            {
				return this.Header?.SubArray(4, 6);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 4, value.Length);
			}
		}

		/// <summary>
		/// Whether or not this movie file is for a Wii game.
		/// </summary>
		public bool IsWiiGame
		{
			get
			{
				return Convert.ToBoolean(this.Header[10]);
			}
			set
			{
				this.Header[10] = Convert.ToByte(value);
			}
		}

		/// <summary>
		/// Number of connected controllers (1-4)
		/// </summary>
		public byte ConnectedControllers
		{
			get
			{
				return this.Header[11];
			}
			set
			{
				this.Header[11] = value;
			}
		}

		/// <summary>
		/// false indicates that the recording started from bootup, true for savestate
		/// </summary>
		public bool IsFromSaveState
		{
			get
			{
				return Convert.ToBoolean(this.Header[12]);
			}
			set
			{
				this.Header[12] = Convert.ToByte(value);
			}
		}

		/// <summary>
		/// Number of frames in the recording.
		/// </summary>
		public ulong FrameCount
		{
			get
			{
				return Convert.ToUInt64(BitConverter.ToUInt64(this.Header, 13));
			}
			set
			{
				Array.Copy(BitConverter.GetBytes(value), 0, this.Header, 13, sizeof(ulong));
			}
		}

		/// <summary>
		/// Number of input frames in the recording.
		/// </summary>
		public ulong InputFrameCount
		{
			get
			{
				return Convert.ToUInt64(BitConverter.ToUInt64(this.Header, 21));
			}
			set
			{
				Array.Copy(BitConverter.GetBytes(value), 0, this.Header, 21, sizeof(ulong));
			}
		}

		/// <summary>
		/// Number of lag frames in the recording.
		/// </summary>
		public ulong LagFrameCount
		{
			get
			{
				return Convert.ToUInt64(BitConverter.ToUInt64(this.Header, 29));
			}
			set
			{
				Array.Copy(BitConverter.GetBytes(value), 0, this.Header, 29, sizeof(ulong));
			}
		}

		/// <summary>
		/// (not implemented) A Unique ID comprised of: md5(time + Game ID)
		/// </summary>
		public ulong UniqueID
		{
			get
			{
				return Convert.ToUInt64(BitConverter.ToUInt64(this.Header, 37));
			}
			set
			{
				Array.Copy(BitConverter.GetBytes(value), 0, this.Header, 37, sizeof(ulong));
			}
		}

		/// <summary>
		/// Number of rerecords/'cuts' of this TAS
		/// </summary>
		public uint NumRerecords
		{
			get
			{
				return Convert.ToUInt32(BitConverter.ToUInt64(this.Header, 45));
			}
			set
			{
				Array.Copy(BitConverter.GetBytes(value), 0, this.Header, 45, sizeof(uint));
			}
		}

		/// <summary>
		/// Author's name
		/// </summary>
		public byte[] Author
		{
			get
			{
				return this.Header?.SubArray(49, 32);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 49, 32);
			}
		}

		/// <summary>
		/// Name of the video backend used.
		/// </summary>
		public byte[] VideoBackEnd
		{
			get
			{
				return this.Header?.SubArray(81, 16);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 81, 16);
			}
		}

		/// <summary>
		/// Name of the audio emulator used.
		/// </summary>
		public byte[] AudioEmulator
		{
			get
			{
				return this.Header?.SubArray(97, 16);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 97, 16);
			}
		}

		/// <summary>
		/// MD5 of the game ISO.
		/// </summary>
		public byte[] MD5
		{
			get
			{
				return this.Header?.SubArray(113, 16);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 113, 16);
			}
		}

		/// <summary>
		/// Seconds since 1970 that recording started (used for RTC).
		/// </summary>
		public ulong RecordingStartTime
		{
			get
			{
				return Convert.ToUInt64(BitConverter.ToUInt64(this.Header, 129));
			}
			set
			{
				Array.Copy(BitConverter.GetBytes(value), 0, this.Header, 129, sizeof(ulong));
			}
		}

		/// <summary>
		/// Whether or not certain settings are loaded.
		/// </summary>
		/// <remarks>
		/// All properties following this one are loaded during startup if true.
		/// </remarks>
		public bool IsSavedConfig
		{
			get
			{
				return Convert.ToBoolean(this.Header[137]);
			}
			set
			{
				this.Header[137] = Convert.ToByte(value);
			}
		}

		public bool UsingIdleSkip
		{
			get
			{
				return Convert.ToBoolean(this.Header[138]);
			}
			set
			{
				this.Header[138] = Convert.ToByte(value);
			}
		}

		public bool UsingDualCore
		{
			get
			{
				return Convert.ToBoolean(this.Header[139]);
			}
			set
			{
				this.Header[139] = Convert.ToByte(value);
			}
		}

		public bool UsingProgressiveScan
		{
			get
			{
				return Convert.ToBoolean(this.Header[140]);
			}
			set
			{
				this.Header[140] = Convert.ToByte(value);
			}
		}

		public bool UsingHLEDSP
		{
			get
			{
				return Convert.ToBoolean(this.Header[141]);
			}
			set
			{
				this.Header[141] = Convert.ToByte(value);
			}
		}

		public bool UsingFastDiscSpeed
		{
			get
			{
				return Convert.ToBoolean(this.Header[142]);
			}
			set
			{
				this.Header[142] = Convert.ToByte(value);
			}
		}

		public byte CPUCore
		{
			get
			{
				return this.Header[143];
			}
			set
			{
				this.Header[143] = value;
			}
		}

		public bool IsEFBAccessEnabled
		{
			get
			{
				return Convert.ToBoolean(this.Header[144]);
			}
			set
			{
				this.Header[144] = Convert.ToByte(value);
			}
		}

		public bool IsEFBCopiesEnabled
		{
			get
			{
				return Convert.ToBoolean(this.Header[145]);
			}
			set
			{
				this.Header[145] = Convert.ToByte(value);
			}
		}

		public bool UsingEFBToTexture
		{
			get
			{
				return Convert.ToBoolean(this.Header[146]);
			}
			set
			{
				this.Header[146] = Convert.ToByte(value);
			}
		}

		public bool IsEFBCopyCacheEnabled
		{
			get
			{
				return Convert.ToBoolean(this.Header[147]);
			}
			set
			{
				this.Header[147] = Convert.ToByte(value);
			}
		}

		public bool IsEmulatingEFBFormatChanges
		{
			get
			{
				return Convert.ToBoolean(this.Header[148]);
			}
			set
			{
				this.Header[148] = Convert.ToByte(value);
			}
		}

		public bool UsingXFB
		{
			get
			{
				return Convert.ToBoolean(this.Header[149]);
			}
			set
			{
				this.Header[149] = Convert.ToByte(value);
			}
		}

		public bool UsingRealXFB
		{
			get
			{
				return Convert.ToBoolean(this.Header[150]);
			}
			set
			{
				this.Header[150] = Convert.ToByte(value);
			}
		}

		public bool UsingMemoryCard
		{
			get
			{
				return Convert.ToBoolean(this.Header[151]);
			}
			set
			{
				this.Header[151] = Convert.ToByte(value);
			}
		}

		public bool UsingClearSaves
		{
			get
			{
				return Convert.ToBoolean(this.Header[152]);
			}
			set
			{
				this.Header[152] = Convert.ToByte(value);
			}
		}

		public byte NumBongos
		{
			get
			{
				return this.Header[153];
			}
			set
			{
				this.Header[153] = value;
			}
		}

		public bool SyncGPU
		{
			get
			{
				return Convert.ToBoolean(this.Header[154]);
			}
			set
			{
				this.Header[154] = Convert.ToByte(value);
			}
		}

		public bool UsingNetplay
		{
			get
			{
				return Convert.ToBoolean(this.Header[155]);
			}
			set
			{
				this.Header[155] = Convert.ToByte(value);
			}
		}

		public bool PAL60
		{
			get
			{
				return Convert.ToBoolean(this.Header[156]);
			}
			set
			{
				this.Header[156] = Convert.ToByte(value);
			}
		}

		public byte[] Misc1
		{
			get
			{
				return this.Header?.SubArray(157, 12);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 157, 12);
			}
		}

		public byte[] SecondDiscName
		{
			get
			{
				return this.Header?.SubArray(169, 40);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 169, 40);
			}
		}

		public byte[] GitRevision
		{
			get
			{
				return this.Header?.SubArray(209, 20);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 209, 20);
			}
		}

		public uint DSPIROMHash
		{
			get
			{
				return Convert.ToUInt32(BitConverter.ToUInt32(this.Header, 229));
			}
			set
			{
				Array.Copy(BitConverter.GetBytes(value), 0, this.Header, 229, sizeof(uint));
			}
		}

		public uint DSPCoefHash
		{
			get
			{
				return Convert.ToUInt32(BitConverter.ToUInt32(this.Header, 233));
			}
			set
			{
				Array.Copy(BitConverter.GetBytes(value), 0, this.Header, 233, sizeof(uint));
			}
		}

		public ulong TickCount
		{
			get
			{
				return Convert.ToUInt64(BitConverter.ToUInt64(this.Header, 237));
			}
			set
			{
				Array.Copy(BitConverter.GetBytes(value), 0, this.Header, 237, sizeof(ulong));
			}
		}

		public byte[] Misc2
		{
			get
			{
				return this.Header?.SubArray(245, 11);
			}
			set
			{
				Array.Copy(value, 0, this.Header, 245, 11);
			}
		}

		/// <summary>
		/// Serializes this DTM file back to disc.
		/// </summary>
		public void Save(string fileName)
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(fileName, FileMode.Create)))
			{
				binaryWriter.Write(this.Header);

				foreach (DTMFrameInfo frameInfo in FrameInfo)
				{
					binaryWriter.Write(frameInfo.Data);
				}
			}
		}

		private bool IsValidHeaderID(byte[] data)
		{
			return data.Length >= 4
				&& data[0] == 'D'
				&& data[1] == 'T'
				&& data[2] == 'M'
				&& data[3] == 0x1A;
		}
	}
	//// End class
}
//// End namespace
