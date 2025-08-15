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
 *    2025-08-14: V1.1.0: Use GroupingWriter. fhs
 *    2025-08-15: V2.0.0: Use ComposableTextReader and ComposableTextWriter. fhs
 */

using EncodingHandling;
using LineEndingHandling;
using ReadHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WriteHandling;

namespace MatrixTranspose {
   /// <summary>
   /// Class to handle filtered and transformed files.
   /// </summary>
   public static class FileHandler {
      #region Private constants
      /// <summary>
      /// Maximum file size.
      /// </summary>
      private const long MaxFileSize = 100 * 1024 * 1024; // 100 MiB

      // Error messages.

      private const string FormatErrorMustBePositive = "{0} must be positive";
      private const string FormatErrorFileOperation = "Error {0}ing file '{1}': {2}";
      private const string ErrorFilePathEmpty = "File path must not be null or empty";
      #endregion


      #region Public methods
      #region Read methods
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
         long fileSize = CheckReadBaseData(filePath, encoding);
         if (substitutionCharacters == null)
            throw new ArgumentNullException(nameof(substitutionCharacters));
         if (numPlaces <= 0)
            throw new ArgumentOutOfRangeException(nameof(numPlaces), string.Format(FormatErrorMustBePositive, "number of places"));

         char[] result;
         try {
            using (var reader = new ComposableTextReader(filePath, encoding)
               .WithNormalizedLineEnding()
               .WithTransformation(TransformingTextReader.ToUpperTransformer)
               .WithFiltering(substitutionCharacters)) {
               (result, readLength) = reader.ReadData(fileSize);
               usedEncoding = reader.CurrentEncoding;
               hasBom = reader.HasBom;
            }
         } catch (Exception ex) {
            throw new IOException(string.Format(FormatErrorFileOperation, "read", filePath, ex.Message));
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
         long fileSize = CheckReadBaseData(filePath, encoding);
         if (substitutions == null)
            throw new ArgumentNullException(nameof(substitutions));
         if (numPlaces <= 0)
            throw new ArgumentOutOfRangeException(nameof(numPlaces), string.Format(FormatErrorMustBePositive, "number of places"));

         char[] result;
         try {
            using (var reader = new ComposableTextReader(filePath, encoding)
               .WithNormalizedLineEnding()) {

               if (treatJAsI)
                  reader.WithTransformation(TransformingTextReader.JToITransformer);

               if (toUpper)
                  reader.WithTransformation(TransformingTextReader.ToUpperTransformer);

               reader.WithMapping(substitutions);

               (result, readLength) = reader.ReadData(fileSize);
               usedEncoding = reader.CurrentEncoding;
               hasBom = reader.HasBom;
            }
         } catch (Exception ex) {
            throw new IOException(string.Format(FormatErrorFileOperation, "read", filePath, ex.Message));
         }

         return result;
      }
      #endregion

      #region Write methods
      /// <summary>
      /// Writes an encrypted file.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Wanted encoding of file.</param>
      /// <param name="withBom">Indicates, if a BOM must be written.</param>
      /// <param name="lineEndingOption">Indicates, which line ending shall be written.</param>
      /// <param name="data">Data to write.</param>
      /// <param name="writeLength">Number of characters to write.</param>
      /// <param name="groupSize">Size of output group (0, if no grouping should be done).</param>
      /// <param name="maxLineLength">Maximum line length (0, if there is no maximum line length)</param>
      public static void WriteEncryptedFile(
         in string filePath,
         Encoding encoding,
         bool withBom,
         LineEndingHandler.Option lineEndingOption,
         in char[] data,
         int writeLength,
         int groupSize,
         int maxLineLength) {
         CheckWriteBaseData(filePath, encoding, data, writeLength);

         encoding = EncodingHelper.EncodingWithMatchingBom(encoding, withBom);

         try {
            using (var writer = new ComposableTextWriter(filePath, encoding)
               .WithNormalizedLineEndings(lineEndingOption)
               .WithGroupedOutput(groupSize, maxLineLength))
               writer.WriteData(data, writeLength);
         } catch (Exception ex) {
            throw new IOException(string.Format(FormatErrorFileOperation, "writ", filePath, ex.Message));
         }
      }

