/*
 * SPDX-FileCopyrightText: 2025 Frank Schwab
 *
 * SPDX-License-Identifier: Apache-2.0
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * You may not use this file except in compliance with the License.
 *
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Author: Frank Schwab
 *
 * Change history:
 *    2025-08-02: V1.0.0: Created. fhs
 */

using Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Writer;

namespace MatrixTranspose {
   /// <summary>
   /// Class to handle filtered and transformed files.
   /// </summary>
   public static class FileHandler {
      // ******** Private constants ********

      /// <summary>
      /// Maximum file size.
      /// </summary>
      private const long MAX_FILE_SIZE = 100 * 1024 * 1024; // 100 MiB

      // Error messages.

      private const string FORMAT_ERROR_MUST_BE_POSITIVE = @"{0} must be positive";
      private const string FORMAT_ERROR_FILE_OPERATION = @"Error {0}ing file '{1}': {2}";
      private const string ERROR_FILE_PATH_EMPTY = @"File path must not be null or empty";


      // ******** Public methods ********

      /// <summary>
      /// Reads an encrypted file and filters for substitution characters.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Wanted encoding of file.</param>
      /// <param name="substitutionCharacters">Substitution characters.</param>
      /// <param name="numPlaces">Number of places per substitution.</param>
      /// <param name="hasBom">Out: Reports, whether file has a BOM.</param>
      /// <param name="readLength">Out: Length of read data.</param>
      /// <param name="usedEncoding">Out: Encoding used for reading.</param>
      /// <returns>Read data. The array length is file size * <paramref name="numPlaces"/>,
      /// which is probably larger than <paramref name="readLength"/>.</returns>
      public static char[] ReadEncryptedTextFile(
         in string filePath,
         in Encoding encoding,
         in char[] substitutionCharacters,
         byte numPlaces,
         out bool hasBom,
         out int readLength,
         out Encoding usedEncoding) {
         long fileSize = CheckFileParameters(filePath, encoding);

         char[] result;
         try {
            using (var fileStream = File.OpenRead(filePath)) {
               hasBom = EncodingHelper.DetectBomEncoding(fileStream, out usedEncoding);
               if (usedEncoding == null)
                  usedEncoding = encoding;

               using (var fileReader = new StreamReader(fileStream, usedEncoding, true))
               using (var lineReader = new LineEndingNormalizingTextReader(fileReader))
               using (var upperReader = new TransformingTextReader(lineReader, TransformingTextReader.ToUpperTransformer))
               using (var filterReader = new FilteringTextReader(upperReader, substitutionCharacters))
                  result = ReadToEnd(filterReader, fileSize, usedEncoding.BytesPerCharacter(), numPlaces, out readLength);
            }
         } catch (Exception ex) {
            throw new IOException(string.Format(FORMAT_ERROR_FILE_OPERATION, @"read", filePath, ex.Message));
         }

         return result;
      }

      /// <summary>
      /// Reads a file and substitutes the matrix entries for characters.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Wanted encoding of file.</param>
      /// <param name="substitutions">Substitution of characters with matrix entries.</param>
      /// <param name="numPlaces">Number of places per substitution.</param>
      /// <param name="treatJAsI">Should a "J" be treated as "I".</param>
      /// <param name="toUpper">Convert all characters to uppercase on read.</param>
      /// <param name="hasBom">Out: Reports, whether file has a BOM.</param>
      /// <param name="readLength">Out: Length of read data.</param>
      /// <param name="usedEncoding">Out: Encoding used for reading.</param>
      /// <returns>Read data. The array length is file size * <paramref name="numPlaces"/>,
      /// which is probably larger than <paramref name="readLength"/>.</returns>
      public static char[] ReadSubstitutedCleartextFile(
         in string filePath,
         in Encoding encoding,
         in SortedDictionary<char, char[]> substitutions,
         byte numPlaces,
         bool treatJAsI,
         bool toUpper,
         out bool hasBom,
         out int readLength,
         out Encoding usedEncoding) {
         long fileSize = CheckFileParameters(filePath, encoding);

         TextReader reader = null;
         char[] result;
         try {
            using (var fileStream = File.OpenRead(filePath)) {
               hasBom = EncodingHelper.DetectBomEncoding(fileStream, out usedEncoding);
               if (usedEncoding == null)
                  usedEncoding = encoding;

               using (var fileReader = new StreamReader(fileStream, usedEncoding, detectEncodingFromByteOrderMarks: false)) {
                  reader = new LineEndingNormalizingTextReader(fileReader);

                  if (treatJAsI)
                     reader = new TransformingTextReader(reader, TransformingTextReader.JToITransformer);

                  if (toUpper)
                     reader = new TransformingTextReader(reader, TransformingTextReader.ToUpperTransformer);

                  reader = new MappingTextReader(reader, substitutions);

                  result = ReadToEnd(reader, fileSize, usedEncoding.BytesPerCharacter(), numPlaces, out readLength);
               }
            }
         } catch (Exception ex) {
            throw new IOException(string.Format(FORMAT_ERROR_FILE_OPERATION, @"read", filePath, ex.Message));
         } finally {
            reader?.Dispose();
         }

         return result;
      }

