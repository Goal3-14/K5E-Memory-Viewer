namespace K5E.Source.HeapVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    public enum SFAHeapEntryType : UInt16
    {
        Free    = 0x0,
        Ram     = 0x1,
        Ram2    = 0x4,
    }

    public enum SFAAllocTag : UInt32
    {
        ZERO            = 0x0,
        LISTS_COL       = 0x1,
        SCREEN_COL      = 0x2,
        CODE_COL        = 0x3,
        DLL_COL         = 0x4,
        TRACK_COL       = 0x5,
        TEX_COL         = 0x6,
        TRACKTEX_COL    = 0x7,
        SPRITETEX_COL   = 0x8,
        MODELS_COL      = 0x9,
        ANIMS_COL       = 0xa,
        AUDIO_COL       = 0xb,
        SEQ_COL         = 0xc,
        SFX_COL         = 0xd,
        OBJECTS_COL     = 0xe,
        CAM_COL         = 0xf,
        VOX_COL         = 0x10,
        ANIMSEQ_COL     = 0x11,
        LFX_COL         = 0x12,
        GFX_CO          = 0x13,
        EXPGFX_COL      = 0x14,
        MODGFX_COL      = 0x15,
        PROJGFX_COL     = 0x16,
        SKY_COL         = 0x17,
        SHAD_COL        = 0x18,
        GAME_COL        = 0x19,
        TEST_COL        = 0x1a,
        BLACK           = 0x1b,
        RED             = 0x1c,
        GREEN           = 0x1d,
        BLUE            = 0x1e,
        CYAN            = 0x1f,
        MAGENTA         = 0x20,
        YELLOW          = 0x21,
        WHITE           = 0x22,
        GREY            = 0x23,
        ORANGE          = 0x24,
        OBJECTS         = 0xeeff,
        VOX             = 0x880099,
        ANIMS           = 0x8888ff,
        TRACK           = 0xee00ff,
        MODELS          = 0xeeeeff,
        GAME            = 0x12345678,
        ANIMSEQ         = 0x4050ffff,
        FILE            = 0x7d7d7d7d,
        CODE            = 0x7e7e7efe,
        SHAD            = 0x7f7f7f99,
        COMPRESSED_FILE = 0x7f7f7fff,
        CAM             = 0x88000099,
        DLL             = 0x8e8e8efe,
        LISTS           = 0xcececefe,
        SFX             = 0xee0000bb,
        SEQ             = 0xee0000dd,
        AUDIO           = 0xee0000ff,
        SPRITETEX       = 0xee00ee99,
        TRACKTEX        = 0xee00eecc,
        TEX             = 0xee00eeff,
        SCREEN          = 0xeeeeeefe,
        FACEFEED        = 0xfacefeed,
        SKY             = 0xff3300ff,
        PROJGFX         = 0xff5500ff,
        MODGFX          = 0xff7700ff,
        EXPGFX          = 0xff9900ff,
        GFX             = 0xffaa00ff,
        LFX             = 0xffcc00ff,
        TEST            = 0xffdd00ff,
        INTERSECT_POINT = 0xffff00ff,
        SAVEGAME        = 0xffffffff,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 28)]
    public class SFAHeapEntry
    {
        [MarshalAs(UnmanagedType.I4)]
        public UInt32 entryPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 size;

        [MarshalAs(UnmanagedType.I2)]
        private UInt16 entryTypeRaw;

        [MarshalAs(UnmanagedType.I2)]
        public UInt16 prevIdx;

        [MarshalAs(UnmanagedType.I2)]
        public UInt16 nextIdx;

        [MarshalAs(UnmanagedType.I2)]
        public UInt16 selfIdxOrStack;

        [MarshalAs(UnmanagedType.I4)]
        private UInt32 allocTagRaw;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unk14;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 uniqueIdent;

        public SFAHeapEntryType entryType;
        public SFAAllocTag allocTag;

        public static SFAHeapEntry FromByteArray(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                SFAHeapEntry result = Marshal.PtrToStructure<SFAHeapEntry>(handle.AddrOfPinnedObject());

                // Fix GC endianness
                result.entryPtr = BinaryPrimitives.ReverseEndianness(result.entryPtr);
                result.size = BinaryPrimitives.ReverseEndianness(result.size);
                result.entryType = (SFAHeapEntryType)BinaryPrimitives.ReverseEndianness(result.entryTypeRaw);
                result.prevIdx = BinaryPrimitives.ReverseEndianness(result.prevIdx);
                result.nextIdx = BinaryPrimitives.ReverseEndianness(result.nextIdx);
                result.selfIdxOrStack = BinaryPrimitives.ReverseEndianness(result.selfIdxOrStack);
                result.allocTag = (SFAAllocTag)BinaryPrimitives.ReverseEndianness(result.allocTagRaw);
                result.unk14 = BinaryPrimitives.ReverseEndianness(result.unk14);
                result.uniqueIdent = BinaryPrimitives.ReverseEndianness(result.uniqueIdent);

                return result;
            }
            finally
            {
                handle.Free();
            }
        }
    }
    //// End class
}
//// End namespace
