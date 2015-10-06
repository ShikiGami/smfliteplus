//
// SmfLitePlus.cs - A minimal toolkit for handling standard MIDI files (SMF) on Unity
//
// Copyright (C) 2015 Shiki Byakko
// Copyright (C) 2013 Keijiro Takahashi (The original SmfLite)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Collections.Generic;

namespace SmfLitePlus
{
    // An alias for internal use.
    using DeltaEventPairList = System.Collections.Generic.List<SmfLitePlus.MidiTrack.DeltaEventPair>;

    /// <summary>
    /// MIDI event
    /// </summary>
    public struct MidiEvent
    {
        #region Public members

        /// <summary>
        /// Type of MIDI event from the status byte
        /// </summary>
        public enum StatusType : byte
        {
            NOTE_OFF = 0x80,
            NOTE_ON = 0x90,
            POLYPHONIC_AFTERTOUCH = 0xa0,
            CONTROL_MODE_CHANGE = 0xb0,
            PROGRAM_CHANGE = 0xc0,
            CHANNEL_AFTERTOUCH = 0xd0,
            PITCH_WHEEL_RANGE = 0xe0,
        }

        /// <summary>
        /// Number of channel from the status byte
        /// </summary>
        public enum Channel : byte
        {
            CHANNEL_1 = 0x00,
            CHANNEL_2 = 0x01,
            CHANNEL_3 = 0x02,
            CHANNEL_4 = 0x03,
            CHANNEL_5 = 0x04,
            CHANNEL_6 = 0x05,
            CHANNEL_7 = 0x06,
            CHANNEL_8 = 0x07,
            CHANNEL_9 = 0x08,
            CHANNEL_10 = 0x09,
            CHANNEL_11 = 0x0a,
            CHANNEL_12 = 0x0b,
            CHANNEL_13 = 0x0c,
            CHANNEL_14 = 0x0d,
            CHANNEL_15 = 0x0e,
            CHANNEL_16 = 0x0f,
        }

        public StatusType statusType
        {
            get { return (StatusType)(status & 0xF0); }
        }

        public Channel channel
        {
            get { return (Channel)(status & 0x0F); }
        }

        /// <summary>
        /// Note Number (NOTE_OFF/NOTE_ON/POLYPHONIC_AFTERTOUCH)
        /// Control Function (CONTROL_MODE_CHANGE)
        /// Program # (PROGRAM_CHANGE)
        /// Aftertouch pressure (CHANNEL_AFTERTOUCH)
        /// Pitch wheel LSB (PITCH_WHEEL_RANGE)
        /// </summary>
        public byte data1;

        /// <summary>
        /// Note Velocity (NOTE_OFF/NOTE_ON)
        /// Aftertouch pressure (POLYPHONIC_AFTERTOUCH)
        /// MSB/LSB/ON/OFF (CONTROL_MODE_CHANGE)
        /// Pitch wheel MSB (PITCH_WHEEL_RANGE)
        /// </summary>
        public byte data2;

        public MidiEvent (byte status, byte data1, byte data2)
        {
            this.status = status;
            this.data1 = data1;
            this.data2 = data2;
        }
        
        public override string ToString ()
        {
            return "[" + channel.ToString() + "_" + statusType.ToString() + "," + data1 + "," + data2 + "]";
        }

        #endregion

        #region Private members

        byte status;

        #endregion
    }

    /// <summary>
    /// Meta event
    /// </summary>
    public struct MetaEvent
    {
        #region Public members

        /// <summary>
        /// Type of Meta event
        /// </summary>
        public enum Type : byte
        {
            SEQUENCE_NUMBER = 0x00,
            TEXT_EVENT = 0x01,
            COPYRIGHT_NOTICE = 0x02,
            TRACK_NAME = 0x03,
            INSTRUMENT_NAME = 0x04,
            LYRIC = 0x05,
            MARKER = 0x06,
            CUE_POINT = 0x07,
            MIDI_CHANNEL_PREFIX = 0x20,
            END_OF_TRACK = 0x27,
            SET_TEMPO = 0x51,
            SMTPE_OFFSET = 0x54,
            TIME_SIGNATURE = 0x58,
            KEY_SIGNATURE = 0x59,
        }

        public Type type
        {
            get { return (Type)(typeByte); }
        }

        public byte[] data;

        public MetaEvent (byte type, byte[] data)
        {
            this.typeByte = type;
            this.data = data;
        }
        
        public override string ToString ()
        {
            return "[" + typeByte.ToString() + "_" + data + "]";
        }

        #endregion

        #region Private members

        byte typeByte;

        #endregion
    }

    /// <summary>
    /// MIDI track
    /// 
    /// Stores only one track (usually a MIDI file contains one or more tracks).
    /// </summary>
    public class MidiTrack
    {
        #region Internal data structure

        /// <summary>
        /// Data pair storing a delta-time value and an event.
        /// </summary>
        public struct DeltaEventPair
        {
            public int delta;
            public MidiEvent? midiEvent;
            public MetaEvent? metaEvent;
            