      /// <summary>
      /// Writes a decrypted file.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Wanted encoding of file.</param>
      /// <param name="withBom">Indicates, if a BOM must be written.</param>
      /// <param name="lineEndingOption">Indicates, which line ending shall be written.</param>
      /// <param name="unsubstitutionMap">Mapping of matrix substitutions to characters.</param>
      /// <param name="data">Data to write.</param>
      /// <param name="writeLength">Number of characters to write.</param>
      public static void WriteUnsubstitutedCleartextFile(
         in string filePath,
         Encoding encoding,
         bool withBom,
         LineEndingHandler.Option lineEndingOption,
         in SortedDictionary<char[], char> unsubstitutionMap,
         in char[] data,
         int writeLength) {
         CheckWriteBaseData(filePath, encoding, data, writeLength);

         encoding = EncodingHelper.EncodingWithMatchingBom(encoding, withBom);

         try {
            using (var writer = new ComposableTextWriter(filePath, encoding)
               .WithNormalizedLineEndings(lineEndingOption)
               .WithUnsubstitution(unsubstitutionMap))
               writer.WriteData(data, writeLength);
         } catch (Exception ex) {
            throw new IOException(string.Format(FormatErrorFileOperation, "writ", filePath, ex.Message));
         }
      }
      #endregion

      /// <summary>
      /// Gets the size of a file in bytes.
      /// </summary>
      /// <param name="filePath">The path to the file.</param>
      /// <returns>The size of the file in bytes.</returns>
      public static long GetFileSize(in string filePath) {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException(ErrorFilePathEmpty, nameof(filePath));

         return GetFileSizeNoCheck(filePath);
      }
      #endregion


      #region Private Methods
      /// <summary>
      /// Checks if the file and encoding is suited.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Encoding.</param>
      /// <exception cref="ArgumentException">Thrown if <paramref name="filePath"/> is null or an empty string
      /// or if the file exceeds the maximum allowed size.</exception>
      /// <exception cref="ArgumentNullException">Thrown if <paramref name="encoding"/> is null.</exception>
      /// <returns>Size of file.</returns>
      private static long CheckReadBaseData(in string filePath, in Encoding encoding) {
         CheckFileBaseData(filePath, encoding);

         long fileSize = GetFileSizeNoCheck(filePath);
         if (fileSize > MaxFileSize)
            throw new ArgumentException($"File size exceeds the maximum allowed size of {MaxFileSize} bytes.", nameof(filePath));

         return fileSize;
      }

      /// <summary>
      /// Checks if the file, encoding, data and write length are suited.
      /// </summary>
      /// <param name="filePath">The path to the file to be validated.</param>
      /// <param name="encoding">The character encoding to be used.</param>
      /// <param name="data">Data to write.</param>
      /// <param name="writeLength">Length of data to write.</param>
      /// <exception cref="ArgumentException">Thrown if <paramref name="filePath"/> is null or an empty string.</exception>
      /// <exception cref="ArgumentNullException">Thrown if <paramref name="encoding"/> or <paramref name="data"/> is null.</exception>
      /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="writeLength"/> is negative.</exception>
      private static void CheckWriteBaseData(in string filePath, in Encoding encoding, in char[] data, int writeLength) {
         CheckFileBaseData(filePath, encoding);

         if (data == null)
            throw new ArgumentNullException(nameof(data));
         if (writeLength < 0)
            throw new ArgumentOutOfRangeException(string.Format(FormatErrorMustBePositive, "Write length"), nameof(writeLength));
      }

      /// <summary>
      /// Validates the provided file path and encoding for correctness.
      /// </summary>
      /// <param name="filePath">The path to the file to be validated.</param>
      /// <param name="encoding">The character encoding to be used.</param>
      /// <exception cref="ArgumentException">Thrown if <paramref name="filePath"/> is null or an empty string.</exception>
      /// <exception cref="ArgumentNullException">Thrown if <paramref name="encoding"/> is null.</exception>
      private static void CheckFileBaseData(in string filePath, in Encoding encoding) {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException(ErrorFilePathEmpty, nameof(filePath));
         if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));
      }

      /// <summary>
      /// Gets the size of a file in bytes.
      /// This method assumes that the argument has been checked.
      /// </summary>
      /// <param name="filePath">The path to the file.</param>
      /// <returns>The size of the file in bytes.</returns>
      private static long GetFileSizeNoCheck(in string filePath) {
         var fileInfo = new FileInfo(filePath);
         return fileInfo.Length;
      }
      #endregion
   }
}
