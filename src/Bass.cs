using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace plr.BassLib
{
     /// <summary>
    /// Internet stream download callback function (to be used with <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" />).
    /// </summary>
    /// <param name="Buffer">The pointer to the Buffer containing the downloaded data... <see cref="IntPtr.Zero" /> = finished downloading.</param>
    /// <param name="Length">The number of bytes in the Buffer... 0 = HTTP or ICY tags.</param>
    /// <param name="User">The User instance data given when <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" /> was called.</param>
    /// <remarks>
    /// <para>
    /// The callback will be called before the <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" /> call returns (if it's successful), with the initial downloaded data.
    /// So any initialization (eg. creating the file if writing to disk) needs to be done either before the call, or in the callback function.
    /// </para>
    /// <para>
    /// When the <see cref="BassFlags.StreamStatus"/> flag is specified in the <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" /> call,
    /// HTTP and ICY tags may be passed to the callback during connection, before any stream data is received.
    /// The tags are given exactly as would be returned by <see cref="Bass.ChannelGetTags" />.
    /// You can destinguish between HTTP and ICY tags by checking what the first string starts with ("HTTP" or "ICY").
    /// </para>
    /// <para>
    /// A download callback function could be used in conjunction with a <see cref="SyncFlags.MetadataReceived"/> sync set via <see cref="Bass.ChannelSetSync" />,
    /// to save individual tracks to disk from a Shoutcast stream.
    /// </para>
    /// </remarks>
    public delegate void DownloadProcedure(IntPtr Buffer, int Length, IntPtr User);

    /// <summary>
    /// User defined synchronizer callback function (see <see cref="Bass.ChannelSetSync" /> for details).
    /// </summary>
    /// <param name="Handle">The sync Handle that has occured (as returned by <see cref="Bass.ChannelSetSync" />).</param>
    /// <param name="Channel">The channel that the sync occured on.</param>
    /// <param name="Data">Additional data associated with the sync's occurance.</param>
    /// <param name="User">The User instance data given when <see cref="Bass.ChannelSetSync" /> was called.</param>
    /// <remarks>
    /// <para>
    /// BASS creates a single thread dedicated to executing sync callback functions, so a callback function should be quick as other syncs cannot be processed until it has finished.
    /// Attribute slides (<see cref="Bass.ChannelSlideAttribute" />) are also performed by the sync thread, so are also affected if a sync callback takes a long time.</para>
    /// <para>"Mixtime" syncs <see cref="SyncFlags.Mixtime"/> are not executed in the sync thread, but immediately in whichever thread triggers them.
    /// In most cases that will be an update thread, and so the same restrictions that apply to stream callbacks (<see cref="StreamProcedure" />) also apply here.</para>
    /// <para>
    /// <see cref="Bass.ChannelSetPosition" /> can be used in a mixtime sync to implement custom looping,
    /// eg. set a <see cref="SyncFlags.Position"/> sync at the loop end position and seek to the loop start position in the callback.
    /// </para>
    /// </remarks>
    public delegate void SyncProcedure(int Handle, int Channel, int Data, IntPtr User);

    /// <summary>
    /// Sync types to be used with <see cref="Bass.ChannelSetSync" /> (param flag) and <see cref="SyncProcedure" /> (data flag).
    /// </summary>
    [Flags]
    internal enum SyncFlags
    {
        /// <summary>
        /// FLAG: sync only once, else continuously
        /// </summary>
        Onetime = -2147483648,

        /// <summary>
        /// FLAG: sync at mixtime, else at playtime
        /// </summary>
        Mixtime = 1073741824,

        /// <summary>
        /// Sync when a channel reaches a position.
        /// param : position in bytes
        /// data : not used
        /// </summary>
        Position = 0,

        /// <summary>
        /// Sync when an instrument (sample for the non-instrument based formats) is played in a MOD music (not including retrigs).
        /// param : LOWORD=instrument (1=first) HIWORD=note (0=c0...119=b9, -1=all)
        /// data : LOWORD=note HIWORD=volume (0-64)
        /// </summary>
        MusicInstrument = 1,

        /// <summary>
        /// Sync when a channel reaches the end.
        /// param : not used
        /// data : 1 = the sync is triggered by a backward jump in a MOD music, otherwise not used
        /// </summary>
        End = 2,

        /// <summary>
        /// Sync when the "sync" effect (XM/MTM/MOD: E8x/Wxx, IT/S3M: S2x) is used.
        /// param : 0:data=Position, 1:data="x" value
        /// data : param=0: LOWORD=order HIWORD=row, param=1: "x" value
        /// </summary>
        MusicFx = 3,

        /// <summary>
        /// Sync when metadata is received in a stream.
        /// param : not used
        /// data : not used - the updated metadata is available from <see cref="Bass.ChannelGetTags"/>
        /// </summary>
        MetadataReceived = 4,

        /// <summary>
        /// Sync when an attribute slide is completed.
        /// param : not used
        /// data : the Type of slide completed (one of the <see cref="ChannelAttribute"/> values)
        /// </summary>
        Slided = 5,

        /// <summary>
        /// Sync when playback has stalled.
        /// param : not used
        /// data : 0=stalled, 1=resumed
        /// </summary>
        Stalled = 6,

        /// <summary>
        /// Sync when downloading of an internet (or "buffered" User file) stream has ended.
        /// param : not used
        /// data : not used
        /// </summary>
        Downloaded = 7,

        /// <summary>
        /// Sync when a channel is freed.
        /// param : not used
        /// data : not used
        /// </summary>
        Free = 8,

        /// <summary>
        /// Sync when a MOD music reaches an order:row position.
        /// param : LOWORD=order (0=first, -1=all) HIWORD=row (0=first, -1=all)
        /// data : LOWORD=order HIWORD=row
        /// </summary>
        MusicPosition = 10,

        /// <summary>
        /// Sync when seeking (inc. looping and restarting).
        /// So it could be used to reset DSP/etc.
        /// param : position in bytes
        /// data : 0=playback is unbroken, 1=if is it broken (eg. Buffer flushed).
        /// The latter would be the time to reset DSP/etc.
        /// </summary>
        Seeking = 11,

        /// <summary>
        /// Sync when a new logical bitstream begins in a chained OGG stream.
        /// Updated tags are available from <see cref="Bass.ChannelGetTags"/>.
        /// param : not used
        /// data : not used
        /// </summary>
        OggChange = 12,

        /// <summary>
        /// Sync when the DirectSound Buffer fails during playback, eg. when the device is no longer available.
        /// param : not used
        /// data : not used
        /// </summary>
        Stop = 14,

        /// <summary>
        /// WINAMP add-on: Sync when bitrate is changed or retrieved from a winamp Input plug-in.
        /// param : not used
        /// data : the bitrate retrieved from the winamp Input plug-in -
        /// called when it is retrieved or changed (VBR MP3s, OGGs, etc).
        /// </summary>
        WinampBitRate = 100,

        #region BassCd
        /// <summary>
        /// CD add-on: Sync when playback is stopped due to an error.
        /// For example, the drive door being opened.
        /// param : not used
        /// data : the position that was being read from the CD track at the time.
        /// </summary>
        CDError = 1000,

        /// <summary>
        /// CD add-on: Sync when the read speed is automatically changed due to the BassCd.AutoSpeedReduction setting.
        /// param : not used
        /// data : the new read speed.
        /// </summary>
        CDSpeed = 1002,
        #endregion

        #region BassMidi
        /// <summary>
        /// MIDI add-on: Sync when a marker is encountered.
        /// param : not used
        /// data : the marker index, which can be used in a <see cref="Midi.BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker)"/> call.
        /// </summary>
        MidiMarker = 0x10000,

        /// <summary>
        /// MIDI add-on: Sync when a cue is encountered.
        /// param : not used
        /// data : the marker index, which can be used in a <see cref="Midi.BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker)"/> call.
        /// </summary>
        MidiCue = 0x10001,

        /// <summary>
        /// MIDI add-on: Sync when a lyric event is encountered.
        /// param : not used
        /// data : the marker index, which can be used in a <see cref="Midi.BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker)"/> call.
        /// If the text begins with a '/' (slash) character, a new line should be started.
        /// If it begins with a '\' (backslash) character, the display should be cleared.
        /// </summary>
        MidiLyric = 0x10002,

        /// <summary>
        /// MIDI add-on: Sync when a text event is encountered.
        /// param : not used
        /// data : the marker index, which can be used in a <see cref="Midi.BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker)"/> call.
        /// Lyrics can sometimes be found in <see cref="Midi.MidiMarkerType.Text"/> instead of <see cref="Midi.MidiMarkerType.Lyric"/> markers.
        /// </summary>
        MidiText = 0x10003,

        /// <summary>
        /// MIDI add-on: Sync when a Type of event is processed, in either a MIDI file or <see cref="Midi.BassMidi.StreamEvent(int,int,Midi.MidiEventType,int)"/>.
        /// param : event Type (0 = all types).
        /// data : LOWORD = event parameter, HIWORD = channel (high 8 bits contain the event Type when syncing on all types).
        /// See <see cref="Midi.BassMidi.StreamEvent(int,int,Midi.MidiEventType,int)"/> for a list of event types and their parameters.
        /// </summary>
        MidiEvent = 0x10004,

        /// <summary>
        /// MIDI add-on: Sync when reaching a tick position.
        /// param : tick position.
        /// data : not used
        /// </summary>
        MidiTick = 0x10005,

        /// <summary>
        /// MIDI add-on: Sync when a time signature event is processed.
        /// param : event Type.
        /// data : The time signature events are given (by <see cref="Midi.BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker)"/>)
        /// in the form of "numerator/denominator metronome-pulse 32nd-notes-per-MIDI-quarter-note", eg. "4/4 24 8".
        /// </summary>
        MidiTimeSignature = 0x10006,

        /// <summary>
        /// MIDI add-on: Sync when a key signature event is processed.
        /// param : event Type.
        /// data : The key signature events are given (by <see cref="Midi.BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker)"/>) in the form of "a b",
        /// where a is the number of sharps (if positive) or flats (if negative),
        /// and b signifies major (if 0) or minor (if 1).
        /// </summary>
        MidiKeySignature = 0x10007,
        #endregion

        #region BassWma
        /// <summary>
        /// WMA add-on: Sync on a track change in a server-side playlist.
        /// Updated tags are available via <see cref="Bass.ChannelGetTags"/>.
        /// param : not used
        /// data : not used
        /// </summary>
        WmaChange = 0x10100,

        /// <summary>
        /// WMA add-on: Sync on a mid-stream tag change in a server-side playlist.
        /// Updated tags are available via <see cref="Bass.ChannelGetTags"/>.
        /// param : not used
        /// data : not used - the updated metadata is available from <see cref="Bass.ChannelGetTags"/>.
        /// </summary>
        WmaMeta = 0x10101,
        #endregion

        #region BassMix
        /// <summary>
        /// MIX add-on: Sync when an envelope reaches the end.
        /// param : not used
        /// data : envelope Type
        /// </summary>
        MixerEnvelope = 0x10200,

        /// <summary>
        /// MIX add-on: Sync when an envelope node is reached.
        /// param : Optional limit the sync to a certain envelope Type (one of the BASSMIXEnvelope values).
        /// data : Will contain the envelope Type in the LOWORD and the current node number in the HIWORD.
        /// </summary>
        MixerEnvelopeNode = 0x10201,
        #endregion

        /// <summary>
        /// Sync when a new segment begins downloading.
        /// Mixtime only.
        /// param: not used.
        /// data: not used.
        /// </summary>
        HlsSegement = 0x10300
    }

    /// <summary>
    /// Stream/Sample/Music/Recording/AddOn create flags to be used with Stream Creation functions.
    /// </summary>
    [Flags]
    internal enum BassFlags : uint
    {
        /// <summary>
        /// 0 = default create stream: 16 Bit, stereo, no Float, hardware mixing, no Loop, no 3D, no speaker assignments...
        /// </summary>
        Default,

        /// <summary>
        /// Use 8-bit resolution. If neither this or the <see cref="Float"/> flags are specified, then the stream is 16-bit.
        /// </summary>
        Byte = 0x1,

        /// <summary>
        /// Decode/play the stream (MP3/MP2/MP1 only) in mono, reducing the CPU usage (if it was originally stereo).
        /// This flag is automatically applied if <see cref="DeviceInitFlags.Mono"/> was specified when calling <see cref="Bass.Init"/>.
        /// </summary>
        Mono = 0x2,

        /// <summary>
        /// Loop the file. This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        Loop = 0x4,

        /// <summary>
        /// Use 3D functionality.
        /// This is ignored if <see cref="DeviceInitFlags.Device3D"/> wasn't specified when calling <see cref="Bass.Init"/>.
        /// 3D streams must be mono (chans=1).
        /// The Speaker flags can not be used together with this flag.
        /// </summary>
        Bass3D = 0x8,

        /// <summary>
        /// Force the stream to not use hardware mixing (Windows Only).
        /// </summary>
        SoftwareMixing = 0x10,

        /// <summary>
        /// Enable the old implementation of DirectX 8 effects (Windows Only).
        /// Use <see cref="Bass.ChannelSetFX"/> to add effects to the stream.
        /// Requires DirectX 8 or above.
        /// </summary>
        FX = 0x80,
        
        /// <summary>
        /// Use 32-bit floating-point sample data (see Floating-Point Channels for details).
        /// WDM drivers or the <see cref="Decode"/> flag are required to use this flag.
        /// </summary>
        Float = 0x100,

        /// <summary>
        /// Enable pin-point accurate seeking (to the exact byte) on the MP3/MP2/MP1 stream or MOD music.
        /// This also increases the time taken to create the stream,
        /// due to the entire file being pre-scanned for the seek points.
        /// Note: This flag is ONLY needed for files with a VBR, files with a CBR are always accurate.
        /// </summary>
        Prescan = 0x20000,

        /// <summary>
        /// Automatically free the music or stream's resources when it has reached the end,
        /// or when <see cref="Bass.ChannelStop"/> or <see cref="Bass.Stop"/> is called.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        AutoFree = 0x40000,

        /// <summary>
        /// Restrict the download rate of the file to the rate required to sustain playback.
        /// If this flag is not used, then the file will be downloaded as quickly as possible.
        /// This flag has no effect on "unbuffered" streams (Buffer=false).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        RestrictDownloadRate = 0x80000,

        /// <summary>
        /// Download and play the file in smaller chunks.
        /// Uses a lot less memory than otherwise,
        /// but it's not possible to seek or loop the stream - once it's ended,
        /// the file must be opened again to play it again.
        /// This flag will automatically be applied when the file Length is unknown.
        /// This flag also has the effect of resticting the download rate.
        /// This flag has no effect on "unbuffered" streams (Buffer=false).
        /// </summary>
        StreamDownloadBlocks = 0x100000,

        /// <summary>
        /// Decode the sample data, without outputting it.
        /// Use <see cref="Bass.ChannelGetData(int,IntPtr,int)"/> to retrieve decoded sample data.
        /// Bass.SoftwareMixing/<see cref="Bass3D"/>/BassFlags.FX/<see cref="AutoFree"/> are all ignored when using this flag, as are the Speaker flags.
        /// </summary>
        Decode = 0x200000,

        /// <summary>
        /// Pass status info (HTTP/ICY tags) from the server to the <see cref="DownloadProcedure"/> callback during connection.
        /// This can be useful to determine the reason for a failure.
        /// </summary>
        StreamStatus = 0x800000,

        /// <summary>
        /// Use an async look-ahead cache.
        /// </summary>
        AsyncFile = 0x40000000,

        /// <summary>
        /// File is a Unicode (16-bit characters) filename
        /// </summary>
        Unicode = 0x80000000,

        #region BassFx
        /// <summary>
        /// BassFx add-on: If in use, then you can do other stuff while detection's in process.
        /// </summary>
        FxBpmBackground = 0x1,

        /// <summary>
        /// BassFx add-on: If in use, then will auto multiply bpm by 2 (if BPM &lt; MinBPM*2)
        /// </summary>
        FXBpmMult2 = 0x2,

        /// <summary>
        /// BassFx add-on (<see cref="Fx.BassFx.TempoCreate"/>): Uses a linear interpolation mode (simple).
        /// </summary>
        FxTempoAlgorithmLinear = 0x200,

        /// <summary>
        /// BassFx add-on (<see cref="Fx.BassFx.TempoCreate"/>): Uses a cubic interpolation mode (recommended, default).
        /// </summary>
        FxTempoAlgorithmCubic = 0x400,

        /// <summary>
        /// BassFx add-on (<see cref="Fx.BassFx.TempoCreate"/>):
        /// Uses a 8-tap band-limited Shannon interpolation (complex, but not much better than cubic).
        /// </summary>
        FxTempoAlgorithmShannon = 0x800,

        /// <summary>
        /// BassFx add-on: Free the source Handle as well?
        /// </summary>
        FxFreeSource = 0x10000,
        #endregion

        #region BassMidi
        /// <summary>
        /// BASSMIDI add-on: Don't send a WAVE header to the encoder.
        /// If this flag is used then the sample format (mono 16-bit)
        /// must be passed to the encoder some other way, eg. via the command-line.
        /// </summary>
        MidiNoHeader = 0x1,

        /// <summary>
        /// BASSMIDI add-on: Reduce 24-bit sample data to 16-bit before encoding.
        /// </summary>
        Midi16Bit = 0x2,

        /// <summary>
        /// BASSMIDI add-on: Ignore system reset events (<see cref="Midi.MidiEventType.System"/>) when the system mode is unchanged.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MidiNoSystemReset = 0x800,

        /// <summary>
        /// BASSMIDI add-on: Let the sound decay naturally (including reverb) instead of stopping it abruptly at the end of the file.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/> methods.
        /// </summary>
        MidiDecayEnd = 0x1000,

        /// <summary>
        /// BASSMIDI add-on: Disable the MIDI reverb/chorus processing.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MidiNoFx = 0x2000,

        /// <summary>
        /// BASSMIDI add-on: Let the old sound decay naturally (including reverb) when changing the position, including looping.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>, and can also be used in <see cref="Bass.ChannelSetPosition"/>
        /// calls to have it apply to particular position changes.
        /// </summary>
        MidiDecaySeek = 0x4000,

        /// <summary>
        /// BASSMIDI add-on: Do not remove empty space (containing no events) from the end of the file.
        /// </summary>
        MidiNoCrop = 0x8000,

        /// <summary>
        /// BASSMIDI add-on: Only release the oldest instance upon a note off event (<see cref="Midi.MidiEventType.Note"/> with velocity=0)
        /// when there are overlapping instances of the note.
        /// Otherwise all instances are released.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MidiNoteOff1 = 0x10000,

        /// <summary>
        /// BASSMIDI add-on: Map the file into memory.
        /// This flag is ignored if the soundfont is packed as the sample data cannot be played directly from a mapping;
        /// it needs to be decoded.
        /// This flag is also ignored if the file is too large to be mapped into memory.
        /// </summary>
        MidiFontMemoryMap = 0x20000,

        /// <summary>
        /// Use bank 127 in the soundfont for XG drum kits.
        /// When an XG drum kit is needed, bank 127 in soundfonts that have this flag set will be checked first,
        /// before falling back to bank 128 (the standard SF2 drum kit bank) if it is not available there.
        /// </summary>
        MidiFontXGDRUMS = 0x40000,
        #endregion

        /// <summary>
        /// Music and BASSMIDI add-on: Use sinc interpolated sample mixing.
        /// This increases the sound quality, but also requires more CPU.
        /// Otherwise linear interpolation is used.
        /// Music: If neither this or the <see cref="MusicNonInterpolated"/> flag is specified, linear interpolation is used.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        SincInterpolation = 0x800000,

        #region MOD Music
        /// <summary>
        /// Music: Use "normal" ramping (as used in FastTracker 2).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicRamp = 0x200,

        /// <summary>
        /// Music: Use "sensitive" ramping.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicSensitiveRamping = 0x400,

        /// <summary>
        /// Music: Apply XMPlay's surround sound to the music (ignored in mono).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicSurround = 0x800,

        /// <summary>
        /// Music: Apply XMPlay's surround sound mode 2 to the music (ignored in mono).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicSurround2 = 0x1000,

        /// <summary>
        /// Music: Play .MOD file as FastTracker 2 would.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicFT2Mod = 0x2000,

        /// <summary>
        /// Apply FastTracker 2 panning to XM files.
        /// </summary>
        MusicFT2PAN = 0x2000,

        /// <summary>
        /// Music: Play .MOD file as ProTracker 1 would.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicPT1Mod = 0x4000,

        /// <summary>
        /// Music: Stop all notes when seeking (using <see cref="Bass.ChannelSetPosition"/>).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicPositionReset = 0x8000,

        /// <summary>
        /// Music: Use non-interpolated mixing.
        /// This generally reduces the sound quality, but can be good for chip-tunes.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicNonInterpolated = 0x10000,

        /// <summary>
        /// Music: Stop the music when a backward jump effect is played.
        /// This stops musics that never reach the end from going into endless loops.
        /// Some MOD musics are designed to jump all over the place,
        /// so this flag would cause those to be stopped prematurely.
        /// If this flag is used together with the <see cref="Loop"/> flag,
        /// then the music would not be stopped but any <see cref="SyncFlags.End"/> sync would be triggered.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicStopBack = 0x80000,

        /// <summary>
        /// Music: Don't load the samples.
        /// This reduces the time taken to load the music, notably with MO3 files,
        /// which is useful if you just want to get the name and Length of the music without playing it.
        /// </summary>
        MusicNoSample = 0x100000,

        /// <summary>
        /// Music: Stop all notes and reset bpm/etc when seeking.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicPositionResetEx = 0x400000,
        #endregion

        #region Sample
        /// <summary>
        /// Sample: muted at max distance (3D only)
        /// </summary>
        MuteMax = 0x20,

        /// <summary>
        /// Sample: uses the DX7 voice allocation and management
        /// </summary>
        VAM = 0x40,

        /// <summary>
        /// Sample: override lowest volume
        /// </summary>
        SampleOverrideLowestVolume = 0x10000,

        /// <summary>
        /// Sample: override longest playing
        /// </summary>
        SampleOverrideLongestPlaying = 0x20000,

        /// <summary>
        /// Sample: override furthest from listener (3D only)
        /// </summary>
        SampleOverrideDistance = 0x30000,
        #endregion

        #region BassCd
        /// <summary>
        /// BASSCD add-on: Read sub-channel data.
        /// 96 bytes of de-interleaved sub-channel data will be returned after each 2352 bytes of audio.
        /// This flag can not be used with the <see cref="Float"/> flag,
        /// and is ignored if the <see cref="Decode"/> flag is not used.
        /// </summary>
        CDSubChannel = 0x200,

        /// <summary>
        /// BASSCD add-on: Read sub-channel data, without using any hardware de-interleaving.
        /// This is identical to the <see cref="CDSubChannel"/> flag, except that the
        /// de-interleaving is always performed by BASSCD even if the drive is apparently capable of de-interleaving itself.
        /// </summary>
        CDSubchannelNoHW = 0x400,

        /// <summary>
        /// BASSCD add-on: Include C2 error info.
        /// 296 bytes of C2 error info is returned after each 2352 bytes of audio (and optionally 96 bytes of sub-channel data).
        /// This flag cannot be used with the <see cref="Float"/> flag, and is ignored if the <see cref="Decode"/> flag is not used.
        /// The first 294 bytes contain the C2 error bits (one bit for each byte of audio),
        /// followed by a byte containing the logical OR of all 294 bytes,
        /// which can be used to quickly check if there were any C2 errors.
        /// The final byte is just padding.
        /// Note that if you request both sub-channel data and C2 error info, the C2 info will come before the sub-channel data!
        /// </summary>
        CdC2Errors = 0x800,
        #endregion

        #region BassMix
        /// <summary>
        /// BASSmix add-on: only read buffered data.
        /// </summary>
        SplitSlave = 0x1000,
        
        /// <summary>
        /// BASSmix add-on: The splitter's length and position is based on the splitter's (rather than the source's) channel count.
        /// </summary>
        SplitPosition = 0x2000,

        /// <summary>
        /// BASSmix add-on: resume a stalled mixer immediately upon new/unpaused source
        /// </summary>
        MixerResume = 0x1000,

        /// <summary>
        /// BASSmix add-on: enable <see cref="Mix.BassMix.ChannelGetPosition(int,PositionFlags,int)"/> support.
        /// </summary>
        MixerPositionEx = 0x2000,

        /// <summary>
        /// BASSmix add-on: Buffer source data for <see cref="Mix.BassMix.ChannelGetData(int,IntPtr,int)"/> and <see cref="Mix.BassMix.ChannelGetLevel(int)"/>.
        /// </summary>
        MixerBuffer = 0x2000,

        /// <summary>
        /// BASSmix add-on: Limit mixer processing to the amount available from this source.
        /// </summary>
        MixerLimit = 0x4000,

        /// <summary>
        /// BASSmix add-on: end the stream when there are no sources
        /// </summary>
        MixerEnd = 0x10000,

        /// <summary>
        /// BASSmix add-on: Matrix mixing
        /// </summary>
        MixerMatrix = 0x10000,

        /// <summary>
        /// BASSmix add-on: don't stall when there are no sources
        /// </summary>
        MixerNonStop = 0x20000,

        /// <summary>
        /// BASSmix add-on: don't process the source
        /// </summary>
        MixerPause = 0x20000,

        /// <summary>
        /// BASSmix add-on: downmix to stereo (or mono if mixer is mono)
        /// </summary>
        MixerDownMix = 0x400000,

        /// <summary>
        /// BASSmix add-on: don't ramp-in the start
        /// </summary>
        MixerNoRampin = 0x800000,
        #endregion

        #region Recording
        /// <summary>
        /// Recording: Start the recording paused. Use <see cref="Bass.ChannelPlay"/> to start it.
        /// </summary>
        RecordPause = 0x8000,

        /// <summary>
		/// Recording: Enable Echo Cancellation (only available on certain devices, like iOS).
		/// </summary>
        RecordEchoCancel = 0x2000,

        /// <summary>
		/// Recording: Enable Automatic Gain Control (only available on certain devices, like iOS).
		/// </summary>
        RecordAGC = 0x4000,
        #endregion

        #region Speaker Assignment
        /// <summary>
        /// Front speakers (channel 1/2)
        /// </summary>
        SpeakerFront = 0x1000000,

        /// <summary>
        /// Rear/Side speakers (channel 3/4)
        /// </summary>
        SpeakerRear = 0x2000000,

        /// <summary>
        /// Center and LFE speakers (5.1, channel 5/6)
        /// </summary>
        SpeakerCenterLFE = 0x3000000,

        /// <summary>
        /// Rear Center speakers (7.1, channel 7/8)
        /// </summary>
        SpeakerRearCenter = 0x4000000,

        #region Pairs
        /// <summary>
        /// Speakers Pair 1
        /// </summary>
        SpeakerPair1 = 1 << 24,

        /// <summary>
        /// Speakers Pair 2
        /// </summary>
        SpeakerPair2 = 2 << 24,

        /// <summary>
        /// Speakers Pair 3
        /// </summary>
        SpeakerPair3 = 3 << 24,

        /// <summary>
        /// Speakers Pair 4
        /// </summary>
        SpeakerPair4 = 4 << 24,

        /// <summary>
        /// Speakers Pair 5
        /// </summary>
        SpeakerPair5 = 5 << 24,

        /// <summary>
        /// Speakers Pair 6
        /// </summary>
        SpeakerPair6 = 6 << 24,

        /// <summary>
        /// Speakers Pair 7
        /// </summary>
        SpeakerPair7 = 7 << 24,

        /// <summary>
        /// Speakers Pair 8
        /// </summary>
        SpeakerPair8 = 8 << 24,

        /// <summary>
        /// Speakers Pair 9
        /// </summary>
        SpeakerPair9 = 9 << 24,

        /// <summary>
        /// Speakers Pair 10
        /// </summary>
        SpeakerPair10 = 10 << 24,

        /// <summary>
        /// Speakers Pair 11
        /// </summary>
        SpeakerPair11 = 11 << 24,

        /// <summary>
        /// Speakers Pair 12
        /// </summary>
        SpeakerPair12 = 12 << 24,

        /// <summary>
        /// Speakers Pair 13
        /// </summary>
        SpeakerPair13 = 13 << 24,

        /// <summary>
        /// Speakers Pair 14
        /// </summary>
        SpeakerPair14 = 14 << 24,

        /// <summary>
        /// Speakers Pair 15
        /// </summary>
        SpeakerPair15 = 15 << 24,
        #endregion

        #region Modifiers
        /// <summary>
        /// Speaker Modifier: left channel only
        /// </summary>
        SpeakerLeft = 0x10000000,

        /// <summary>
        /// Speaker Modifier: right channel only
        /// </summary>
        SpeakerRight = 0x20000000,
        #endregion

        /// <summary>
        /// Front Left speaker only (channel 1)
        /// </summary>
        SpeakerFrontLeft = SpeakerFront | SpeakerLeft,

        /// <summary>
        /// Rear/Side Left speaker only (channel 3)
        /// </summary>
        SpeakerRearLeft = SpeakerRear | SpeakerLeft,

        /// <summary>
        /// Center speaker only (5.1, channel 5)
        /// </summary>
        SpeakerCenter = SpeakerCenterLFE | SpeakerLeft,

        /// <summary>
        /// Rear Center Left speaker only (7.1, channel 7)
        /// </summary>
        SpeakerRearCenterLeft = SpeakerRearCenter | SpeakerLeft,

        /// <summary>
        /// Front Right speaker only (channel 2)
        /// </summary>
        SpeakerFrontRight = SpeakerFront | SpeakerRight,

        /// <summary>
        /// Rear/Side Right speaker only (channel 4)
        /// </summary>
        SpeakerRearRight = SpeakerRear | SpeakerRight,

        /// <summary>
        /// LFE speaker only (5.1, channel 6)
        /// </summary>
        SpeakerLFE = SpeakerCenterLFE | SpeakerRight,

        /// <summary>
        /// Rear Center Right speaker only (7.1, channel 8)
        /// </summary>
        SpeakerRearCenterRight = SpeakerRearCenter | SpeakerRight,
        #endregion

        #region BassAac
        /// <summary>
        /// BassAac add-on: use 960 samples per frame
        /// </summary>
        AacFrame960 = 0x1000,

        /// <summary>
        /// BassAac add-on: Downmatrix to Stereo
        /// </summary>
        AacStereo = 0x400000,
        #endregion

        #region BassDSD
        /// <summary>
        /// BassDSD add-on: Produce DSD-over-PCM data (with 0x05/0xFA markers). DSD-over-PCM data is 24-bit, so the <see cref="Float"/> flag is required.
        /// </summary>
        DSDOverPCM = 0x400,

        /// <summary>
        /// BassDSD add-on: Produce raw DSD data instead of PCM. The DSD data is in blocks of 8 bits (1 byte) per-channel with the MSB being first/oldest.
        /// DSD data is not playable by BASS, so the <see cref="Decode"/> flag is required.
        /// </summary>
        DSDRaw = 0x200,
        #endregion

        #region BassAc3
        /// <summary>
        /// BassAC3 add-on: downmix to stereo
        /// </summary>
        Ac3DownmixStereo = 0x200,

        /// <summary>
        /// BASS_AC3 add-on: downmix to quad
        /// </summary>
        Ac3DownmixQuad = 0x400,

        /// <summary>
        /// BASS_AC3 add-on: downmix to dolby
        /// </summary>
        Ac3DownmixDolby = 0x600,

        /// <summary>
        /// BASS_AC3 add-on: enable dynamic range compression
        /// </summary>
        Ac3DRC = 0x800,
        #endregion

        #region BassDShow
        /// <summary>
        /// DSHOW add-on: Use this flag to disable audio processing.
        /// </summary>
        DShowNoAudioProcessing = 0x80000,

        /// <summary>
        /// DSHOW add-on: Use this flag to enable mixing video on a channel.
        /// </summary>
        DShowStreamMix = 0x1000000,

        /// <summary>
        /// DSHOW add-on: Use this flag to enable auto dvd functions(on mouse down, keys etc).
        /// </summary>
        DShowAutoDVD = 0x4000000,

        /// <summary>
        /// DSHOW add-on: Use this flag to restart the stream when it's finished.
        /// </summary>
        DShowLoop = 0x8000000,

        /// <summary>
        /// DSHOW add-on: Use this to enable video processing.
        /// </summary>
        DShowVideoProcessing = 0x20000,
        #endregion

        /// <summary>
        /// BassWV add-on: Limit to stereo
        /// </summary>
        WVStereo = 0x400000
    }

    /// <summary>
    /// Channel attribute options used by <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)" /> and <see cref="Bass.ChannelGetAttribute(int,ChannelAttribute,out float)" />.
    /// </summary>
    internal enum ChannelAttribute
    {
        /// <summary>
        /// The sample rate of a channel... 0 = original rate (when the channel was created).
        /// <para>
        /// This attribute applies to playback of the channel, and does not affect the
        /// channel's sample data, so has no real effect on decoding channels.
        /// It is still adjustable though, so that it can be used by the BassMix add-on,
        /// and anything else that wants to use it.
        /// </para>
        /// <para>
        /// It is not possible to change the sample rate of a channel if the "with FX
        /// flag" DX8 effect implementation enabled on it, unless DirectX 9 or above is installed.
        /// </para>
        /// <para>
        /// Increasing the sample rate of a stream or MOD music increases its CPU usage,
        /// and reduces the Length of its playback Buffer in terms of time.
        /// If you intend to raise the sample rate above the original rate, then you may also need
        /// to increase the Buffer Length via the <see cref="Bass.PlaybackBufferLength"/>
        /// config option to avoid break-ups in the sound.
        /// </para>
        ///
        /// <para><b>Platform-specific</b></para>
        /// <para>On Windows, the sample rate will get rounded down to a whole number during playback.</para>
        /// </summary>
        Frequency = 0x1,

        /// <summary>
        /// The volume level of a channel... 0 (silent) to 1 (full).
        /// <para>This can go above 1.0 on decoding channels.</para>
        /// <para>
        /// This attribute applies to playback of the channel, and does not affect the
        /// channel's sample data, so has no real effect on decoding channels.
        /// It is still adjustable though, so that it can be used by the BassMix add-on,
        /// and anything else that wants to use it.
        /// </para>
        /// <para>
        /// When using <see cref="Bass.ChannelSlideAttribute"/>
        /// to slide this attribute, a negative volume value can be used to fade-out and then stop the channel.
        /// </para>
        /// </summary>
        Volume = 0x2,

        /// <summary>
        /// The panning/balance position of a channel... -1 (Full Left) to +1 (Full Right), 0 = Centre.
        /// <para>
        /// This attribute applies to playback of the channel, and does not affect the
        /// channel's sample data, so has no real effect on decoding channels.
        /// It is still adjustable though, so that it can be used by the <see cref="Mix.BassMix"/> add-on,
        /// and anything else that wants to use it.
        /// </para>
        /// <para>
        /// It is not possible to set the pan position of a 3D channel.
        /// It is also not possible to set the pan position when using speaker assignment, but if needed,
        /// it can be done via a <see cref="DSPProcedure"/> instead (not on mono channels).
        /// </para>
        ///
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Windows, this attribute has no effect when speaker assignment is used,
        /// except on Windows Vista and newer with the Bass.VistaSpeakerAssignment config option enabled.
        /// Balance control could be implemented via a <see cref="DSPProcedure"/> instead
        /// </para>
        /// </summary>
        Pan = 0x3,

        /// <summary>
        /// The wet (reverb) / dry (no reverb) mix ratio... 0 (full dry) to 1 (full wet), -1 = automatically calculate the mix based on the distance (the default).
        /// <para>For a sample, stream, or MOD music channel with 3D functionality.</para>
        /// <para>
        /// Obviously, EAX functions have no effect if the output device does not support EAX.
        /// <see cref="Bass.GetInfo"/> can be used to check that.
        /// </para>
        /// <para>
        /// EAX only affects 3D channels, but EAX functions do not require <see cref="Bass.Apply3D"/> to apply the changes.
        /// LastError.NoEAX: The channel does not have EAX support.
        /// EAX only applies to 3D channels that are mixed by the hardware/drivers.
        /// <see cref="Bass.ChannelGetInfo(int, out ChannelInfo)"/> can be used to check if a channel is being mixed by the hardware.
        /// EAX is only supported on Windows.
        /// </para>
        /// </summary>
        EaxMix = 0x4,

        /// <summary>
        /// Non-Windows: Disable playback buffering?... 0 = no, else yes..
        /// <para>
        /// A playing channel is normally asked to render data to its playback Buffer in advance,
        /// via automatic Buffer updates or the <see cref="Bass.Update"/> and <see cref="Bass.ChannelUpdate"/> functions,
        /// ready for mixing with other channels to produce the final mix that is given to the output device.
        /// </para>
        /// <para>
        /// When this attribute is switched on (the default is off), that buffering is skipped and
        /// the channel will only be asked to produce data as it is needed during the generation of the final mix.
        /// This allows the lowest latency to be achieved, but also imposes tighter timing requirements
        /// on the channel to produce its data and apply any DSP/FX (and run mixtime syncs) that are set on it;
        /// if too long is taken, there will be a break in the output, affecting all channels that are playing on the same device.
        /// </para>
        /// <para>
        /// The channel's data is still placed in its playback Buffer when this attribute is on,
        /// which allows <see cref="Bass.ChannelGetData(int,IntPtr,int)"/> and <see cref="Bass.ChannelGetLevel(int)"/> to be used, although there is
        /// likely to be less data available to them due to the Buffer being less full.
        /// </para>
        /// <para>This attribute can be changed mid-playback.</para>
        /// <para>If switched on, any already buffered data will still be played, so that there is no break in sound.</para>
        /// <para>This attribute is not available on Windows, as BASS does not generate the final mix.</para>
        /// </summary>
        NoBuffer = 0x5,

        /// <summary>
        /// The CPU usage of a channel. (in percentage).
        /// <para>
        /// This attribute gives the percentage of CPU that the channel is using,
        /// including the time taken by decoding and DSP processing, and any FX that are
        /// not using the "with FX flag" DX8 effect implementation.
        /// It does not include the time taken to add the channel's data to the final output mix during playback.
        /// The processing of some add-on stream formats may also not be entirely included,
        /// if they use additional decoding threads; see the add-on documentation for details.
        /// </para>
        /// <para>
        /// Like <see cref="Bass.CPUUsage"/>, this function does not strictly tell the CPU usage, but rather how timely the processing is.
        /// For example, if it takes 10ms to generate 100ms of data, that would be 10%.
        /// </para>
        /// <para>
        /// If the reported usage exceeds 100%, that means the channel's data is taking longer to generate than to play.
        /// The duration of the data is based on the channel's current sample rate (<see cref="ChannelAttribute.Frequency"/>).
        /// A channel's CPU usage is updated whenever it generates data.
        /// That could be during a playback Buffer update cycle, or a <see cref="Bass.Update"/> call, or a <see cref="Bass.ChannelUpdate"/> call.
        /// For a decoding channel, it would be in a <see cref="Bass.ChannelGetData(int,IntPtr,int)"/> or <see cref="Bass.ChannelGetLevel(int)"/> call.
        /// </para>
        /// <para>This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int, ChannelAttribute, float)"/>.</para>
        /// </summary>
        CPUUsage = 0x7,

        /// <summary>
        /// The sample rate conversion quality of a channel
        /// <para>
        /// 0 = linear interpolation, 1 = 8 point sinc interpolation, 2 = 16 point sinc interpolation, 3 = 32 point sinc interpolation.
        /// Other values are also accepted but will be interpreted as 0 or 3, depending on whether they are lower or higher.
        /// </para>
        /// <para>
        /// When a channel has a different sample rate to what the output device is using,
        /// the channel's sample data will need to be converted to match the output device's rate during playback.
        /// This attribute determines how that is done.
        /// The linear interpolation option uses less CPU, but the sinc interpolation gives better sound quality (less aliasing),
        /// with the quality and CPU usage increasing with the number of points.
        /// A good compromise for lower spec systems could be to use sinc interpolation for music playback and linear interpolation for sound effects.
        /// </para>
        /// <para>
        /// Whenever possible, a channel's sample rate should match the output device's rate to avoid the need for any sample rate conversion.
        /// The device's sample rate could be used in <see cref="Bass.CreateStream(int,int,BassFlags,StreamProcedure,IntPtr)" />
        /// or <see cref="Bass.MusicLoad(string,long,int,BassFlags,int)" /> or <see cref="Midi.BassMidi" /> stream creation calls, for example.
        /// </para>
        /// <para>
        /// The sample rate conversion occurs (when required) during playback,
        /// after the sample data has left the channel's playback Buffer, so it does not affect the data delivered by <see cref="Bass.ChannelGetData(int,IntPtr,int)" />.
        /// Although this attribute has no direct effect on decoding channels,
        /// it is still available so that it can be used by the <see cref="Mix.BassMix" /> add-on and anything else that wants to use it.
        /// </para>
        /// <para>
        /// This attribute can be set at any time, and changes take immediate effect.
        /// A channel's initial setting is determined by the <see cref="Bass.SRCQuality" /> config option,
        /// or <see cref="Bass.SampleSRCQuality" /> in the case of a sample channel.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>On Windows, sample rate conversion is handled by Windows or the output device/driver rather than BASS, so this setting has no effect on playback there.</para>
        /// </summary>
        SampleRateConversion = 0x8,

        /// <summary>
        /// The download Buffer level required to resume stalled playback in percent... 0 - 100 (the default is 50%).
        /// <para>
        /// This attribute determines what percentage of the download Buffer (<see cref="Bass.NetBufferLength"/>)
        /// needs to be filled before playback of a stalled internet stream will resume.
        /// It also applies to 'buffered' User file streams created with <see cref="Bass.CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)"/>.
        /// </para>
        /// </summary>
        NetworkResumeBufferLevel = 0x9,

        /// <summary>
        /// The scanned info of a channel.
        /// </summary>
        ScannedInfo = 0xa,

        #region MOD Music
        /// <summary>
        /// The amplification level of a MOD music... 0 (min) to 100 (max).
        /// <para>This will be rounded down to a whole number.</para>
        /// <para>
        /// As the amplification level get's higher, the sample data's range increases, and therefore, the resolution increases.
        /// But if the level is set too high, then clipping can occur, which can result in distortion of the sound.
        /// You can check the current level of a MOD music at any time by <see cref="Bass.ChannelGetLevel(int)"/>.
        /// By doing so, you can decide if a MOD music's amplification level needs adjusting.
        /// The default amplification level is 50.
        /// </para>
        /// <para>
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicAmplify = 0x100,

        /// <summary>
        /// The pan separation level of a MOD music... 0 (min) to 100 (max), 50 = linear.
        /// <para>
        /// This will be rounded down to a whole number.
        /// By default BASS uses a linear panning "curve".
        /// If you want to use the panning of FT2, use a pan separation setting of around 35.
        /// To use the Amiga panning (ie. full left and right) set it to 100.
        /// </para>
        /// </summary>
        MusicPanSeparation = 0x101,

        /// <summary>
        /// The position scaler of a MOD music... 1 (min) to 256 (max).
        /// <para>
        /// This will be rounded down to a whole number.
        /// When calling <see cref="Bass.ChannelGetPosition"/>, the row (HIWORD) will be scaled by this value.
        /// By using a higher scaler, you can get a more precise position indication.
        /// The default scaler is 1.
        /// </para>
        /// </summary>
        MusicPositionScaler = 0x102,

        /// <summary>
        /// The BPM of a MOD music... 1 (min) to 255 (max).
        /// <para>
        /// This will be rounded down to a whole number.
        /// This attribute is a direct mapping of the MOD's BPM, so the value can be changed via effects in the MOD itself.
        /// Note that by changing this attribute, you are changing the playback Length.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicBPM = 0x103,

        /// <summary>
        /// The speed of a MOD music... 0 (min) to 255 (max).
        /// <para>
        /// This will be rounded down to a whole number.
        /// This attribute is a direct mapping of the MOD's speed, so the value can be changed via effects in the MOD itself.
        /// The "speed" is the number of ticks per row.
        /// Setting it to 0, stops and ends the music.
        /// Note that by changing this attribute, you are changing the playback Length.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicSpeed = 0x104,

        /// <summary>
        /// The global volume level of a MOD music... 0 (min) to 64 (max, 128 for IT format).
        /// <para>
        /// This will be rounded down to a whole number.
        /// This attribute is a direct mapping of the MOD's global volume, so the value can be changed via effects in the MOD itself.
        /// The "speed" is the number of ticks per row.
        /// Setting it to 0, stops and ends the music.
        /// Note that by changing this attribute, you are changing the playback Length.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicVolumeGlobal = 0x105,

        /// <summary>
        /// The number of active channels in a MOD music.
        /// <para>
        /// This attribute gives the number of channels (including virtual) that are currently active in the decoder,
        /// which may not match what is being heard during playback due to buffering.
        /// To reduce the time difference, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)"/>.
        /// </para>
        /// </summary>
        MusicActiveChannelCount = 0x106,

        /// <summary>
        /// The volume level... 0 (silent) to 1 (full) of a channel in a MOD music + channel#.
        /// <para>channel: The channel to set the volume of... 0 = 1st channel.</para>
        /// <para>
        /// The volume curve used by this attribute is always linear, eg. 0.5 = 50%.
        /// The <see cref="Bass.LogarithmicVolumeCurve"/> config option setting has no effect on this.
        /// The volume level of all channels is initially 1 (full).
        /// This attribute can also be used to count the number of channels in a MOD Music.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicVolumeChannel = 0x200,

        /// <summary>
        /// The volume level... 0 (silent) to 1 (full) of an instrument in a MOD music + inst#.
        /// <para>inst: The instrument to set the volume of... 0 = 1st instrument.</para>
        /// <para>
        /// The volume curve used by this attribute is always linear, eg. 0.5 = 50%.
        /// The <see cref="Bass.LogarithmicVolumeCurve"/> config option setting has no effect on this.
        /// The volume level of all instruments is initially 1 (full).
        /// For MOD formats that do not use instruments, read "sample" for "instrument".
        /// This attribute can also be used to count the number of instruments in a MOD music.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicVolumeInstrument = 0x300,
        #endregion

        #region BassFx
        /// <summary>
        /// BassFx Tempo: The Tempo in percents (-95%..0..+5000%).
        /// </summary>
        Tempo = 0x10000,

        /// <summary>
        /// BassFx Tempo: The Pitch in semitones (-60..0..+60).
        /// </summary>
        Pitch = 0x10001,

        /// <summary>
        /// BassFx Tempo: The Samplerate in Hz, but calculates by the same % as <see cref="Tempo"/>.
        /// </summary>
        TempoFrequency = 0x10002,

        /// <summary>
        /// BassFx Tempo Option: Use FIR low-pass (anti-alias) filter (gain speed, lose quality)? true=1 (default), false=0.
        /// <para>See <see cref="Fx.BassFx.TempoCreate"/> for details.</para>
        /// <para>On iOS, Android, WinCE and Linux ARM platforms this is by default disabled for lower CPU usage.</para>
        /// </summary>
        TempoUseAAFilter = 0x10010,

        /// <summary>
        /// BassFx Tempo Option: FIR low-pass (anti-alias) filter Length in taps (8 .. 128 taps, default = 32, should be %4).
        /// <para>See <see cref="Fx.BassFx.TempoCreate"/> for details.</para>
        /// </summary>
        TempoAAFilterLength = 0x10011,

        /// <summary>
        /// BassFx Tempo Option: Use quicker tempo change algorithm (gain speed, lose quality)? true=1, false=0 (default).
        /// <para>See <see cref="Fx.BassFx.TempoCreate"/> for details.</para>
        /// </summary>
        TempoUseQuickAlgorithm = 0x10012,

        /// <summary>
        /// BassFx Tempo Option: Tempo Sequence in milliseconds (default = 82).
        /// <para>See <see cref="Fx.BassFx.TempoCreate"/> for details.</para>
        /// </summary>
        TempoSequenceMilliseconds = 0x10013,

        /// <summary>
        /// BassFx Tempo Option: SeekWindow in milliseconds (default = 14).
        /// <para>See <see cref="Fx.BassFx.TempoCreate"/> for details.</para>
        /// </summary>
        TempoSeekWindowMilliseconds = 0x10014,

        /// <summary>
        /// BassFx Tempo Option: Tempo Overlap in milliseconds (default = 12).
        /// <para>See <see cref="Fx.BassFx.TempoCreate"/> for details.</para>
        /// </summary>
        TempoOverlapMilliseconds = 0x10015,

        /// <summary>
        /// BassFx Tempo Option: Prevents clicks with tempo changes (default = FALSE).
        /// <para>See <see cref="Fx.BassFx.TempoCreate"/> for details.</para>
        /// </summary>
        TempoPreventClick = 0x10016,

        /// <summary>
        /// Playback direction (-1 = Reverse or 1 = Forward).
        /// </summary>
        ReverseDirection = 0x11000,
        #endregion

        #region BassMidi
        /// <summary>
        /// BASSMIDI: Gets the Pulses Per Quarter Note (or ticks per beat) value of the MIDI file.
        /// <para>
        /// This attribute is the number of ticks per beat as defined by the MIDI file;
        /// it will be 0 for MIDI streams created via <see cref="Midi.BassMidi.CreateStream(int,BassFlags,int)"/>,
        /// It is read-only, so can't be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)"/>.
        /// </para>
        /// </summary>
        MidiPPQN = 0x12000,

        /// <summary>
        /// BASSMIDI: The maximum percentage of CPU time that a MIDI stream can use... 0 to 100, 0 = unlimited.
        /// <para>
        /// It is not strictly the CPU usage that is measured, but rather how timely the stream is able to render data.
        /// For example, a limit of 50% would mean that the rendering would need to be at least 2x real-time speed.
        /// When the limit is exceeded, <see cref="Midi.BassMidi"/> will begin killing voices, starting with the  most quiet.
        /// When the CPU usage is limited, the stream's samples are loaded asynchronously
        /// so that any loading delays (eg. due to slow disk) do not hold up the stream for too long.
        /// If a sample cannot be loaded in time, then it will be silenced
        /// until it is available and the stream will continue playing other samples as normal in the meantime.
        /// This does not affect sample loading via <see cref="Midi.BassMidi.StreamLoadSamples"/>, which always operates synchronously.
        /// By default, a MIDI stream will have no CPU limit.
        /// </para>
        /// </summary>
        MidiCPU = 0x12001,

        /// <summary>
        /// BASSMIDI: The number of MIDI channels in a MIDI stream... 1 (min) - 128 (max).
        /// <para>
        /// For a MIDI file stream, the minimum is 16.
        /// New channels are melodic by default.
        /// Any notes playing on a removed channel are immediately stopped.
        /// </para>
        /// </summary>
        MidiChannels = 0x12002,

        /// <summary>
        /// BASSMIDI: The maximum number of samples to play at a time (polyphony) in a MIDI stream... 1 (min) - 1000 (max).
        /// <para>
        /// If there are currently more voices active than the new limit, then some voices will be killed to meet the limit.
        /// The number of voices currently active is available via the Voices attribute.
        /// A MIDI stream will initially have a default number of voices as determined by the Voices config option.
        /// </para>
        /// </summary>
        MidiVoices = 0x12003,

        /// <summary>
        /// BASSMIDI: The number of samples (voices) currently playing in a MIDI stream.
        /// <para>This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)"/>.</para>
        /// </summary>
        MidiVoicesActive = 0x12004,

        /// <summary>
        /// BASSMIDI: The current state of a MIDI stream.
        /// </summary>
        MidiState = 0x12005,

        /// <summary>
        /// BASSMIDI: The sample rate conversion quality of a MIDI stream's samples.
        /// </summary>
        MidiSRC = 0x12006,

        MidiKill = 0x12007,

        /// <summary>
        /// BASSMIDI: The volume level (0.0=silent, 1.0=normal/default) of a track in a MIDI file stream + track#.
        /// <para>track#: The track to set the volume of... 0 = first track.</para>
        /// <para>
        /// The volume curve used by this attribute is always linear, eg. 0.5 = 50%.
        /// The <see cref="Bass.LogarithmicVolumeCurve"/> config option setting has no effect on this.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// This attribute can also be used to count the number of tracks in a MIDI file stream.
        /// MIDI streams created via <see cref="Midi.BassMidi.CreateStream(int,BassFlags,int)"/> do not have any tracks.
        /// </para>
        /// </summary>
        MidiTrackVolume = 0x12100,
        #endregion

        /// <summary>
        /// BassOpus: The sample rate of an Opus stream's source material.
        /// <para>
        /// Opus streams always have a sample rate of 48000 Hz, and an Opus encoder will resample the source material to that if necessary.
        /// This attribute presents the original sample rate, which may be stored in the Opus file header.
        /// This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)" />.
        /// </para>
        /// </summary>
        OpusOriginalFrequency = 0x13000,

        /// <summary>
        /// BassDSD: The gain (in decibels) applied when converting to PCM.
        /// </summary>
        /// <remarks>
        /// This attribute is only applicable when converting to PCM, and is unavailable when producing DSD-over-PCM or raw DSD data.
        /// The default setting is determined by the <see cref="DSDGain" /> config option
        /// </remarks>
        DSDGain = 0x14000,

        /// <summary>
        /// BassDSD: The DSD sample rate.
        /// </summary>
        /// <remarks>This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)" />.</remarks>
        DSDRate = 0x14001,

        /// <summary>
        /// BassMix: Custom output latency in seconds... default = 0 (no accounting for latency). Changes take immediate effect.
        /// </summary>
        /// <remarks>
        /// When a mixer is played by BASS, the <see cref="Mix.BassMix.ChannelGetData(int,IntPtr,int)"/>, <see cref="Mix.BassMix.ChannelGetLevel(int)"/>, <see cref="Mix.BassMix.ChannelGetLevel(int,float[],float,LevelRetrievalFlags)"/>, and <see cref="Mix.BassMix.ChannelGetPosition(int,PositionFlags)"/> functions will get the output latency and account for that so that they reflect what is currently being heard, but that cannot be done when a different output system is used, eg. ASIO or WASAPI.
        /// In that case, this attribute can be used to tell the mixer what the output latency is, so that those functions can still account for it.
        /// The mixer needs to have the <see cref="BassFlags.Decode"/> and <see cref="BassFlags.MixerPositionEx"/> flags set to use this attribute. 
        /// </remarks>
        MixerLatency = 0x15000,

        /// <summary>
        /// The average bitrate of a file stream. 
        /// </summary>
        Bitrate = 0xc,

        /// <summary>
        /// Disable playback ramping? 
        /// </summary>
        NoRamp = 0xb,

        /// <summary>
        /// Amount of data to asynchronously buffer from a splitter's source.
        /// 0 = disable asynchronous buffering. The asynchronous buffering will be limited to the splitter's buffer length.
        /// </summary>
        SplitAsyncBuffer = 0x15010,

        /// <summary>
        /// Maximum amount of data to asynchronously buffer at a time from a splitter's source.
        /// 0 = as much as possible.
        /// </summary>
        SplitAsyncPeriod = 0x15011
    }

    /// <summary>
    /// Initialization flags to be used with <see cref="Bass.Init" />
    /// </summary>
    [Flags]
    internal enum DeviceInitFlags
    {
        /// <summary>
        /// 0 = 16 bit, stereo, no 3D, no Latency calc, no Speaker Assignments
        /// </summary>
        Default,

        /// <summary>
        /// Use 8 bit resolution, else 16 bit.
        /// </summary>
        Byte = 0x1,

        /// <summary>
        /// Use mono, else stereo.
        /// </summary>
        Mono = 0x2,

        /// <summary>
        /// Enable 3D functionality.
        /// Note: If this is not specified when initilizing BASS,
        /// then the <see cref="BassFlags.Bass3D"/> is ignored when loading/creating a sample/stream/music.
        /// </summary>
        Device3D = 0x4,

        /// <summary>
        /// Calculate device latency (<see cref="BassInfo"/> struct).
        /// </summary>
        Latency = 0x100,

        /// <summary>
        /// Use the Windows control panel setting to detect the number of speakers.
        /// Only use this option if BASS doesn't detect the correct number of supported
        /// speakers automatically and you want to force BASS to use the number of speakers
        /// as configured in the windows control panel.
        /// </summary>
        CPSpeakers = 0x400,

        /// <summary>
        /// Force enabling of speaker assignment (always 8 speakers will be used regardless if the soundcard supports them).
        /// Only use this option if BASS doesn't detect the correct number of supported
        /// speakers automatically and you want to force BASS to use 8 speakers.
        /// </summary>
        ForcedSpeakerAssignment = 0x800,

        /// <summary>
        /// Ignore speaker arrangement
        /// </summary>
        NoSpeakerAssignment = 0x1000,

        /// <summary>
        /// Linux-only: Initialize the device using the ALSA "dmix" plugin, else initialize the device for exclusive access.
        /// </summary>
        DMix = 0x2000,

        /// <summary>
        /// Set the device's output rate to freq, otherwise leave it as it is.
        /// </summary>
        Frequency = 0x4000
    }

    internal static class Bass
    {
        private const string DllName = "bass";

        /// <summary>
        /// Index of No Sound Device.
        /// </summary>
        public const int NoSoundDevice = 0;

        /// <summary>
        /// Index of Default Device.
        /// </summary>
        public const int DefaultDevice = -1;

        /// <summary>
        /// Initializes an output device.
        /// </summary>
        /// <param name="Device">The device to use... -1 = default device, 0 = no sound, 1 = first real output device.
        /// <see cref="GetDeviceInfo(int,out DeviceInfo)" /> or <see cref="DeviceCount" /> can be used to get the total number of devices.
        /// </param>
        /// <param name="Frequency">Output sample rate.</param>
        /// <param name="Flags">Any combination of <see cref="DeviceInitFlags"/>.</param>
        /// <param name="Win">The application's main window... <see cref="IntPtr.Zero" /> = the desktop window (use this for console applications).</param>
        /// <param name="ClsID">Class identifier of the object to create, that will be used to initialize DirectSound... <see langword="null" /> = use default</param>
        /// <returns>If the device was successfully initialized, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        /// <exception cref="Errors.Already">The device has already been initialized. You must call <see cref="Free()" /> before you can initialize it again.</exception>
        /// <exception cref="Errors.Driver">There is no available device driver... the device may already be in use.</exception>
        /// <exception cref="Errors.SampleFormat">The specified format is not supported by the device. Try changing the <paramref name="Frequency" /> and <paramref name="Flags" /> parameters.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">The device has no 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        /// <remarks>
        /// <para>This function must be successfully called before using any sample, stream or MOD music functions. The recording functions may be used without having called this function.</para>
        /// <para>Playback is not possible with the <see cref="NoSoundDevice"/> device, but it does allow the use of "decoding channels", eg. to decode files.</para>
        /// <para>When specifying a class identifier (<paramref name="ClsID"/>), after successful initialization, you can use GetDSoundObject(DSInterface) to retrieve the DirectSound object, and through that access any special interfaces that the object may provide.</para>
        /// <para>
        /// Simultaneously using multiple devices is supported in the BASS API via a context switching system - instead of there being an extra "device" parameter in the function calls, the device to be used is set prior to calling the functions. <see cref="CurrentDevice" /> is used to switch the current device.
        /// When successful, <see cref="Init"/> automatically sets the current thread's device to the one that was just initialized.
        /// </para>
        /// <para>
        /// When using the default device (device = -1), <see cref="CurrentDevice" /> can be used to find out which device it was mapped to.
        /// On Windows, it'll always be the first device.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Linux, a 'Default' device is hardcoded to device number 1, which uses the default output set in the ALSA config; that could map directly to one of the other devices or it could use ALSA plugins.
        /// If the IncludeDefaultDevice config option has been enbled, a "Default" device is also available on Windows, who's output will follow default device changes on Windows 7.
        /// In both cases, the "Default" device will also be the default device (device = -1).
        /// </para>
        /// <para>
        /// The sample format specified in the <paramref name="Frequency" /> and <paramref name="Flags" /> parameters has no effect on the device output on iOS or OSX, and not on Windows unless VxD drivers are used (on Windows 98/95);
        /// with WDM drivers (on Windows XP/2000/Me/98SE), the output format is automatically set depending on the format of what is played and what the device supports, while on Vista and above, the output format is determined by the user's choice in the Sound control panel.
        /// On Linux the output device will use the specified format if possible, but will otherwise use a format as close to it as possible.
        /// If the <see cref="DeviceInitFlags.Frequency"/> flag is specified on iOS or OSX, then the device's output rate will be set to the freq parameter (if possible).
        /// The <see cref="DeviceInitFlags.Frequency"/> flag has no effect on other platforms.
        /// <see cref="GetInfo" /> can be used to check what the output format actually is.
        /// </para>
        /// <para>
        /// The <paramref name="Win" /> and <paramref name="ClsID" /> parameters are only used on Windows and are ignored on other platforms.
        /// That applies to the <see cref="DeviceInitFlags.CPSpeakers"/> and <see cref="DeviceInitFlags.ForcedSpeakerAssignment"/> flags too, as the number of available speakers is always accurately detected on the other platforms.
        /// The <see cref="DeviceInitFlags.Latency"/> flag is also ignored on Linux/OSX/Android/Windows CE, as latency information is available without it.
        /// The latency is also available without it on iOS, but not immediately following this function call unless the flag is used.
        /// </para>
        /// <para>
        /// The DeviceInitFlags.DMix flag is only available on Linux, and allows multiple applications to share the device (if they all use 'dmix').
        /// It may also be possible for multiple applications to use exclusive access if the device is capable of hardware mixing.
        /// If exclusive access initialization fails, the DeviceInitFlags.DMix flag will automatically be tried;
        /// if that happens, it can be detected via <see cref="GetInfo" /> and the <see cref="BassInfo.InitFlags"/>.
        /// </para>
        /// <para>On Linux and Windows CE, the length of the device's buffer can be set via the <see cref="PlaybackBufferLength" /> config option.</para>
        /// </remarks>
        /// <seealso cref="Free()"/>
        /// <seealso cref="CPUUsage"/>
        /// <seealso cref="GetDeviceInfo(int, out DeviceInfo)"/>
        /// <seealso cref="GetInfo(out BassInfo)"/>
        /// <seealso cref="MusicLoad(string, long, int, BassFlags, int)"/>
        /// <seealso cref="CreateSample"/>
        /// <seealso cref="SampleLoad(string, long, int, int, BassFlags)"/>
        [DllImport(DllName, EntryPoint = "BASS_Init")]
        public static extern bool Init(int Device = DefaultDevice, int Frequency = 44100, DeviceInitFlags Flags = DeviceInitFlags.Default, IntPtr Win = default(IntPtr), IntPtr ClsID = default(IntPtr));

        /// <summary>
        /// Starts (or resumes) the output.
        /// </summary>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Init"><see cref="Init"/> has not been successfully called.</exception>
        /// <remarks>
        /// <para>The output is automatically started by <see cref="Init"/>, so there is no need to use this function unless you have stopped or paused the output.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice"/>) determines which device this function call applies to.</para>
        /// </remarks>
        /// <seealso cref="Pause"/>
        /// <seealso cref="Stop"/>
        [DllImport(DllName, EntryPoint = "BASS_Start")]
        public static extern bool Start();

        /// <summary>
        /// Stops the output, pausing all musics/samples/streams.
        /// </summary>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>Use <see cref="Start" /> to resume the output and paused channels.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Pause")]
        public static extern bool Pause();

        /// <summary>
        /// Stops the output, stopping all musics/samples/streams.
        /// </summary>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>This function can be used after <see cref="Pause" /> to stop the paused channels, so that they will not be resumed the next time <see cref="Start" /> is called.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Stop")]
        public static extern bool Stop();

        /// <summary>
        /// Frees all resources used by the output device, including all it's samples, streams, and MOD musics.
        /// </summary>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>This function should be called for all initialized devices before your program exits. It's not necessary to individually free the samples/streams/musics as these are all automatically freed by this function.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Free")]
        public static extern bool Free();

        /// <summary>
        /// Retrieves the device that the channel is using.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD. HSAMPLE handles may also be used.</param>
        /// <returns>If successful, the device number is returned, else -1 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Recording devices are indicated by the HIWORD of the return value being 1, when this function is called with a HRECORD channel.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetDevice")]
        public static extern int ChannelGetDevice(int Handle);

        /// <summary>
        /// Changes the device that a stream, MOD music or sample is using.
        /// </summary>
        /// <param name="Handle">The channel or sample handle... only HMUSIC, HSTREAM or HSAMPLE are supported.</param>
        /// <param name="Device">The device to use...0 = no sound, 1 = first real output device.</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// All of the channel's current settings are carried over to the new device, but if the channel is using the "with FX flag" DX8 effect implementation,
        /// the internal state (eg. buffers) of the DX8 effects will be reset. Using the "without FX flag" DX8 effect implementation, the state of the DX8 effects is preserved.
        /// <para>
        /// When changing a sample's device, all the sample's existing channels (HCHANNELs) are freed.
        /// It's not possible to change the device of an individual sample channel.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Device"><paramref name="Device" /> is invalid.</exception>
        /// <exception cref="Errors.Init">The requested device has not been initialized.</exception>
        /// <exception cref="Errors.Already">The channel is already using the requested device.</exception>
        /// <exception cref="Errors.NotAvailable">Only decoding channels are allowed to use the <see cref="NoSoundDevice"/> device.</exception>
        /// <exception cref="Errors.SampleFormat">
        /// The sample format is not supported by the device/drivers.
        /// If the channel is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.
        /// </exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSetDevice")]
        public static extern bool ChannelSetDevice(int Handle, int Device);

        #region Current Device Volume
        [DllImport(DllName)]
        static extern float BASS_GetVolume();

        [DllImport(DllName)]
        static extern bool BASS_SetVolume(float volume);

        /// <summary>
        /// Gets or sets the current output master volume level... 0 (silent) to 1 (max).
        /// </summary>
        /// <remarks>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// <para>A return value of -1 indicates error. Use <see cref="LastError" /> to get the error code. Throws <see cref="BassException"/> on Error while setting value.</para>
        /// <para>The actual volume level may not be exactly the same as set, due to underlying precision differences.</para>
        /// <para>
        /// This function affects the volume level of all applications using the same output device.
        /// If you wish to only affect the level of your app's sounds, <see cref="ChannelSetAttribute(int, ChannelAttribute, float)" />
        /// and/or the <see cref="GlobalMusicVolume"/>, <see cref="GlobalSampleVolume"/> and <see cref="GlobalStreamVolume"/> config options should be used instead.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">There is no volume control when using the <see cref="NoSoundDevice">No Sound Device</see>.</exception>
        /// <exception cref="Errors.Parameter">Invalid volume.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static double Volume
        {
            get => BASS_GetVolume();
            set
            {
                if (!BASS_SetVolume((float)value))
                    throw new Exception();
            }
        }
        #endregion

          /// <summary>
        /// Starts (or resumes) playback of a sample, stream, MOD music, or recording.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL / HMUSIC / HSTREAM / HRECORD Handle.</param>
        /// <param name="Restart">
        /// Restart playback from the beginning? If Handle is a User stream, it's current Buffer contents are flushed.
        /// If it's a MOD music, it's BPM/etc are automatically reset to their initial values.
        /// </param>
        /// <returns>
        /// If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Start">The output is paused/stopped, use <see cref="Start" /> to start it.</exception>
        /// <exception cref="Errors.Decode">The channel is not playable, it's a "decoding channel".</exception>
        /// <exception cref="Errors.BufferLost">Should not happen... check that a valid window Handle was used with <see cref="Init"/>.</exception>
        /// <exception cref="Errors.NoHW">
        /// No hardware voices are available (HCHANNEL only).
        /// This only occurs if the sample was loaded/created with the <see cref="BassFlags.VAM"/> flag,
        /// and <see cref="VAMMode.Hardware"/> is set in the sample's VAM mode,
        /// and there are no hardware voices available to play it.
        /// </exception>
        /// <remarks>
        /// When streaming in blocks (<see cref="BassFlags.StreamDownloadBlocks"/>), the restart parameter is ignored as it's not possible to go back to the start.
        /// The <paramref name="Restart" /> parameter is also of no consequence with recording channels.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelPlay")]
        public static extern bool ChannelPlay(int Handle, bool Restart = false);

        /// <summary>
        /// Pauses a sample, stream, MOD music, or recording.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL / HMUSIC / HSTREAM / HRECORD Handle.</param>
        /// <returns>
        /// If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.NotPlaying">The channel is not playing (or <paramref name="Handle" /> is not a valid channel).</exception>
        /// <exception cref="Errors.Decode">The channel is not playable, it's a "decoding channel".</exception>
        /// <exception cref="Errors.Already">The channel is already paused.</exception>
        /// <remarks>
        /// Use <see cref="ChannelPlay" /> to resume a paused channel.
        /// <see cref="ChannelStop" /> can be used to stop a paused channel.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelPause")]
        public static extern bool ChannelPause(int Handle);

        /// <summary>
        /// Stops a sample, stream, MOD music, or recording.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM or HRECORD Handle.</param>
        /// <returns>
        /// If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <remarks>
        /// <para>
        /// Stopping a User stream (created with <see cref="CreateStream(int,int,BassFlags,StreamProcedure,IntPtr)" />) will clear its Buffer contents,
        /// and stopping a sample channel (HCHANNEL) will result in it being freed.
        /// Use <see cref="ChannelPause" /> instead if you wish to stop a User stream and then resume it from the same point.
        /// </para>
        /// <para>
        /// When used with a "decoding channel" (<see cref="BassFlags.Decode"/> was used at creation),
        /// this function will end the channel at its current position, so that it's not possible to decode any more data from it.
        /// Any <see cref="SyncFlags.End"/> syncs that have been set on the channel will not be triggered by this, they are only triggered when reaching the natural end.
        /// <see cref="ChannelSetPosition" /> can be used to reset the channel and start decoding again.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelStop")]
        public static extern bool ChannelStop(int Handle);

        /// <summary>
        /// Locks a stream, MOD music or recording channel to the current thread.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HMUSIC, HSTREAM or HRECORD Handle.</param>
        /// <param name="Lock">If <see langword="false" />, unlock the channel, else lock it.</param>
        /// <returns>
        /// If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <remarks>
        /// Locking a channel prevents other threads from performing most functions on it, including Buffer updates.
        /// Other threads wanting to access a locked channel will block until it is unlocked, so a channel should only be locked very briefly.
        /// A channel must be unlocked in the same thread that it was locked.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelLock")]
        public static extern bool ChannelLock(int Handle, bool Lock = true);

        #region Channel Attributes
        /// <summary>
        /// Retrieves the value of an attribute of a sample, stream or MOD music.
        /// Can also get the sample rate of a recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of (one of <see cref="ChannelAttribute" />)</param>
        /// <param name="Value">Reference to a float to receive the attribute value.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetAttribute")]
        public static extern bool ChannelGetAttribute(int Handle, ChannelAttribute Attribute, out float Value);

        /// <summary>
        /// Retrieves the value of an attribute of a sample, stream or MOD music.
        /// Can also get the sample rate of a recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of (one of <see cref="ChannelAttribute" />)</param>
        /// <returns>If successful, the attribute value is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        public static double ChannelGetAttribute(int Handle, ChannelAttribute Attribute)
        {
            ChannelGetAttribute(Handle, Attribute, out float temp);
            return temp;
        }

        /// <summary>
        /// Retrieves the value of a channel's attribute.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM  or HRECORD.</param>
        /// <param name="Attribute">The attribute to get the value of (e.g. <see cref="ChannelAttribute.ScannedInfo"/>)</param>
        /// <param name="Value">Pointer to a buffer to receive the attribute data.</param>
        /// <param name="Size">The size of the attribute data... 0 = get the size of the attribute without getting the data.</param>
        /// <returns>If successful, the size of the attribute data is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>This function also supports the floating-point attributes supported by <see cref="ChannelGetAttribute(int, ChannelAttribute, out float)" />.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.NotAvailable">The <paramref name="Attribute" /> is not available.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Value" /> content or <paramref name="Size" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetAttributeEx")]
        public static extern int ChannelGetAttribute(int Handle, ChannelAttribute Attribute, IntPtr Value, int Size);

        /// <summary>
        /// Sets the value of an attribute of a sample, stream or MOD music.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM  or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of.</param>
        /// <param name="Value">The new attribute value. See the attribute's documentation for details on the possible values.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The actual attribute value may not be exactly the same as requested, due to precision differences.
        /// For example, an attribute might only allow whole number values.
        /// <see cref="ChannelGetAttribute(int, ChannelAttribute, out float)" /> can be used to confirm what the value is.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Value" /> is not valid. See the attribute's documentation for the valid range of values.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSetAttribute")]
        public static extern bool ChannelSetAttribute(int Handle, ChannelAttribute Attribute, float Value);

        /// <summary>
        /// Sets the value of an attribute of a sample, stream or MOD music.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM  or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of.</param>
        /// <param name="Value">The new attribute value. See the attribute's documentation for details on the possible values.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The actual attribute value may not be exactly the same as requested, due to precision differences.
        /// For example, an attribute might only allow whole number values.
        /// <see cref="ChannelGetAttribute(int, ChannelAttribute)" /> can be used to confirm what the value is.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Value" /> is not valid. See the attribute's documentation for the valid range of values.</exception>
        public static bool ChannelSetAttribute(int Handle, ChannelAttribute Attribute, double Value)
        {
            return ChannelSetAttribute(Handle, Attribute, (float)Value);
        }

        /// <summary>
        /// Sets the value of a channel's attribute.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM  or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of. (e.g. <see cref="ChannelAttribute.ScannedInfo"/>)</param>
        /// <param name="Value">The pointer to the new attribute data.</param>
        /// <param name="Size">The size of the attribute data.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Value" /> is not valid. See the attribute's documentation for the valid range of values.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSetAttributeEx")]
        public static extern bool ChannelSetAttribute(int Handle, ChannelAttribute Attribute, IntPtr Value, int Size);
        #endregion

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_StreamCreateURL(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User);
        
        /// <summary>
        /// Creates a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plugin supported file on the internet, optionally receiving the downloaded data in a callback.
        /// </summary>
        /// <param name="Url">
        /// URL of the file to stream.
        /// Should begin with "http://", "https://" or "ftp://", or another add-on supported protocol.
        /// The URL can be followed by custom HTTP request headers to be sent to the server;
        /// the URL and each header should be terminated with a carriage return and line feed ("\r\n").
        /// </param>
        /// <param name="Offset">File position to start streaming from. This is ignored by some servers, specifically when the file length is unknown, for example a Shout/Icecast server.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags" /></param>
        /// <param name="Procedure">Callback function to receive the file as it is downloaded... <see langword="null" /> = no callback.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// Use <see cref="ChannelGetInfo(int, out ChannelInfo)" /> to retrieve information on the format (sample rate, resolution, channels) of the stream.
        /// The playback length of the stream can be retrieved using <see cref="ChannelGetLength(int, PositionFlags)" />.
        /// </para>
        /// <para>
        /// When playing the stream, BASS will stall the playback if there is insufficient data to continue playing.
        /// Playback will automatically be resumed when sufficient data has been downloaded.
        /// <see cref="ChannelIsActive" /> can be used to check if the playback is stalled, and the progress of the file download can be checked with <see cref="StreamGetFilePosition" />.
        /// </para>
        /// <para>When streaming in blocks (<see cref="BassFlags.StreamDownloadBlocks"/>), be careful not to stop/pause the stream for too long, otherwise the connection may timeout due to there being no activity and the stream will end prematurely.</para>
        /// <para>
        /// When streaming from Shoutcast servers, metadata (track titles) may be sent by the server.
        /// The data can be retrieved with <see cref="ChannelGetTags" />.
        /// A sync can also be set (using <see cref="ChannelSetSync" />) so that you are informed when metadata is received.
        /// A <see cref="SyncFlags.OggChange"/> sync can be used to be informed of when a new logical bitstream begins in an Icecast/OGG stream.
        /// </para>
        /// <para>
        /// When using an <paramref name="Offset" />, the file length returned by <see cref="StreamGetFilePosition" /> can be used to check that it was successful by comparing it with the original file length.
        /// Another way to check is to inspect the HTTP headers retrieved with <see cref="ChannelGetTags" />.
        /// </para>
        /// <para>Custom HTTP request headers may be ignored by some plugins, notably BassWma.</para>
        /// <para>
        /// Unlike Bass.Net, a reference to <paramref name="Procedure"/> doesn't need to be held by you manually.
        /// ManagedBass automatically holds a reference and frees it when the Channel is freed.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Windows and Windows CE, ACM codecs are supported with compressed WAV files.
        /// Media Foundation codecs are also supported on Windows 7 and updated versions of Vista, including support for AAC and WMA.
        /// On iOS and OSX, CoreAudio codecs are supported, including support for AAC and ALAC.
        /// Media Foundation and CoreAudio codecs are only tried after the built-in decoders and any plugins have rejected the file.
        /// Built-in support for IMA and Microsoft ADPCM WAV files is provided on Linux/Android/Windows CE, while they are supported via ACM and CoreAudio codecs on Windows and OSX/iOS.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Only decoding channels (<see cref="BassFlags.Decode"/>) are allowed when using the <see cref="NoSoundDevice"/> device. The <see cref="BassFlags.AutoFree"/> flag is also unavailable to decoding channels.</exception>
        /// <exception cref="Errors.NoInternet">No internet connection could be opened. Can be caused by a bad proxy setting.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Url" /> is not a valid URL.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with <see cref="NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.FileOpen">The file could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The file's format is not recognised/supported.</exception>
        /// <exception cref="Errors.Codec">The file uses a codec that's not available/supported. This can apply to WAV and AIFF files, and also MP3 files when using the "MP3-free" BASS version.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified Speaker flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int CreateStream(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User = default(IntPtr))
        {
            var h = BASS_StreamCreateURL(Url, Offset, Flags | BassFlags.Unicode, Procedure, User);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedure);

            return h;
        }

        [DllImport(DllName)]
        static extern int BASS_ChannelSetSync(int Handle, SyncFlags Type, long Parameter, SyncProcedure Procedure, IntPtr User);

          /// <summary>
        /// Sets up a synchronizer on a MOD music, stream or recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HMUSIC, HSTREAM or HRECORD.</param>
        /// <param name="Type">The Type of sync (see <see cref="SyncFlags" />).</param>
        /// <param name="Parameter">The sync parameters, depends on the sync Type (see <see cref="SyncFlags"/>).</param>
        /// <param name="Procedure">The callback function which should be invoked with the sync.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>
        /// If succesful, then the new synchronizer's Handle is returned, else 0 is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type">An illegal <paramref name="Type" /> was specified.</exception>
        /// <exception cref="Errors.Parameter">An illegal <paramref name="Parameter" /> was specified.</exception>
        /// <remarks>
        /// <para>
        /// Multiple synchronizers may be used per channel, and they can be set before and while playing.
        /// Equally, synchronizers can also be removed at any time, using <see cref="ChannelRemoveSync" />.
        /// If the <see cref="SyncFlags.Onetime"/> flag is used, then the sync is automatically removed after its first occurrence.
        /// </para>
        /// <para>The <see cref="SyncFlags.Mixtime"/> flag can be used with <see cref="SyncFlags.End"/> or <see cref="SyncFlags.Position"/>/<see cref="SyncFlags.MusicPosition"/> syncs to implement custom looping, by using <see cref="ChannelSetPosition" /> in the callback.
        /// A <see cref="SyncFlags.Mixtime"/> sync can also be used to add or remove DSP/FX at specific points, or change a HMUSIC channel's flags or attributes (see <see cref="ChannelFlags" />).
        /// The <see cref="SyncFlags.Mixtime"/> flag can also be useful with a <see cref="SyncFlags.Seeking"/> sync, to reset DSP states after seeking.</para>
        /// <para>
        /// Several of the sync types are triggered in the process of rendering the channel's sample data;
        /// for example, <see cref="SyncFlags.Position"/> and <see cref="SyncFlags.End"/> syncs, when the rendering reaches the sync position or the end, respectively.
        /// Those sync types should be set before starting playback or pre-buffering (ie. before any rendering), to avoid missing any early sync events.
        /// </para>
        /// <para>With recording channels, <see cref="SyncFlags.Position"/> syncs are triggered just before the <see cref="RecordProcedure" /> receives the block of data containing the sync position.</para>
        /// <para>
        /// Unlike Bass.Net, a reference to <paramref name="Procedure"/> doesn't need to be held by you manually.
        /// ManagedBass automatically holds a reference and frees it when the Channel is freed or Sync is removed via <see cref="ChannelRemoveSync"/>.
        /// </para>
        /// </remarks>
        public static int ChannelSetSync(int Handle, SyncFlags Type, long Parameter, SyncProcedure Procedure, IntPtr User = default(IntPtr))
        {
            // Define a dummy SyncProcedure for OneTime syncs.
            var proc = Type.HasFlag(SyncFlags.Onetime)
                ? ((I, Channel, Data, Ptr) =>
                {
                    Procedure(I, Channel, Data, Ptr);
                    ChannelReferences.Remove(Channel, I);
                }) : Procedure;

            var h = BASS_ChannelSetSync(Handle, Type, Parameter, proc, User);

            if (h != 0)
                ChannelReferences.Add(Handle, h, proc);

            return h;
        }
    }

    /// <summary>
    /// Holds References to Channel Items like <see cref="SyncProcedure"/> and <see cref="FileProcedures"/>.
    /// </summary>
    internal static class ChannelReferences
    {
#if !__IOS__
        static readonly ConcurrentDictionary<Tuple<int, int>, object> Procedures = new ConcurrentDictionary<Tuple<int, int>, object>();
        static readonly SyncProcedure Freeproc = Callback;
#endif

        /// <summary>
        /// Adds a Reference.
        /// </summary>
        public static void Add(int Handle, int SpecificHandle, object proc)
        {
#if !__IOS__
            if (proc == null)
                return;

            if (proc.Equals(Freeproc))
                return;

            var key = Tuple.Create(Handle, SpecificHandle);

            var contains = Procedures.ContainsKey(key);
            
            if (Freeproc != null && Procedures.All(pair => pair.Key.Item1 != Handle))
                Bass.ChannelSetSync(Handle, SyncFlags.Free, 0, Freeproc);

            if (contains)
                Procedures[key] = proc;
            else Procedures.TryAdd(key, proc);
#endif
        }

        /// <summary>
        /// Removes a Reference.
        /// </summary>
        public static void Remove(int Handle, int SpecialHandle)
        {
#if !__IOS__
            var key = Tuple.Create(Handle, SpecialHandle);
            Procedures.TryRemove(key, out object unused);
#endif
        }

#if !__IOS__
        static void Callback(int Handle, int Channel, int Data, IntPtr User)
        {
            // ToArray is necessary because the object iterated on should not be modified.
            var toRemove = Procedures.Where(Pair => Pair.Key.Item1 == Channel).Select(Pair => Pair.Key).ToArray();
            
            foreach (var key in toRemove)
                Procedures.TryRemove(key, out object unused);
        }
#endif
    }
}