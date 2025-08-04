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

using System.Text;

namespace MatrixTranspose {
   /// <summary>
   /// Extension class for <see cref="Encoding"/>.
   /// </summary>
   public static class EncodingExtension {
      /// <summary>
      /// Reports, whether the encoding writes a BOM.
      /// </summary>
      /// <param name="encoding">Encoding to check.</param>
      /// <returns><c>True</c>, if it writes a BOM; <c>False</c>, if not.</returns>
      public static bool WritesBom(this Encoding encoding) {
         return encoding.GetPreamble().Length != 0;
      }

      /// <summary>
      /// Returns the minimum bytes per character of the encoding.
      /// </summary>
      /// <param name="encoding">Encoding.</param>
      /// <returns>Minimum bytes per character.</returns>
      public static int BytesPerCharacter(this Encoding encoding) {
         if (encoding.IsSingleByte)
            return 1;
         else
            if (encoding is UTF8Encoding)
               return 1;
            else
               if (encoding is UnicodeEncoding)
                  return 2;
               else
                  if (encoding is UTF32Encoding)
                     return 4;
                  else
                     return 1;  // This is the default. We assume 1 byte / character.
      }
   }
}
