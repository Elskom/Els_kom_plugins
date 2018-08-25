// Copyright (c) 2014-2018, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.
/* define if not defined already. */
#if !KOMV3
#define KOMV3
#endif

namespace komv3_plugin
{
    using System;
    using System.IO;
    using System.Text;
    using Els_kom_Core.Classes;
    using Els_kom_Core.Interfaces;

    public class Komv3_plugin : IKomPlugin
    {
        public string PluginName => "KOM V3 Plugin";
        public string KOMHeaderString => "KOG GC TEAM MASSFILE V.0.3.";
        public int SupportedKOMVersion => 3;

        public void Pack(string in_path, string out_path, string KOMFileName)
        {
            var kOMStream = new KOMStream();
            // convert the crc.xml file to the version for this plugin, if needed.
            kOMStream.ConvertCRC(3, in_path + Path.DirectorySeparatorChar + "crc.xml");
            kOMStream.Dispose();
            // not implemented yet due to lack of packing information on v3 koms.
            throw new NotImplementedException();
        }

        public void Unpack(string in_path, string out_path, string KOMFileName)
        {
            var reader = new BinaryReader(File.OpenRead(in_path), Encoding.ASCII);
            reader.BaseStream.Position += 52;
            var entry_count = (int)reader.ReadUInt64();
            reader.BaseStream.Position += 4;
            var file_time = reader.ReadInt32();
            var xml_size = reader.ReadInt32();
            var xmldatabuffer = reader.ReadBytes(xml_size);
            var xmldata = Encoding.ASCII.GetString(xmldatabuffer);
            var kOMStream = new KOMStream();
            var entries = kOMStream.Make_entries_v3(xmldata, entry_count);
            foreach (var entry in entries)
            {
                // we iterate through every entry here and unpack the data.
                kOMStream.WriteOutput(reader, out_path, entry, SupportedKOMVersion, xmldata, in_path);
            }
            kOMStream.Dispose();
            reader.Dispose();
        }

        public void Delete(string in_path, bool folder)
        {
        }

        public void ConvertCRC(int crcversion, string crcpath)
        {
            if (crcversion == 2)
            {
                // do conversions here.
            }
            else if (crcversion == 4)
            {
                // do conversions here.
            }
        }

        public void UpdateCRC(string crcpath, string checkpath)
        {
            // backup original crc.xml.
            // modify crc.xml object.
            // save xml object.
            // manually compare the 2 files later when debugging.
        }
    }
}
