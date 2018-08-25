// Copyright (c) 2014-2018, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.

#if KOMV2
namespace komv2_plugin
#elif KOMV3
namespace komv3_plugin
#elif KOMV4
namespace komv4_plugin
#endif
{
    using System;
#if KOMV2
    using System.IO;
#endif
#if !KOMV3
    using System.Text;
#endif
    using System.Collections.Generic;
#if !KOMV2
    using System.Globalization;
    using System.Xml.Linq;
#if KOMV4
    using System.Security.Cryptography;
#endif
#endif
    using Els_kom_Core.Classes;

    internal static class KOMStreamExtension
    {
#if KOMV2
        internal static void ReadCrc(this KOMStream kOMStream, string crcfile, out byte[] crcdata, ref int entry_count, ref int crc_size)
        {
            crcdata = File.ReadAllBytes(crcfile);
            File.Delete(crcfile);
            entry_count++;
            crc_size = crcdata.Length;
        }

        internal static List<EntryVer> Make_entries_v2(this KOMStream kOMStream, int entrycount, BinaryReader reader)
        {
            var entries = new List<EntryVer>();
            for (var i = 0; i < entrycount; i++)
            {
                kOMStream.ReadInFile(reader, out var key, 60, System.Text.Encoding.ASCII);
                kOMStream.ReadInFile(reader, out var originalsize);
                kOMStream.ReadInFile(reader, out var compressedSize);
                kOMStream.ReadInFile(reader, out var offset);
                var entry = new EntryVer(kOMStream.GetSafeString(key), originalsize, compressedSize, offset);
                entries.Add(entry);
            }
            return entries;
        }

        internal static string GetSafeString(this KOMStream kOMStream, string source)
            => source.Contains(new string(char.MinValue, 1)) ? source.Substring(0, source.IndexOf(char.MinValue)) : source;

        internal static bool ReadInFile(this KOMStream kOMStream, BinaryReader binaryReader, out string destString, int length, Encoding encoding)
        {
            if (binaryReader == null)
            {
                throw new ArgumentNullException(nameof(binaryReader));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }
            var position = binaryReader.BaseStream.Position;
            var readBytes = binaryReader.ReadBytes(length);
            if ((binaryReader.BaseStream.Position - position) == length)
            {
                destString = encoding.GetString(readBytes);
                return true;
            }
            destString = null;
            return false;
        }

        internal static bool ReadInFile(this KOMStream kOMStream, BinaryReader binaryReader, out int destInt)
        {
            if (binaryReader == null)
            {
                throw new ArgumentNullException(nameof(binaryReader));
            }
            var position = binaryReader.BaseStream.Position;
            var readInt = binaryReader.ReadInt32();
            if ((binaryReader.BaseStream.Position - position) == sizeof(int))
            {
                destInt = readInt;
                return true;
            }
            destInt = int.MinValue;
            return false;
        }
#elif KOMV3
        internal static List<EntryVer> Make_entries_v3(this KOMStream kOMStream, string xmldata, int entry_count)
        {
            var entries = new List<EntryVer>();
            var xml = XElement.Parse(xmldata);
            foreach (var fileElement in xml.Elements("File"))
            {
                var nameAttribute = fileElement.Attribute("Name");
                var name = nameAttribute?.Value ?? "no value";
                var sizeAttribute = fileElement.Attribute("Size");
                var size = sizeAttribute == null ? -1 : Convert.ToInt32(sizeAttribute.Value);
                var CompressedSizeAttribute = fileElement.Attribute("CompressedSize");
                var CompressedSize = CompressedSizeAttribute == null ? -1 : Convert.ToInt32(CompressedSizeAttribute.Value);
                var ChecksumAttribute = fileElement.Attribute("Checksum");
                var Checksum = ChecksumAttribute == null ? -1 : int.Parse(ChecksumAttribute.Value, NumberStyles.HexNumber);
                var FileTimeAttribute = fileElement.Attribute("FileTime");
                var FileTime = FileTimeAttribute == null ? -1 : int.Parse(FileTimeAttribute.Value, NumberStyles.HexNumber);
                var AlgorithmAttribute = fileElement.Attribute("Algorithm");
                var Algorithm = AlgorithmAttribute == null ? -1 : Convert.ToInt32(AlgorithmAttribute.Value);
                var entry = new EntryVer(name, size, CompressedSize, Checksum, FileTime, Algorithm);
                entries.Add(entry);
            }
            return entries;
        }
#elif KOMV4
        private static Dictionary<int, int> KeyMap { get; set; } = new Dictionary<int, int>();

        private static void LoadKeyMap(int value)
        {
            //try
            //{
            //    System.IO.FileStream KeyMapfs = System.IO.File.OpenRead(System.Windows.Forms.Application.StartupPath + "\\plugins\\komKeyMap.dms");
            //    System.IO.BinaryReader KeyMapreader = new System.IO.BinaryReader(KeyMapfs, System.Text.Encoding.ASCII);
            //    for (long i = 0; i < KeyMapreader.BaseStream.Length;)
            //    {
            //        int key = KeyMapreader.ReadInt32();
            //        int value = KeyMapreader.ReadInt32();
            //        try
            //        {
            //            KeyMap.Add(key, value);
            //        }
            //        catch (System.ArgumentException)
            //        {
            //        }
            //        i = KeyMapreader.BaseStream.Position;
            //    }
            //    KeyMapreader.Dispose();
            //}
            //catch (System.IO.FileNotFoundException)
            //{
                // keymap file not found.
            //}
            var key = 0;
            if (KeyMap.Count != 0)
            {
                KeyMap.Clear();
            }
            KeyMap.Add(key, value);
        }

        internal static void DecryptCRCXml(this KOMStream kOMStream, int enckey, ref byte[] data, int length, Encoding encoding)
        {
            var key = 0;
            LoadKeyMap(enckey);
            if (!KeyMap.ContainsKey(key))
            {
                return;
            }

            var keyStr = KeyMap[key].ToString();
            var sha1Key = BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(encoding.GetBytes(keyStr))).Replace("-", "");

            var blowfish = new BlowFish(sha1Key);
            data = blowfish.Decrypt(data, CipherMode.ECB);
            blowfish.Dispose();
        }

        internal static List<EntryVer> Make_entries_v4(this KOMStream kOMStream, string xmldata)
        {
            var entries = new List<EntryVer>();
            var xml = XElement.Parse(xmldata);
            foreach (var fileElement in xml.Elements("File"))
            {
                var nameAttribute = fileElement.Attribute("Name");
                var name = nameAttribute?.Value ?? "no value";
                var sizeAttribute = fileElement.Attribute("Size");
                var size = sizeAttribute == null ? -1 : Convert.ToInt32(sizeAttribute.Value);
                var CompressedSizeAttribute = fileElement.Attribute("CompressedSize");
                var CompressedSize = CompressedSizeAttribute == null ? -1 : Convert.ToInt32(CompressedSizeAttribute.Value);
                var ChecksumAttribute = fileElement.Attribute("Checksum");
                var Checksum = ChecksumAttribute == null ? -1 : int.Parse(ChecksumAttribute.Value, NumberStyles.HexNumber);
                var FileTimeAttribute = fileElement.Attribute("FileTime");
                var FileTime = FileTimeAttribute == null ? -1 : int.Parse(FileTimeAttribute.Value, NumberStyles.HexNumber);
                var AlgorithmAttribute = fileElement.Attribute("Algorithm");
                var Algorithm = AlgorithmAttribute == null ? -1 : Convert.ToInt32(AlgorithmAttribute.Value);
                // on v4 at least on Elsword there is now an MappedID attribute.
                // this is even more of an reason to store some cache
                // file for not only kom v3 for the algorithm 2 & 3
                // files to be able to get repacked to those
                // algorithm’s but also to store these unique
                // map id’s to this version of kom.
                var MappedIDAttribute = fileElement.Attribute("MappedID");
                var MappedID = MappedIDAttribute?.Value ?? "no value";
                var entry = new EntryVer(name, size, CompressedSize, Checksum, FileTime, Algorithm, MappedID);
                entries.Add(entry);
            }
            return entries;
        }
#endif
    }
}
