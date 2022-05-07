namespace K5E.Source.DTMEditor
{
	using K5E.Engine.Common.Logging;
    using K5E.Source.Controls;
    using System.ComponentModel;

    public class DTMFrameInfo : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data">The frame info stored in a 64-bit integer.</param>
		public DTMFrameInfo(DTMFileInfo owner, ulong data)
		{
			this.Owner = owner;
			this.Data = data;
		}

		/// <summary>
		/// Raw representation of the controller data.
		/// </summary>
		[Browsable(false)]
		public ulong Data { get; private set; }

		private DTMFileInfo Owner { get; set; }

		[SortedCategory(SortedCategory.CategoryType.Frame), DisplayName("Frame"), Description("The frame of this input"), DefaultValue(false)]
		public int Frame
		{
			get
			{
				return this.Owner?.FrameInfo?.IndexOf(this) ?? -1;
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Buttons), DisplayName("A"), Description("Whether the A button is pressed"), DefaultValue(false)]
		public bool A
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.A);
			}
			set
			{
				this.ModifyButton(GameCubeButton.A, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Buttons), DisplayName("B"), Description("Whether the B button is pressed"), DefaultValue(false)]
		public bool B
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.B);
			}
			set
			{
				this.ModifyButton(GameCubeButton.B, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Buttons), DisplayName("Start"), Description("Whether the Start button is pressed"), DefaultValue(false)]
		public bool Start
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.Start);
			}
			set
			{
				this.ModifyButton(GameCubeButton.Start, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Buttons), DisplayName("X"), Description("Whether the X button is pressed"), DefaultValue(false)]
		public bool X
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.X);
			}
			set
			{
				this.ModifyButton(GameCubeButton.X, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Buttons), DisplayName("Y"), Description("Whether the Y button is pressed"), DefaultValue(false)]
		public bool Y
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.Y);
			}
			set
			{
				this.ModifyButton(GameCubeButton.Y, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Buttons), DisplayName("Z"), Description("Whether the Z button is pressed"), DefaultValue(false)]
		public bool Z
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.Z);
			}
			set
			{
				this.ModifyButton(GameCubeButton.Z, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Triggers), DisplayName("L"), Description("Whether the L button is pressed"), DefaultValue(false)]
		public bool L
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.L);
			}
			set
			{
				this.ModifyButton(GameCubeButton.L, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Triggers), DisplayName("R"), Description("Whether the R trigger is pressed"), DefaultValue(false)]
		public bool R
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.R);
			}
			set
			{
				this.ModifyButton(GameCubeButton.R, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Triggers), DisplayName("L-Trigger"), Description("The degree to which the L trigger is pressed"), DefaultValue(0)]
		public byte LTrigger
		{
			get
			{
				return this.GetTriggerValue(GameCubeTrigger.L);
			}
			set
			{
				this.ModifyTrigger(GameCubeTrigger.L, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.Triggers), DisplayName("R-Trigger"), Description("The degree to which the R trigger is pressed"), DefaultValue(0)]
		public byte RTrigger
		{
			get
			{
				return this.GetTriggerValue(GameCubeTrigger.R);
			}
			set
			{
				this.ModifyTrigger(GameCubeTrigger.R, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.ControlSticks), DisplayName("Control Stick X"), Description("The degree to which the main control stick X-axis is moved."), DefaultValue(128)]
		public byte ControlStickX
		{
			get
			{
				return this.GetAxisValue(GameCubeAxis.AnalogXAxis);
			}
			set
			{
				this.ModifyAxis(GameCubeAxis.AnalogXAxis, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.ControlSticks), DisplayName("Control Stick Y"), Description("The degree to which the main control stick Y-axis is moved."), DefaultValue(128)]
		public byte ControlStickY
		{
			get
			{
				return this.GetAxisValue(GameCubeAxis.AnalogYAxis);
			}
			set
			{
				this.ModifyAxis(GameCubeAxis.AnalogYAxis, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.ControlSticks), DisplayName("C-Stick X"), Description("The degree to which the C-stick X-axis is moved."), DefaultValue(128)]
		public byte CStickX
		{
			get
			{
				return this.GetAxisValue(GameCubeAxis.CStickXAxis);
			}
			set
			{
				this.ModifyAxis(GameCubeAxis.CStickXAxis, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.ControlSticks), DisplayName("C-Stick Y"), Description("The degree to which the C-stick Y-axis is moved."), DefaultValue(128)]
		public byte CStickY
		{
			get
			{
				return this.GetAxisValue(GameCubeAxis.CStickYAxis);
			}
			set
			{
				this.ModifyAxis(GameCubeAxis.CStickYAxis, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.DPad), DisplayName("D-Pad Down"), Description("Whether the D-Pad down button is pressed"), DefaultValue(false)]
		public bool DDown
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.DPadDown);
			}
			set
			{
				this.ModifyButton(GameCubeButton.DPadDown, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.DPad), DisplayName("D-Pad Left"), Description("Whether the D-Pad left button is pressed"), DefaultValue(false)]
		public bool DLeft
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.DPadLeft);
			}
			set
			{
				this.ModifyButton(GameCubeButton.DPadLeft, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.DPad), DisplayName("D-Pad Right"), Description("Whether the D-Pad right button is pressed"), DefaultValue(false)]
		public bool DRight
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.DPadRight);
			}
			set
			{
				this.ModifyButton(GameCubeButton.DPadRight, value);
			}
		}

		[SortedCategory(SortedCategory.CategoryType.DPad), DisplayName("D-Pad Up"), Description("Whether the D-Pad up button is pressed"), DefaultValue(false)]
		public bool DUp
		{
			get
			{
				return this.IsButtonPressed(GameCubeButton.DPadUp);
			}
			set
			{
				this.ModifyButton(GameCubeButton.DPadUp, value);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the given button is pressed.
		/// </summary>
		/// <param name="button">The button to check the state of.</param>
		/// <returns>true if the button is pressed, false otherwise.</returns>
		public bool IsButtonPressed(GameCubeButton button)
		{
			return (Data & (uint)button) != 0;
		}

		/// <summary>
		/// Gets the encoded axis value
		/// </summary>
		/// <param name="axis">Axis to get the value of.</param>
		/// <returns>The encoded axis value.</returns>
		public byte GetAxisValue(GameCubeAxis axis)
		{
			switch (axis)
			{
				case GameCubeAxis.AnalogXAxis:
					return (byte)((Data >> 32) & 0xFF);
				case GameCubeAxis.AnalogYAxis:
				case GameCubeAxis.CStickXAxis:
					return (byte)((Data >> 48) & 0xFF);
				case GameCubeAxis.CStickYAxis:
					return (byte)((Data >> 56) & 0xFF);
				default:
					Logger.Log(LogLevel.Error, string.Format("Unknown axis value {0}", axis));
					return 0;
			}
		}

		/// <summary>
		/// Gets the encoded trigger value.
		/// </summary>
		/// <param name="trigger">The trigger to get the value of.</param>
		/// <returns>The encoded trigger value.</returns>
		public byte GetTriggerValue(GameCubeTrigger trigger)
		{
			switch (trigger)
			{
				case GameCubeTrigger.L:
					return (byte)((Data >> 16) & 0xFF);
				case GameCubeTrigger.R:
					return (byte)((Data >> 24) & 0xFF);
				default:
					Logger.Log(LogLevel.Error, string.Format("Unknown trigger value {0}", trigger));
					return 0;
			}
		}

		/// <summary>
		/// Modifies the data to indicate whether or not a given button is pressed.
		/// </summary>
		/// <param name="button">The button to modify data of</param>
		/// <param name="pressed">Whether or not the given button should be considered pressed.</param>
		public void ModifyButton(GameCubeButton button, bool pressed)
		{
			if (pressed)
			{ 
				Data |= (ulong)button;
			}
			else
			{ 
				Data &= (ulong)~button;
			}
		}

		/// <summary>
		/// Modifies the data relating to the trigger values.
		/// </summary>
		/// <param name="trigger">The trigger to modify the data of.</param>
		/// <param name="value">The value to set.</param>
		public void ModifyTrigger(GameCubeTrigger trigger, byte value)
		{
			if (trigger == GameCubeTrigger.L)
			{
				Data = (Data & ~0xFF0000UL) | ((ulong)value << 16);
			}
			else if (trigger == GameCubeTrigger.R)
            {
                Data = (Data & ~0xFF000000UL) | ((ulong)value << 24);
            }
		}

		/// <summary>
		/// Modifies data relating to a certain axis.
		/// </summary>
		/// <param name="axis">The axis to modify the data of.</param>
		/// <param name="value">The value to use for the given axis.</param>
		public void ModifyAxis(GameCubeAxis axis, int value)
		{
			if (axis == GameCubeAxis.AnalogXAxis)
            {
                Data = (Data & ~0x000000FF00000000UL) | ((ulong)value << 32);
            }
            else if (axis == GameCubeAxis.AnalogYAxis)
			{
				Data = (Data & ~0x0000FF0000000000UL) | ((ulong)value << 40);
			}
            else if (axis == GameCubeAxis.CStickXAxis)
            {
                Data = (Data & ~0x00FF000000000000UL) | ((ulong)value << 48);
            }
            else if (axis == GameCubeAxis.CStickYAxis)
            {
                Data = (Data & ~0xFF00000000000000UL) | ((ulong)value << 56);
            }
		}
	}
	//// End class
}
//// End namespace