      /// <summary>
      /// Writes an encrypted file.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Wanted encoding of file.</param>
      /// <param name="withBom">Indicates, if a BOM must be written.</param>
      /// <param name="data">Data to write.</param>
      /// <param name="writeLength">Number of characters to write.</param>
      /// <param name="groupSize">Size of output group (0, if no grouping should be done).</param>
      /// <param name="maxLineLength">Maximum line length (0, if there is no maximum line length)</param>
      public static void WriteEncryptedFile(
         in string filePath,
         Encoding encoding,
         bool withBom,
         in char[] data,
         int writeLength,
         int groupSize,
         int maxLineLength) {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException(ERROR_FILE_PATH_EMPTY, nameof(filePath));
         if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));
         if (writeLength < 0)
            throw new ArgumentException(string.Format(FORMAT_ERROR_MUST_BE_POSITIVE, @"Write length"), nameof(writeLength));
         if (groupSize < 0)
            throw new ArgumentException(string.Format(FORMAT_ERROR_MUST_BE_POSITIVE, @"Group size"), nameof(groupSize));
         if (maxLineLength < 0)
            throw new ArgumentException(string.Format(FORMAT_ERROR_MUST_BE_POSITIVE, @"Maximum line length"), nameof(maxLineLength));

         if (groupSize > 1 && maxLineLength > 1)
            maxLineLength = maxLineLength / (groupSize + 1) * (groupSize + 1);

         encoding = EncodingHelper.EncodingWithMatchingBom(encoding, withBom);

         try {
            int groupPos = 0;
            int linePos = 0;
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(fileStream, encoding))
            using (var lineEndingNormalizer = new LineEndingTextWriter(writer))
               for (int i = 0; i < writeLength; i++) {
                  if (groupSize > 1 && groupPos >= groupSize) {
                     linePos++;
                     if (maxLineLength == 0 || linePos < maxLineLength)
                        lineEndingNormalizer.Write(' ');
                     groupPos = 0;
                  }

                  if (maxLineLength > 1 && linePos >= maxLineLength) {
                     lineEndingNormalizer.Write('\n');
                     linePos = 0;
                  }

                  lineEndingNormalizer.Write(data[i]);

                  groupPos++;
                  linePos++;
               }
         } catch (Exception ex) {
            throw new IOException(string.Format(FORMAT_ERROR_FILE_OPERATION, @"writ", filePath, ex.Message));
         }
      }

      /// <summary>
      /// Writes a decrypted file.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Wanted encoding of file.</param>
      /// <param name="withBom">Indicates, if a BOM must be written.</param>
      /// <param name="unsubstitutionMap">Mapping of matrix substitutions to characters.</param>
      /// <param name="data">Data to write.</param>
      /// <param name="writeLength">Number of characters to write.</param>
      public static void WriteUnsubstitutedCleartextFile(
         in string filePath,
         Encoding encoding,
         bool withBom,
         in SortedDictionary<char[], char> unsubstitutionMap,
         in char[] data,
         int writeLength) {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException(ERROR_FILE_PATH_EMPTY, nameof(filePath));
         if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));
         if (writeLength < 0)
            throw new ArgumentException(string.Format(FORMAT_ERROR_MUST_BE_POSITIVE, @"Write length"), nameof(writeLength));

         encoding = EncodingHelper.EncodingWithMatchingBom(encoding, withBom);

         try {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(fileStream, encoding))
            using (var lineEndingNormalizer = new LineEndingTextWriter(writer))
            using (var unmappingWriter = new UnmappingTextWriter(lineEndingNormalizer, unsubstitutionMap))
               for (int i = 0; i < writeLength; i++)
                  unmappingWriter.Write(data[i]);
         } catch (Exception ex) {
            throw new IOException(string.Format(FORMAT_ERROR_FILE_OPERATION, @"writ", filePath, ex.Message));
         }
      }

      /// <summary>
      /// Gets the size of a file in bytes.
      /// </summary>
      /// <param name="fileName">The path to the file.</param>
      /// <returns>The size of the file in bytes.</returns>
      public static long GetFileSize(in string fileName) {
         var fileInfo = new FileInfo(fileName);
         return fileInfo.Length;
      }

      // ******** Private Methods ********

      /// <summary>
      /// Checks if the file and encoding is suited.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Encoding.</param>
      /// <returns>Size of file.</returns>
      private static long CheckFileParameters(in string filePath, in Encoding encoding) {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException(ERROR_FILE_PATH_EMPTY, nameof(filePath));
         if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));

         long fileSize = GetFileSize(filePath);
         if (fileSize > MAX_FILE_SIZE)
            throw new ArgumentException($"File size exceeds the maximum allowed size of {MAX_FILE_SIZE} bytes.", nameof(filePath));

         return fileSize;
      }

      /// <summary>
      /// Reads the full filtered and mapped content of a file from <paramref name="reader"/>.
      /// </summary>
      /// <param name="reader"><see cref="TextReader"/> to read from.</param>
      /// <param name="fileSize">The size of the file.</param>
      /// <param name="numPlaces">Number of places per substitution.</param>
      /// <param name="readLength">Out: Number of characters read.</param>
      /// <returns>Read characters.</returns>
      private static char[] ReadToEnd(TextReader reader, long fileSize, int bytesPerCharacter, byte numPlaces, out int readLength) {
         char[] result = new char[(fileSize / bytesPerCharacter) * numPlaces];
         int actIndex = 0;
         int c;
         while ((c = reader.Read()) != -1)
            result[actIndex++] = (char)c;

         readLength = actIndex;

         return result;
      }
   }
}
