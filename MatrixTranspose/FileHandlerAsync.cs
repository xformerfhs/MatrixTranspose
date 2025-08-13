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
 *    2025-08-12: V1.0.0: Created. fhs
 */

using LineEndingHandling;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MatrixTranspose {
   /// <summary>
   /// Wrapper class for asynchronous file handling operations.
   /// </summary>
   public static class FileHandlerAsync {
      /// <summary>
      /// Reads an encrypted file asynchronously and filters for substitution characters.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Wanted encoding of file.</param>
      /// <param name="substitutionCharacters">Substitution characters.</param>
      /// <param name="numPlaces">Number of places per substitution.</param>
      /// <returns>Task with read data, BOM indicator, read length and used encoding.
      /// The array length is "file size * <paramref name="numPlaces"/>", which is probably larger 
      /// than the read length.</returns>
      public static Task<(char[] Result, bool HasBom, int ReadLength, Encoding UsedEncoding)>
          ReadEncryptedTextFileAsync(
              string filePath,
              Encoding encoding,
              char[] substitutionCharacters,
              byte numPlaces) {
         return Task.Run(() =>
         {
            var result = FileHandler.ReadEncryptedTextFile(
                filePath,
                encoding,
                substitutionCharacters,
                numPlaces,
                out bool hasBom,
                out int readLength,
                out Encoding usedEncoding);

            return (result, hasBom, readLength, usedEncoding);
         });
      }

      /// <summary>
      /// Reads a file asynchronously and substitutes the matrix entries for characters.
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
      /// <returns>task with read data, BOM indicator, read length and used encoding.
      /// The array length is "file size * <paramref name="numPlaces"/>",
      /// which is probably larger than the read length.</returns>
      public static Task<(char[] Result, bool HasBom, int ReadLength, Encoding UsedEncoding)>
          ReadSubstitutedCleartextFileAsync(
              string filePath,
              Encoding encoding,
              SortedDictionary<char, char[]> substitutions,
              byte numPlaces,
              bool treatJAsI,
              bool toUpper) {
         return Task.Run(() =>
         {
            var result = FileHandler.ReadSubstitutedCleartextFile(
                filePath,
                encoding,
                substitutions,
                numPlaces,
                treatJAsI,
                toUpper,
                out bool hasBom,
                out int readLength,
                out Encoding usedEncoding);

            return (result, hasBom, readLength, usedEncoding);
         });
      }

      /// <summary>
      /// Writes an encrypted file asynchronously.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Wanted encoding of file.</param>
      /// <param name="withBom">Indicates, if a BOM must be written.</param>
      /// <param name="lineEndingOption">Indicates, which line ending shall be written.</param>
      /// <param name="data">Data to write.</param>
      /// <param name="writeLength">Number of characters to write.</param>
      /// <param name="groupSize">Size of output group (0, if no grouping should be done).</param>
      /// <param name="maxLineLength">Maximum line length (0, if there is no maximum line length)</param>

      public static Task WriteEncryptedFileAsync(
          string filePath,
          Encoding encoding,
          bool withBom,
          LineEndingHandler.Option lineEndingOption,
          char[] data,
          int writeLength,
          int groupSize,
          int maxLineLength) {
         return Task.Run(() =>
         {
            FileHandler.WriteEncryptedFile(
                filePath,
                encoding,
                withBom,
                lineEndingOption,
                data,
                writeLength,
                groupSize,
                maxLineLength);
         });
      }

      /// <summary>
      /// Writes a decrypted file asynchronously.
      /// </summary>
      /// <param name="filePath">File path.</param>
      /// <param name="encoding">Wanted encoding of file.</param>
      /// <param name="withBom">Indicates, if a BOM must be written.</param>
      /// <param name="lineEndingOption">Indicates, which line ending shall be written.</param>
      /// <param name="unsubstitutionMap">Mapping of matrix substitutions to characters.</param>
      /// <param name="data">Data to write.</param>
      /// <param name="writeLength">Number of characters to write.</param>
      public static Task WriteUnsubstitutedCleartextFileAsync(
          string filePath,
          Encoding encoding,
          bool withBom,
          LineEndingHandler.Option lineEndingOption,
          SortedDictionary<char[], char> unsubstitutionMap,
          char[] data,
          int writeLength) {
         return Task.Run(() =>
         {
            FileHandler.WriteUnsubstitutedCleartextFile(
                filePath,
                encoding,
                withBom,
                lineEndingOption,
                unsubstitutionMap,
                data,
                writeLength);
         });
      }
   }
}
