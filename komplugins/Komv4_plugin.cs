// Copyright (c) 2014-2018, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.
/* define if not defined already. */
#if !KOMV4
#define KOMV4
#endif

namespace komv4_plugin
{
    public class Komv4_plugin : Els_kom_Core.interfaces.IKomPlugin
    {
        public string PluginName => "KOM V4 Plugin";
        public string KOMHeaderString => "KOG GC TEAM MASSFILE V.0.4.";
        public int SupportedKOMVersion => 4;

        public void Pack(string in_path, string out_path, string KOMFileName)
        {
            Els_kom_Core.Classes.KOMStream kOMStream = new Els_kom_Core.Classes.KOMStream();
            // convert the crc.xml file to the version for this plugin, if needed.
            kOMStream.ConvertCRC(4, in_path + System.IO.Path.DirectorySeparatorChar + "crc.xml");
            kOMStream.Dispose();
            // not implemented yet due to lack of packing information on v4 koms.
            throw new System.NotImplementedException();
        }

        public void Unpack(string in_path, string out_path, string KOMFileName)
        {
            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(in_path), System.Text.Encoding.ASCII);
            // apperently this should seek into offset 50 when in a hex editor.
            reader.BaseStream.Position += 57;
            // 4 bytes
            //int entry_count = (int)reader.ReadInt32();
            // trying to understand this crap... This is where it starts to fail
            // for KOM V4 on Elsword's current data036.kom.
            int enc_key = reader.ReadInt32();
            int file_time = reader.ReadInt32();
            int xml_size = reader.ReadInt32();
            byte[] xmldatabuffer = reader.ReadBytes(xml_size);
            Els_kom_Core.Classes.KOMStream kOMStream = new Els_kom_Core.Classes.KOMStream();
            kOMStream.DecryptCRCXml(enc_key, ref xmldatabuffer, xml_size, System.Text.Encoding.ASCII);
            string xmldata = System.Text.Encoding.ASCII.GetString(xmldatabuffer);
            try
            {
                System.Collections.Generic.List<Els_kom_Core.Classes.EntryVer> entries = kOMStream.Make_entries_v4(xmldata);
                foreach (var entry in entries)
                {
                    // we iterate through every entry here and unpack the data.
                    kOMStream.WriteOutput(reader, out_path, entry, SupportedKOMVersion, xmldata, in_path);
                }
            }
            catch (System.Xml.XmlException)
            {
                throw new Els_kom_Core.Classes.UnpackingError("failure with xml entry data reading...");
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
            else if (crcversion == 3)
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
