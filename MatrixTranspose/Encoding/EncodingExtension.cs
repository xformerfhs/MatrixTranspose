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
 *    2025-08-10: V2.0.0: Added "IsEbcdic". fhs
 */

using StringHandling;
using System;
using System.Text;

namespace EncodingHandling {
   /// <summary>
   /// Extension class for <see cref="Encoding"/>.
   /// </summary>
   public static class EncodingExtension {
      #region Public static methods
      /// <summary>
      /// Returns the minimum bytes per character of the encoding.
      /// </summary>
      /// <param name="encoding">Encoding.</param>
      /// <returns>The minimum number of bytes per character.</returns>
      public static int BytesPerCharacter(this Encoding encoding) {
         if (encoding.IsSingleByte)
            return 1;

         switch (encoding) {
            case UTF8Encoding _:
               return 1;

            case UnicodeEncoding _:
               return 2;

            case UTF32Encoding _:
               return 4;

            default:
               return 1;  // This is the default. We assume 1 byte / character.
         }
      }

      /// <summary>
      /// Reports, whether the encoding is an EBCDIC encoding.
      /// </summary>
      /// <returns>
      /// <see langword="true"/>, if the encoding is an EBCDIC encoding;
      /// otherwise <see langword="false"/>.
      /// </returns>
      public static bool IsEbcdic(this Encoding encoding) {
         return encoding.EncodingName.Contains("EBCDIC", StringComparison.InvariantCultureIgnoreCase);
      }

      /// <summary>
      /// Reports, whether the encoding writes a BOM.
      /// </summary>
      /// <param name="encoding">Encoding to check.</param>
      /// <returns><see langword="true"/>, if it writes a BOM; <see langword="false"/>, if not.</returns>
      public static bool WritesBom(this Encoding encoding) {
         return encoding.GetPreamble().Length != 0;
      }
      #endregion
   }
}
