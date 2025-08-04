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
 *    2025-08-03: V1.0.0: Created. fhs
 */

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MatrixTranspose {
   public static class EncodingHelper {
      // ******** Private constants ********

      // Code pages

      private const int CP_UTF8 = 65001;
      private const int CP_UTF16_LE = 1200;
      private const int CP_UTF16_BE = 1201;
      private const int CP_UTF32_LE = 12000;
      private const int CP_UTF32_BE = 12001;

      /// <summary>
      /// Code pages that can have a BOM.
      /// </summary>
      private static readonly int[] _BomCodePages = {
         CP_UTF8,
         CP_UTF16_LE,
         CP_UTF16_BE,
         CP_UTF32_LE,
         CP_UTF32_BE
      };


      // ******** Public methods ********

      // The following methods do not use "(bool, int)" or "(bool, Encoding) as the
      // return values, as this means to refer to the results as "result.Item1" and
      // "result.Item2" which hides the meaning of the result. Multiple return values
      // are designed very badly in C#. Using "out" is definitely much more precise.

      /// <summary>
      /// Detects a BOM and returns either the corresponding codepage or <c>0</c>.
      /// </summary>
      /// <param name="filePath">Path of file to check.</param>
      /// <param name="codePage">Out: Detected codepage or <c>0</c>.</param>
      /// <returns><c>True</c>, if a BOM was detected; <c>False</c>, if not.</returns>
      public static bool DetectBomCodepage(in string filePath, out int codePage) {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException(@"File path must not be null or empty", nameof(filePath));

         using (var f = File.OpenRead(filePath))
            return DetectBomCodepage(f, out codePage);
      }

      /// <summary>
      /// Detects a BOM and returns either the corresponding codepage or <c>0</c>.
      /// </summary>
      /// <remarks>
      /// After the call to this method, the <paramref name="stream"/> is positioned
      /// after the BOM, if there was one, or where it was before, if there was none.
      /// </remarks>
      /// <param name="stream">Stream to read from.</param>
      /// <param name="codePage">Out: Detected codepage or <c>0</c>.</param>
      /// <returns><c>True</c>, if a BOM was detected; <c>False</c>, if not.</returns>
      public static bool DetectBomCodepage(in Stream stream, out int codePage) {
         if (stream == null)
            throw new ArgumentNullException(nameof(stream));
         if (!stream.CanSeek)
            throw new ArgumentException(@"Stream must be seekable");

         bool result = false;
         codePage = 0;
         long position = stream.Position;

         byte[] readBuffer = new byte[4];
         int readCount = stream.Read(readBuffer, 0, readBuffer.Length);

         if (readCount >= 2) {
            // The case statements are ordered by probability of occurrence.
            switch (readBuffer[0]) {
               case 0xEF:
                  // UTF-8: EF BB BF
                  if (readCount >= 3 && readBuffer[1] == 0xBB && readBuffer[2] == 0xBF) {
                     codePage = CP_UTF8;
                     position += 3;
                     result = true;
                  }
                  break;

               case 0xFF:
                  // UTF-16LE: FF FE
                  // UTF-32LE: FF FE 00 00
                  if (readBuffer[1] == 0xFE) {
                     if (readCount >= 4 && readBuffer[2] == 0x00 && readBuffer[3] == 0x00) {
                        codePage = CP_UTF32_LE;
                        position += 4;
                     } else {
                        codePage = CP_UTF16_LE;
                        position += 2;
                     }
                     result = true;
                  }
                  break;

               case 0xFE:
                  // UTF-16BE: FE FF
                  if (readBuffer[1] == 0xFF) {
                     codePage = CP_UTF16_BE;
                     position += 2;
                     result = true;
                  }
                  break;

               case 0x00:
                  // UTF-32BE: 00 00 FE FF
                  if (readCount >= 4 && readBuffer[1] == 0x00 && readBuffer[2] == 0xFE && readBuffer[3] == 0xFF) {
                     codePage = CP_UTF32_BE;
                     position += 4;
                     result = true;
                  }
                  break;
            }
         }

         stream.Seek(position, SeekOrigin.Begin);

         return result;
      }

      /// <summary>
      /// Detects a BOM and returns either the corresponding encoding or <c>null</c>.
      /// </summary>
      /// <param name="filePath">Path of file to check.</param>
      /// <param name="encoding">Out: The found encoding or <c>null</c>.</param>
      /// <returns><c>True</c>, if a BOM is detected; <c>False</c>, if not.</returns>
      public static bool DetectBomEncoding(in string filePath, out Encoding encoding) {
         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException(@"File path must not be null or empty", nameof(filePath));

         using (var f = File.OpenRead(filePath))
            return DetectBomEncoding(f, out encoding);
      }

      /// <summary>
      /// Detects a BOM and returns either the corresponding encoding or the default encoding.
      /// </summary>
      /// <param name="stream">Stream to read from.</param>
      /// <param name="encoding">Out: The found encoding or <c>null</c>.</param>
      /// <returns><c>True</c>, if a BOM is detected; <c>False</c>, if not.</returns>
      public static bool DetectBomEncoding(in Stream stream, out Encoding encoding) {
         if (stream == null)
            throw new ArgumentNullException(nameof(stream));

         bool result = DetectBomCodepage(stream, out int codePage);

         if (result)
            encoding = Encoding.GetEncoding(codePage);
         else
            encoding = null;

         return result;
      }

      /// <summary>
      /// Get the encoding that matches the value of <paramref name="withBom"/>.
      /// </summary>
      /// <param name="encoding">Encoding to be used as a base.</param>
      /// <param name="withBom"><c>True</c>, if a BOM must be written; <c>False</c>, if not.</param>
      /// <returns><paramref name="encoding"/>, if <paramref name="withBom"/> matches the BOM setting
      /// of <paramref name="encoding"/>. A new encoding of the same type as <paramref name="encoding"/>,
      /// but with the correct BOM setting.</returns>
      public static Encoding EncodingWithMatchingBom(in Encoding encoding, bool withBom) {
         if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));

         Encoding result = encoding;

         switch (encoding.CodePage) {
            case CP_UTF8:
               if (withBom) {
                  if (!encoding.WritesBom())
                     result = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
               } else {
                  if (encoding.WritesBom())
                     result = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
               }
               break;

            case CP_UTF16_LE:
               if (withBom) {
                  if (!encoding.WritesBom())
                     result = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
               } else {
                  if (encoding.WritesBom())
                     result = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
               }
               break;

            case CP_UTF16_BE:
               if (withBom) {
                  if (!encoding.WritesBom())
                     result = new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
               } else {
                  if (encoding.WritesBom())
                     result = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);
               }
               break;

            case CP_UTF32_LE:
               if (withBom) {
                  if (!encoding.WritesBom())
                     result = new UTF32Encoding(bigEndian: false, byteOrderMark: true);
               } else {
                  if (encoding.WritesBom())
                     result = new UTF32Encoding(bigEndian: false, byteOrderMark: false);
               }
               break;

            case CP_UTF32_BE:
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
      /// <returns><c>True</c>, if the codepage may have a BOM; <c>False</c>, if not.</returns>
      public static bool SupportsBom(int codePage) {
         return _BomCodePages.Contains(codePage);
      }
   }
}