            public DeltaEventPair (int delta, MidiEvent midiEvent)
            {
                this.delta = delta;
                this.midiEvent = midiEvent;
                this.metaEvent = null;
            }

            public DeltaEventPair(int delta, MetaEvent metaEvent)
            {
                this.delta = delta;
                this.midiEvent = null;
                this.metaEvent = metaEvent;
            }
            
            public override string ToString ()
            {
                if (midiEvent.HasValue) return "(" + delta + ":" + midiEvent.Value + ")";
                else return "(" + delta + ":" + metaEvent.Value + ")";
            }
        }

        #endregion

        #region Public members

        public MidiTrack ()
        {
            sequence = new List<DeltaEventPair> ();
        }

        /// <summary>
        /// Returns an enumerator which enumerates the all delta-event pairs.
        /// </summary>
        public List<DeltaEventPair>.Enumerator GetEnumerator ()
        {
            return sequence.GetEnumerator ();
        }
        
        public DeltaEventPair GetAtIndex (int index)
        {
            return sequence [index];
        }
        
        public override string ToString ()
        {
            var s = "";
            foreach (var pair in sequence)
                s += pair;
            return s;
        }

        #endregion

        #region Private and internal members

        List<DeltaEventPair> sequence;

        public void AddEvent (int delta, MidiEvent midiEvent)
        {
            sequence.Add (new DeltaEventPair (delta, midiEvent));
        }

        public void AddEvent(int delta, MetaEvent metaEvent)
        {
            sequence.Add(new DeltaEventPair(delta, metaEvent));
        }

        #endregion
    }
    
    /// <summary>
    /// MIDI file container
    /// </summary>
    public struct MidiFileContainer
    {
        #region Public members

        /// <summary>
        /// Division number == PPQN for this song.
        /// </summary>
        public int division;

        /// <summary>
        /// Track list contained in this file.
        /// </summary>
        public List<MidiTrack> tracks;
        
        public MidiFileContainer (int division, List<MidiTrack> tracks)
        {
            this.division = division;
            this.tracks = tracks;
        }
        
        public override string ToString ()
        {
            var temp = division.ToString () + ",";
            foreach (var track in tracks) {
                temp += track;
            }
            return temp;
        }

        #endregion
    }

    /// <summary>
    /// Sequencer for MIDI tracks
    ///
    /// Works like an enumerator for MIDI events.
    /// Note that not only Advance() but also Start() can return MIDI events.
    /// </summary>
    public class MidiTrackSequencer
    {
        #region Public members

        public bool Playing {
            get { return playing; }
        }

        public float BPM
        {
            get { return bpm; }
            set 
            { 
                bpm = value;
                pulsePerSecond = bpm / 60.0f * pulsePerQuarterNote;
            }
        }

