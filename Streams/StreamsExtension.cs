using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Streams
{
    public static class StreamsExtension
    {
        /// <summary>
        /// Implements the logic of byte copying the contents of the source text file using class FileStream as a backing store stream.
        /// </summary>
        /// <param name="sourcePath">Path to source file.</param>
        /// <param name="destinationPath">Path to destination file.</param>
        /// <returns>The number of recorded bytes.</returns>
        /// <exception cref="ArgumentException">Throw if path to source file or path to destination file is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Throw if source file doesn't exist.</exception>
        public static int ByteCopyWithFileStream(string sourcePath, string destinationPath)
        {
            InputValidation(sourcePath, destinationPath);

            using FileStream sourceStream = new FileStream(sourcePath, FileMode.Open),
            destinationStream = new FileStream(destinationPath, FileMode.OpenOrCreate);
            byte oneByte;
            for (int i = 0; i < sourceStream.Length; i++)
            {
                oneByte = (byte)sourceStream.ReadByte();
                destinationStream.WriteByte(oneByte);
            }

            return (int)destinationStream.Length;
        }

        /// <summary>
        /// Implements the logic of block copying the contents of the source text file using FileStream buffer.
        /// </summary>
        /// <param name="sourcePath">Path to source file.</param>
        /// <param name="destinationPath">Path to destination file.</param>
        /// <returns>The number of recorded bytes.</returns>
        /// <exception cref="ArgumentException">Throw if path to source file or path to destination file is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Throw if source file doesn't exist.</exception>
        public static int BlockCopyWithFileStream(string sourcePath, string destinationPath)
        {
            InputValidation(sourcePath, destinationPath);

            using FileStream source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read),
            destination = new FileStream(destinationPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            byte[] buffer = new byte[source.Length];
            int numBytesRead = source.Read(buffer, 0, (int)source.Length);
            destination.Write(buffer);

            return numBytesRead;
        }

        /// <summary>
        /// Implements the logic of block copying the contents of the source text file using FileStream and class-decorator BufferedStream.
        /// </summary>
        /// <param name="sourcePath">Path to source file.</param>
        /// <param name="destinationPath">Path to destination file.</param>
        /// <returns>The number of recorded bytes.</returns>
        /// <exception cref="ArgumentException">Throw if path to source file or path to destination file is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Throw if source file doesn't exist.</exception>
        public static int BlockCopyWithBufferedStream(string sourcePath, string destinationPath)
        {
            InputValidation(sourcePath, destinationPath);

            using Stream source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read),
                destination = new FileStream(destinationPath, FileMode.OpenOrCreate, FileAccess.Write),
                bufferStream = new BufferedStream(destination);

            byte[] bufferArr = new byte[source.Length];
            int numBytesRead = source.Read(bufferArr);
            bufferStream.Write(bufferArr);

            return numBytesRead;
        }

        /// <summary>
        /// Implements the logic of line-by-line copying of the contents of the source text file
        /// using FileStream and classes-adapters  StreamReader/StreamWriter
        /// </summary>
        /// <param name="sourcePath">Path to source file.</param>
        /// <param name="destinationPath">Path to destination file.</param>
        /// <returns>The number of recorded lines.</returns>
        /// <exception cref="ArgumentException">Throw if path to source file or path to destination file are null or empty.</exception>
        /// <exception cref="FileNotFoundException">Throw if source file doesn't exist.</exception>
        public static int LineCopy(string sourcePath, string destinationPath)
        {
            InputValidation(sourcePath, destinationPath);

            using var sr = new StreamReader(sourcePath);
            using var sw = new StreamWriter(destinationPath);
            int count = 0;
            string line;
            while ((line = sr.ReadLine()) != null)
            {    
                if (sr.Peek() < 0)
                {
                    sw.Write(line);
                    count++;
                    break;
                }
                sw.WriteLine(line);
                count++;
            }

            return count;
        }

        /// <summary>
        /// Reads file content encoded with non Unicode encoding.
        /// </summary>
        /// <param name="sourcePath">Path to source file.</param>
        /// <param name="encoding">Encoding name.</param>
        /// <returns>Unicoded file content.</returns>
        /// <exception cref="ArgumentException">Throw if path to source file or encoding string is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Throw if source file doesn't exist.</exception>
        public static string ReadEncodedText(string sourcePath, string encoding)
        {
            InputValidation(sourcePath);

            Encoding srsEnc = Encoding.GetEncoding(encoding);
            Encoding dstEnc = Encoding.Unicode;
            using StreamReader sr = new StreamReader(sourcePath, srsEnc);

            byte[] srsBytes = srsEnc.GetBytes(sr.ReadToEnd());
            byte[] resBytes = Encoding.Convert(srsEnc, dstEnc, srsBytes);

            char[] resChars = new char[dstEnc.GetCharCount(resBytes)];
            dstEnc.GetChars(resBytes, 0, resBytes.Length, resChars, 0);
            string result = new string(resChars);

            return result;
        }

        /// <summary>
        /// Returns decompressed stream from file. 
        /// </summary>
        /// <param name="sourcePath">Path to source file.</param>
        /// <param name="method">Method used for compression (none, deflate, gzip).</param>
        /// <returns>Output stream.</returns>
        /// <exception cref="ArgumentException">Throw if path to source file is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Throw if source file doesn't exist.</exception>
        public static Stream DecompressStream(string sourcePath, DecompressionMethods method)
        {
            InputValidation(sourcePath);

            Stream stream = new FileStream(sourcePath, FileMode.Open);

            return method switch
            {
                DecompressionMethods.GZip => new GZipStream(stream, CompressionMode.Decompress),
                DecompressionMethods.Deflate => new DeflateStream(stream, CompressionMode.Decompress),
                _ => stream,
            };
        }

        /// <summary>
        /// Calculates hash of stream using specified algorithm.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="hashAlgorithmName">
        ///     Hash algorithm ("MD5","SHA1","SHA256" and other supported by .NET).
        /// </param>
        /// <returns>Hash.</returns>
        public static string CalculateHash(this Stream stream, string hashAlgorithmName)
        {
            try
            {
                var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName);
                byte[] arr = hashAlgorithm.ComputeHash(stream);
                return BitConverter.ToString(arr).Replace("-", String.Empty);
            }
            catch
            {
                throw new ArgumentException($"Algorithm Is Not Supported: {nameof(hashAlgorithmName)}");
            }
        }

            private static void InputValidation(string sourcePath, string destinationPath)
            {
                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    throw new ArgumentException($"{nameof(sourcePath)} cannot be null or empty or whitespace.", nameof(sourcePath));
                }

                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException($"File '{sourcePath}' not found. Parameter name: {nameof(sourcePath)}.");
                }

                if (string.IsNullOrWhiteSpace(destinationPath))
                {
                    throw new ArgumentException($"{nameof(destinationPath)} cannot be null or empty or whitespace",
                        nameof(destinationPath));
                }
            }

            private static void InputValidation(string sourcePath)
            {
                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    throw new ArgumentException($"{nameof(sourcePath)} cannot be null or empty or whitespace.", nameof(sourcePath));
                }

                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException($"File '{sourcePath}' not found. Parameter name: {nameof(sourcePath)}.");
                }
            }
        }
    }