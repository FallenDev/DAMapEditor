using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace Capricorn.IO
{
    /// <summary>
    /// DAT File Archive Class
    /// </summary>
    public class DATArchive
    {
        private int expectedFiles;
        private DATFileEntry[] files;
        private string filename;

        /// <summary>
        /// Gets or sets the file entry at the specified index.
        /// </summary>
        /// <param name="index">Zero-based index of the file entry.</param>
        /// <returns>DAT archive file entry.</returns>
        public DATFileEntry this[int index]
        {
            get { return files[index]; }
            set { files[index] = value; }
        }	

        /// <summary>
        /// Gets or sets the file name of the archive.
        /// </summary>
        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }

        /// <summary>
        /// Gets the file entries within the archive.
        /// </summary>
        public DATFileEntry[] Files
        {
            get { return files; }
        }
	
        /// <summary>
        /// Gets the number of expected files within the archive.
        /// </summary>
        public int ExpectedFiles
        {
            get { return expectedFiles; }
        }

        /// <summary>
        /// Loads a data archive from disk.
        /// </summary>
        /// <param name="file">DAT archive to load.</param>
        /// <returns>DAT archive object.</returns>
        public static DATArchive FromFile(string file)
        {
            #region Get Stream and Reader
            FileStream stream = new FileStream(file,
                FileMode.Open, FileAccess.Read, FileShare.Read);

            BinaryReader reader = new BinaryReader(stream);
            #endregion

            // Create DAT Archive
            DATArchive dat = new DATArchive();
            dat.filename = file;

            // Get Expected File Count
            dat.expectedFiles = reader.ReadInt32();

            // Create Entries (Ignore Last Null Entry)
            dat.files = new DATFileEntry[dat.expectedFiles - 1];

            #region Read File Table
            for (int i = 0; i < dat.expectedFiles - 1; i++)
            {
                // Get Start Address
                long startAddress = reader.ReadUInt32();

                // Get Name Bytes
                string name = Encoding.ASCII.GetString(reader.ReadBytes(13));

                // Get End Address
                long endAddress = reader.ReadUInt32();

                // Seek Backwards an UINT32
                reader.BaseStream.Seek(-4, SeekOrigin.Current);

                // Remove Garbage Characters
                int firstNull = name.IndexOf('\0');
                if (firstNull != -1)
                    name = name.Remove(firstNull, 13 - firstNull);

                // Create Entry
                dat.files[i] = new DATFileEntry(name, startAddress, endAddress);

            } reader.Close();
            #endregion

            // Return DAT Archive
            return dat;
        }

        /// <summary>
        /// Checks if the archive contains a file with the specified name (case-sensitive).
        /// </summary>
        /// <param name="name">File name to check for.</param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            foreach (DATFileEntry file in this.files)
            {
                if (file.Name == name)
                    return true;

            } return false;
        }

        /// <summary>
        /// Checks if the archive contains a file with the specified name.
        /// </summary>
        /// <param name="name">File name to check for.</param>
        /// <param name="ignoreCase">Ignore casing (noncase-sensitive).</param>
        /// <returns></returns>
        public bool Contains(string name, bool ignoreCase)
        {
            foreach (DATFileEntry file in this.files)
            {
                if (ignoreCase)
                {
                    if (file.Name.ToUpper() == name.ToUpper())
                        return true;
                }
                else
                {
                    if (file.Name == name)
                        return true;
                }

            } return false;
        }

        /// <summary>
        /// Gets the index of the specified file (case-sensitive).
        /// </summary>
        /// <param name="name">Name of the file to find.</param>
        /// <returns></returns>
        public int IndexOf(string name)
        {
            for (int i = 0; i < this.files.Length; i++)
            {
                if (this.files[i].Name == name)
                    return i;
            }

            // Not Found
            return -1;
        }

        /// <summary>
        /// Gets the index of the specified file.
        /// </summary>
        /// <param name="name">Name of the file to find.</param>
        /// <param name="ignoreCase">Ignore casing (noncase-sensitive).</param>
        /// <returns></returns>
        public int IndexOf(string name, bool ignoreCase)
        {
            for (int i = 0; i < this.files.Length; i++)
            {
                if (ignoreCase)
                {
                    if (this.files[i].Name.ToUpper() == name.ToUpper())
                        return i;
                }
                else
                {
                    if (this.files[i].Name == name)
                        return i;
                }
            }

            // Not Found
            return -1;
        }

        /// <summary>
        /// Extracts a file from the archive, returning the raw data bytes of the file (case-sensitive).
        /// </summary>
        /// <param name="name">File name to extract.</param>
        /// <returns>Raw data of the extracted file.</returns>
        public byte[] ExtractFile(string name)
        {
            // Check if File Exists
            if(!this.Contains(name))
                return null;

            #region Create File Stream and Reader
            FileStream stream = new FileStream(this.filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            BinaryReader reader = new BinaryReader(stream);
            #endregion

            // Get Index
            int index = this.IndexOf(name);

            // Seek to File
            reader.BaseStream.Seek(this.files[index].StartAddress, SeekOrigin.Begin);

            // Read Bytes
            byte[] fileData = reader.ReadBytes((int)this.files[index].FileSize);

            // Close Stream
            reader.Close();

            // Return Data
            return fileData;
        }

        /// <summary>
        /// Extracts a file from the archive, returning the raw data bytes of the file.
        /// </summary>
        /// <param name="name">File name to extract.</param>
        /// <param name="ignoreCase">Ignore casing (noncase-sensitive).</param>
        /// <returns>Raw data of the extracted file.</returns>
        public byte[] ExtractFile(string name, bool ignoreCase)
        {
            // Check if File Exists
            if (!this.Contains(name, ignoreCase))
                return null;

            #region Create File Stream and Reader
            FileStream stream = new FileStream(this.filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            BinaryReader reader = new BinaryReader(stream);
            #endregion

            // Get Index
            int index = this.IndexOf(name, ignoreCase);

            // Seek to File
            reader.BaseStream.Seek(this.files[index].StartAddress, SeekOrigin.Begin);

            // Read Bytes
            byte[] fileData = reader.ReadBytes((int)this.files[index].FileSize);

            // Close Stream
            reader.Close();

            // Return Data
            return fileData;
        }

        /// <summary>
        /// Extracts a file from the archive, returning the raw data bytes of the file.
        /// </summary>
        /// <param name="entry">File to extract.</param>
        /// <returns>Raw data of the extracted file.</returns>
        public byte[] ExtractFile(DATFileEntry entry)
        {
            // Check if File Exists
            if (!this.Contains(entry.Name))
                return null;

            #region Create File Stream and Reader
            FileStream stream = new FileStream(this.filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            BinaryReader reader = new BinaryReader(stream);
            #endregion

            // Seek to File
            reader.BaseStream.Seek(entry.StartAddress, SeekOrigin.Begin);

            // Read Bytes
            byte[] fileData = reader.ReadBytes((int)entry.FileSize);

            // Close Stream
            reader.Close();

            // Return Data
            return fileData;
        }

        /// <summary>
        /// Gets the string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{Name = " + filename + ", Files = " + expectedFiles.ToString() + "}";
        }
    }

    /// <summary>
    /// DAT File Archive Entry Class
    /// </summary>
    public class DATFileEntry
    {
        private string name;
        private long startAddress;
        private long endAddress;

        /// <summary>
        /// Gets the file size of the file, in bytes, within the archive.
        /// </summary>
        public long FileSize
        {
            get { return endAddress - startAddress; }
        }
        /// <summary>
        /// Gets the ending address of the file within the archive.
        /// </summary>
        public long EndAddress
        {
            get { return endAddress; }
        }

        /// <summary>
        /// Gets the starting address of the file within the archive.
        /// </summary>
        public long StartAddress
        {
            get { return startAddress; }
        }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Creates a DAT file archive entry using the specified values.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="startAddress">Starting address of the file within the archive.</param>
        /// <param name="endAddress">Ending address of the file within the archive.</param>
        public DATFileEntry(string name, long startAddress, long endAddress)
        {
            this.name = name;
            this.startAddress = startAddress;
            this.endAddress = endAddress;
        }

        /// <summary>
        /// Gets the string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{Name = " + name + ", Size = " + FileSize.ToString("###,###,###,###,###0") + "}";
        }
    }
}