        public MidiEvent.Channel Channel
        {
            get { return channel; }
            set { channel = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="track">MidiTrack to sequence.</param>
        /// <param name="ppqn">"ppqn" stands for Pulse Per Quater Note, which is usually provided with a MIDI header.</param>
        /// <param name="bpm">Sequencer BPM.</param>
        public MidiTrackSequencer (MidiTrack track, int ppqn, float bpm = 120)
        {
            pulsePerQuarterNote = ppqn;
            BPM = bpm;
            enumerator = track.GetEnumerator ();
        }

        /// <summary>
        /// Start the sequence.
        /// </summary>
        /// <param name="startTime">Sequencer start time</param>
        /// <returns>List of events at the beginning of the track.</returns>
        public List<MidiEvent> Start (float startTime = 0.0f)
        {
            if (enumerator.MoveNext ()) {
                pulseToNext = enumerator.Current.delta;
                playing = true;
                return Advance (startTime);
            } else {
                playing = false;
                return null;
            }
        }

        /// <summary>
        /// Advance the song position.
        /// </summary>
        /// <param name="deltaTime">Time to advance.</param>
        /// <returns>Returns a list of events between the current position and the next one.</returns>
        public List<MidiEvent> Advance (float deltaTime)
        {
            if (!playing) {
                return null;
            }
            
            pulseCounter += pulsePerSecond * deltaTime;
            
            if (pulseCounter < pulseToNext) {
                return null;
            }
            
            var messages = new List<MidiEvent> ();
            
            while (pulseCounter >= pulseToNext) {
                var pair = enumerator.Current;
                if (pair.midiEvent.HasValue) {
                    if (channel != pair.midiEvent.Value.channel)
                        channel = pair.midiEvent.Value.channel;
                    messages.Add (pair.midiEvent.Value);
                }
                else if (pair.metaEvent.HasValue) {
                    /// Change the BPM of the song by using the Tempo Meta event
                    if(pair.metaEvent.Value.type == MetaEvent.Type.SET_TEMPO) {
                        byte[] data = pair.metaEvent.Value.data;
                        int tempo = data[2] + (data[1] << 8) + (data[0] << 16);
                        BPM = 60000000 / tempo;
                    }
                }
                if (!enumerator.MoveNext ()) {
                    playing = false;
                    break;
                }
                
                pulseCounter -= pulseToNext;
                pulseToNext = enumerator.Current.delta;
            }
            
            return messages;
        }

        #endregion

        #region Private members

        DeltaEventPairList.Enumerator enumerator;
        bool playing;
        float bpm;
        MidiEvent.Channel channel;
        float pulsePerQuarterNote;
        float pulsePerSecond;
        float pulseToNext;
        float pulseCounter;

        #endregion
    }

    /// <summary>
    /// MIDI file loader
    ///
    /// Loads an SMF and returns a file container object.
    /// </summary>
    public static class MidiFileLoader
    {
        #region Public members

        public static MidiFileContainer Load (byte[] data)
        {
            var tracks = new List<MidiTrack> ();
            var reader = new MidiDataStreamReader (data);
            
            // Chunk type.
            if (new string (reader.ReadChars (4)) != "MThd") {
                throw new System.FormatException ("Can't find header chunk.");
            }
            
            // Chunk length.
            if (reader.ReadBEInt32 () != 6) {
                throw new System.FormatException ("Length of header chunk must be 6.");
            }
            
            // Format (unused).
            reader.Advance (2);
            
            // Number of tracks.
            var trackCount = reader.ReadBEInt16 ();
            
            // Delta-time divisions.
            var division = reader.ReadBEInt16 ();
            if ((division & 0x8000) != 0) {
                throw new System.FormatException ("SMPTE time code is not supported.");
            }
            
            // Read the tracks.
            for (var trackIndex = 0; trackIndex < trackCount; trackIndex++) {
                tracks.Add (ReadTrack (reader));
            }
            
            return new MidiFileContainer (division, tracks);
        }

        #endregion

        #region Private members
        
        static MidiTrack ReadTrack (MidiDataStreamReader reader)
        {
            var track = new MidiTrack ();
            
            // Chunk type.
            if (new string (reader.ReadChars (4)) != "MTrk") {
                throw new System.FormatException ("Can't find track chunk.");
            }
            
            // Chunk length.
            var chunkEnd = reader.ReadBEInt32 ();
            chunkEnd += reader.Offset;
            
            // Read delta-time and event pairs.
            byte ev = 0;
            while (reader.Offset < chunkEnd) {
                // Delta time.
                var delta = reader.ReadMultiByteValue ();
                
                // Event type.
                if ((reader.PeekByte () & 0x80) != 0) {
                    ev = reader.ReadByte ();
                }
                
                if (ev == 0xff) {
                    // 0xff: Meta event
                    byte type = reader.ReadByte();
                    byte length = reader.ReadByte();
                    byte[] data = reader.ReadBytes(length);
                    track.AddEvent(delta, new MetaEvent(type, data));
                } else if (ev == 0xf0) {
                    // 0xf0: SysEx (unused).
                    while (reader.ReadByte() != 0xf7) {
                    }
                } else {
                    // MIDI event
                    byte data1 = reader.ReadByte ();
                    byte data2 = ((ev & 0xe0) == 0xc0) ? (byte)0 : reader.ReadByte ();
                    track.AddEvent (delta, new MidiEvent (ev, data1, data2));
                }
            }
            
            return track;
        }

        #endregion
    }

    /// <summary>
    /// Binary data stream reader (for internal use)
    /// </summary>
    class MidiDataStreamReader
    {
        private byte[] data;
        private int offset;

        public int Offset {
            get { return offset; }
        }

        public MidiDataStreamReader (byte[] data)
        {
            this.data = data;
        }

        public void Advance (int length)
        {
            offset += length;
        }

        public byte PeekByte ()
        {
            return data [offset];
        }

        public byte ReadByte ()
        {
            return data [offset++];
        }

        public byte[] ReadBytes(int length)
        {
            var temp = new byte[length];
            for (var i = 0; i < length; i++)
            {
                temp[i] = ReadByte();
            }
            return temp;
        }

        public char[] ReadChars (int length)
        {
            var temp = new char[length];
            for (var i = 0; i < length; i++) {
                temp [i] = (char)ReadByte ();
            }
            return temp;
        }

        public int ReadBEInt32 ()
        {
            int b1 = ReadByte ();
            int b2 = ReadByte ();
            int b3 = ReadByte ();
            int b4 = ReadByte ();
            return b4 + (b3 << 8) + (b2 << 16) + (b1 << 24);
        }
        
        public int ReadBEInt16 ()
        {
            int b1 = ReadByte ();
            int b2 = ReadByte ();
            return b2 + (b1 << 8);
        }

        public int ReadMultiByteValue ()
        {
            int value = 0;
            while (true) {
                int b = ReadByte ();
                value += b & 0x7f;
                if (b < 0x80)
                    break;
                value <<= 7;
            }
            return value;
        }
    }
}
