// Copyright (c) 2014-2018, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.
/* define if not defined already. */
#if !KOMV2
#define KOMV2
#endif

namespace komv2_plugin
{
    public class Komv2_plugin : Els_kom_Core.Interfaces.IKomPlugin
    {
        public string PluginName => "KOM V2 Plugin";
        public string KOMHeaderString => "KOG GC TEAM MASSFILE V.0.2.";
        public int SupportedKOMVersion => 2;

        public void Pack(string in_path, string out_path, string KOMFileName)
        {
            var use_XoR = false;
            if (System.IO.File.Exists(in_path + "\\XoRNeeded.dummy"))
            {
                use_XoR = true;
                System.IO.File.Delete(in_path + "\\XoRNeeded.dummy");
            }
            var writer = new System.IO.BinaryWriter(System.IO.File.Create(out_path), System.Text.Encoding.ASCII);
            var entry_count = 0;
            var crc_size = 0;
            var kOMStream = new Els_kom_Core.Classes.KOMStream();
            // convert the crc.xml file to the version for this plugin, if needed.
            kOMStream.ConvertCRC(2, in_path + System.IO.Path.DirectorySeparatorChar + "crc.xml");
            kOMStream.ReadCrc(in_path + "\\crc.xml", out var crc_data, ref entry_count, ref crc_size);
            kOMStream.Dispose();
            var di = new System.IO.DirectoryInfo(in_path);
            var offset = 0;
            var entries = new System.Collections.Generic.List<Els_kom_Core.Classes.EntryVer>();
            foreach (var fi in di.GetFiles())
            {
                entry_count++;
                var file_data = System.IO.File.ReadAllBytes(in_path + "\\" + fi.Name);
                var originalsize = file_data.Length;
                if (use_XoR)
                {
                    var xorkey = System.Text.Encoding.UTF8.GetBytes("\xa9\xa9\xa9\xa9\xa9\xa9\xa9\xa9\xa9\xa9");
                    Els_kom_Core.Classes.BlowFish.XorBlock(ref file_data, xorkey);
                }
                byte[] compressedData;
                try
                {
                    Els_kom_Core.Classes.ZlibHelper.CompressData(file_data, out compressedData);
                }
                catch (Els_kom_Core.Classes.PackingError ex)
                {
                    throw new Els_kom_Core.Classes.PackingError("failed with zlib compression of entries.", ex);
                }
                var compressedSize = compressedData.Length;
                offset += compressedSize;
                entries.Add(new Els_kom_Core.Classes.EntryVer(compressedData, fi.Name, originalsize, compressedSize, offset));
            }
            if (use_XoR)
            {
                var xorkey = System.Text.Encoding.UTF8.GetBytes("\xa9\xa9\xa9\xa9\xa9\xa9\xa9\xa9\xa9\xa9");
                Els_kom_Core.Classes.BlowFish.XorBlock(ref crc_data, xorkey);
            }
            byte[] compressedcrcData;
            try
            {
                Els_kom_Core.Classes.ZlibHelper.CompressData(crc_data, out compressedcrcData);
            }
            catch (Els_kom_Core.Classes.PackingError ex)
            {
                throw new Els_kom_Core.Classes.PackingError("failed with zlib compression of crc.xml.", ex);
            }
            var compressedcrc = compressedcrcData.Length;
            offset += compressedcrc;
            entries.Add(new Els_kom_Core.Classes.EntryVer(compressedcrcData, "crc.xml", crc_size, compressedcrc, offset));
            writer.Write(KOMHeaderString.ToCharArray(), 0, KOMHeaderString.Length);
            writer.BaseStream.Position = 52;
            writer.Write(entry_count);
            writer.Write(1);
            foreach (var entry in entries)
            {
                writer.Write(entry.Name.ToCharArray(), 0, entry.Name.Length);
                var seek_amount = 60 - entry.Name.Length;
                writer.BaseStream.Position += seek_amount;
                writer.Write(entry.Uncompressed_size);
                writer.Write(entry.Compressed_size);
                writer.Write(entry.Relative_offset);
            }
            foreach (var entry in entries)
            {
                writer.Write(entry.Entrydata, 0, entry.Compressed_size);
            }
            writer.Dispose();
        }

        public void Unpack(string in_path, string out_path, string KOMFileName)
        {
            var reader = new System.IO.BinaryReader(System.IO.File.OpenRead(in_path), System.Text.Encoding.ASCII);
            reader.BaseStream.Position = 52;
            var kOMStream = new Els_kom_Core.Classes.KOMStream();
            kOMStream.ReadInFile(reader, out var entry_count);
            // without this dummy read the entry instances would not get the correct
            // data leading to an crash when tring to make an file with the entry name in the output path.
            kOMStream.ReadInFile(reader, out var size);
            var entries = kOMStream.Make_entries_v2(entry_count, reader);
            foreach (var entry in entries)
            {
                // we iterate through every entry here and unpack the data.
                kOMStream.WriteOutput(reader, out_path, entry, SupportedKOMVersion, string.Empty, in_path);
            }
            kOMStream.Dispose();
            reader.Dispose();
        }

        public void Delete(string in_path, bool folder)
        {
            if (folder)
            {
                // delete kom folder data.
                var di = new System.IO.DirectoryInfo(in_path);
                foreach (var fi in di.GetFiles())
                {
                    fi.Delete();
                }
                di.Delete();
            }
            else
            {
                // delete kom file.
                var fi = new System.IO.FileInfo(in_path);
                fi.Delete();
            }
        }

        public void ConvertCRC(int crcversion, string crcpath)
        {
            if (crcversion == 3)
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
