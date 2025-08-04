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

using System.IO;

namespace Reader {
   /// <summary>
   /// Class to normalize line endings in a text reader.
   /// </summary>
   public class LineEndingNormalizingTextReader : ChainableTextReader {
      // ******** Instance Variables ********

      /// <summary>
      /// Remember if the last character read was a carriage return (CR).
      /// </summary>
      private bool lastWasCR = false;

      /// <summary>
      /// Peeked character, if any.
      /// </summary>
      private int? peekedChar;


      // ******** Constructors ********

      /// <summary>
      /// Creates a new instance of the <see cref="LineEndingNormalizingTextReader"/> class.
      /// </summary>
      public LineEndingNormalizingTextReader(TextReader innerReader) : base(innerReader) { }


      // ******** Implementation of TextReader ********

      public override int Read() {
         // 1. Check if we have a peeked character and return it if available.
         if (peekedChar.HasValue) {
            int result = peekedChar.Value;
            peekedChar = null;
            return result;
         }

         // 2. Read character.
         return ReadInternal();
      }

      public override int Peek() {
         // 1. Check if we have a peeked character and return it if available.
         if (peekedChar.HasValue)
            return peekedChar.Value;

         // 2. Read character and store it for Read and Peek.
         peekedChar = ReadInternal();

         return peekedChar.Value;
      }


      // ******** Private Methods ********

      /// <summary>
      /// Reads a character from the inner reader, normalizing line endings.
      /// </summary>
      private int ReadInternal() {
         int ch = _innerReader.Read();

         if (ch == -1)
            return ch;

         char c = (char)ch;

         if (lastWasCR) {
            lastWasCR = false;

            if (c == '\n') {
               // CRLF -> skip the LF, we already returned LF for CR
               return ReadInternal();
            } else {
               // CR followed by something else, process this character
               if (c == '\r') {
                  lastWasCR = true;
                  return '\n';
               }

               return c;
            }
         }

         if (c == '\r') {
            lastWasCR = true;
            return '\n';
         }

         return c;
      }
   }
}
