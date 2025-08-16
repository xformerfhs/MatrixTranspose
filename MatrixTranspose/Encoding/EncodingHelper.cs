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
 *    2025-08-05: V1.0.0: Created. fhs
 *    2025-08-15: V2.0.0: Return multiple values, where appropriate. fhs
 */

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace EncodingHandling {
   /// <summary>
   /// Class that implements helper methods for <see cref="Encoding"/> handling.
   /// </summary>
   public static class EncodingHelper {
      #region Private constants
      // Code pages

      private const int CodepageUtf8 = 65001;
      private const int CodepageUtf16Le = 1200;
      private const int CodepageUtf16Be = 1201;
      private const int CodepageUtf32Le = 12000;
      private const int CodepageUtf32Be = 12001;

      /// <summary>
      /// Code pages that can have a BOM.
      /// </summary>
      private static readonly int[] BomCodePages = {
         CodepageUtf8,
         CodepageUtf16Le,
         CodepageUtf16Be,
         CodepageUtf32Le,
         CodepageUtf32Be
      };
      #endregion


      #region Public static methods
      /// <summary>
      /// Detects a BOM and returns either the corresponding codepage or <c>0</c>.
      /// </summary>
      /// <param name="filePath">Path of file to check.</param>
      /// <returns>
      /// <see langword="true"/>, if a BOM was detected; <see langword="false"/>, if not.
      /// The second return value is the code page or <c>0</c>, if no BOM was detected.
      /// </returns>
      public static (bool, int) DetectBomCodepage(in string filePath) {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path must not be null or empty", nameof(filePath));

         using (var f = File.OpenRead(filePath))
            return DetectBomCodepageNoCheck(f);
      }

      /// <summary>
      /// Detects a BOM and returns either the corresponding codepage or <c>0</c>.
      /// </summary>
      /// <remarks>
      /// After the call to this method, the <paramref name="stream"/> is positioned
      /// after the BOM, if there was one, or where it was before, if there was none.
      /// </remarks>
      /// <param name="stream">Stream to read from.</param>
      /// <returns>
      /// <see langword="true"/>, if a BOM was detected; <see langword="false"/>, if not.
      /// The second return value is the code page or <c>0</c>, if no BOM was detected.
      /// </returns>
      public static (bool, int) DetectBomCodepage(in Stream stream) {
         if (stream == null)
            throw new ArgumentNullException(nameof(stream));
         if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable");

         return DetectBomCodepageNoCheck(stream);
      }

      /// <summary>
      /// Detects a BOM and returns either the corresponding encoding or <see langword="null"/>.
      /// </summary>
      /// <param name="filePath">Path of file to check.</param>
      /// <returns>
      /// <see langword="true"/>, if a BOM is detected; <see langword="false"/>, if not. The second return value
      /// is the encoding, if a BOM was found or <see langword="null"/>.</returns>
      public static (bool, Encoding) DetectBomEncoding(in string filePath) {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path must not be null or empty", nameof(filePath));

         using (var f = File.OpenRead(filePath))
            return DetectBomEncodingNoCheck(f);
      }

      /// <summary>
      /// Detects a BOM and returns either the corresponding encoding or the default encoding.
      /// </summary>
      /// <param name="stream">Stream to read from.</param>
      /// <returns>
      /// <see langword="true"/>, if a BOM is detected; <see langword="false"/>, if not.
      /// The second return value is the found <see cref="Encoding"/> or <see langword="null"/>,
      /// if none was found.
      /// </returns>
      public static (bool, Encoding) DetectBomEncoding(in Stream stream) {
         if (stream == null)
            throw new ArgumentNullException(nameof(stream));

         return DetectBomEncodingNoCheck(stream);
      }

      /// <summary>
      /// Get the encoding that matches the value of <paramref name="withBom"/>.
      /// </summary>
      /// <param name="encoding">Encoding to be used as a base.</param>
      /// <param name="withBom"><see langword="true"/>, if a BOM must be written; <see langword="false"/>, if not.</param>
      /// <returns>
      /// <paramref name="encoding"/>, if <paramref name="withBom"/> matches the BOM setting
      /// of <paramref name="encoding"/>. A new encoding of the same type as <paramref name="encoding"/>,
      /// but with the correct BOM setting.
      /// </returns>
      public static Encoding EncodingWithMatchingBom(in Encoding encoding, bool withBom) {
         if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));

         Encoding result = encoding;

         switch (encoding.CodePage) {
            case CodepageUtf8:
               if (withBom) {
                  if (!encoding.WritesBom())
                     result = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
               } else {
                  if (encoding.WritesBom())
                     result = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
               }
               break;

            case CodepageUtf16Le:
               if (withBom) {
                  if (!encoding.WritesBom())
                     result = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
               } else {
                  if (encoding.WritesBom())
                     result = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
               }
               break;

            case CodepageUtf16Be:
               if (withBom) {
                  if (!encoding.WritesBom())
                     result = new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
               } else {
                  if (encoding.WritesBom())
                     result = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);
               }
               break;

            case CodepageUtf32Le:
               if (withBom) {
                  if (!encoding.WritesBom())
                     result = new UTF32Encoding(bigEndian: false, byteOrderMark: true);
               } else {
                  if (encoding.WritesBom())
                     result = new UTF32Encoding(bigEndian: false, byteOrderMark: false);
               }
               break;

            case CodepageUtf32Be:
               if (withBom) {
                  if (!encoding.WritesBom())
                     result = new UTF32Encoding(bigEndian: true, byteOrderMark: true);
               } else {
                  if (encoding.WritesBom())
                     result = new UTF32Encoding(bigEndian: true, byteOrderMark: false);
               }
               break;
         }

         return result;
      }

      /// <summary>
      /// Checks, if the given codepage can have a BOM.
      /// </summary>
      /// <param name="codePage">Codepage to check.</param>
      /// <returns>
      /// <see langword="true"/>, if the codepage may have a BOM; <see langword="false"/>, if not.
      /// </returns>
      public static bool SupportsBom(int codePage) {
         return BomCodePages.Contains(codePage);
      }
      #endregion


      #region Private static methods
      /// <summary>
      /// Detects a BOM and returns either the corresponding codepage or <c>0</c>.
      /// This method assumes that the arguments are already checked.
      /// </summary>
      /// <remarks>
      /// After the call to this method, the <paramref name="stream"/> is positioned
      /// after the BOM, if there was one, or where it was before, if there was none.
      /// </remarks>
      /// <param name="stream">Stream to read from.</param>
      /// <param name="codePage">Out: Detected codepage or <c>0</c>.</param>
      /// <returns>
      /// <see langword="true"/>, if a BOM was detected; <see langword="false"/>, if not.
      /// The second return value is the code page or <c>0</c>, if no BOM was detected.
      /// </returns>
      public static (bool, int) DetectBomCodepageNoCheck(in Stream stream) {
         bool result = false;
         int codePage = 0;
         long position = stream.Position;

         byte[] readBuffer = new byte[4];
         int readCount = stream.Read(readBuffer, 0, readBuffer.Length);

         if (readCount >= 2) {
            // The case statements are ordered by probability of occurrence.
            switch (readBuffer[0]) {
               case 0xEF:
                  // UTF-8: EF BB BF
                  if (readCount >= 3 && readBuffer[1] == 0xBB && readBuffer[2] == 0xBF) {
                     codePage = CodepageUtf8;
                     position += 3;
                     result = true;
                  }
                  break;

               case 0xFF:
                  // UTF-16LE: FF FE
                  // UTF-32LE: FF FE 00 00
                  if (readBuffer[1] == 0xFE) {
                     if (readCount >= 4 && readBuffer[2] == 0x00 && readBuffer[3] == 0x00) {
                        codePage = CodepageUtf32Le;
                        position += 4;
                     } else {
                        codePage = CodepageUtf16Le;
                        position += 2;
                     }
                     result = true;
                  }
                  break;

               case 0xFE:
                  // UTF-16BE: FE FF
                  if (readBuffer[1] == 0xFF) {
                     codePage = CodepageUtf16Be;
                     position += 2;
                     result = true;
                  }
                  break;

               case 0x00:
                  // UTF-32BE: 00 00 FE FF
                  if (readCount >= 4 && readBuffer[1] == 0x00 && readBuffer[2] == 0xFE && readBuffer[3] == 0xFF) {
                     codePage = CodepageUtf32Be;
                     position += 4;
                     result = true;
                  }
                  break;
            }
         }

         stream.Seek(position, SeekOrigin.Begin);

         return (result, codePage);
      }

      /// <summary>
      /// Detects a BOM and returns either the corresponding encoding or the default encoding.
      /// This method assumes that the arguments are already checked.
      /// </summary>
      /// <param name="stream">Stream to read from.</param>
      /// <returns>
      /// <see langword="true"/>, if a BOM is detected; <see langword="false"/>, if not.
      /// The second return value is the <see cref="Encoding"/>, or <see langword="null"/>,
      /// if no BOM was detected.
      /// </returns>
      public static (bool, Encoding) DetectBomEncodingNoCheck(in Stream stream) {
         bool hasBom;
         int codepage;
         (hasBom, codepage) = DetectBomCodepageNoCheck(stream);

         if (hasBom)
            return (true, Encoding.GetEncoding(codepage));
         else
            return (false, null);
      }
      #endregion
   }
}
