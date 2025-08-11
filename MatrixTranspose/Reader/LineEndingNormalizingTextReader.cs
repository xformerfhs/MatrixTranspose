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
 *    2025-08-11: V2.0.0: Handle "Next Line" control character. fhs
 */

using LineEndingHandling;
using System.IO;

namespace ReadHandling {
   /// <summary>
   /// Class to normalize line endings in a text reader.
   /// It converts CR, LF, CR/LF and NL to LF and leaves all other characters unchanged.
   /// </summary>
   public class LineEndingNormalizingTextReader : ChainableTextReader {
      #region Instance variables
      /// <summary>
      /// Remember if the last character read was a carriage return (CR).
      /// </summary>
      private bool _lastWasCarriageReturn = false;

      /// <summary>
      /// Peeked character, if any.
      /// </summary>
      private int? _peekedChar;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of the <see cref="LineEndingNormalizingTextReader"/> class.
      /// </summary>
      public LineEndingNormalizingTextReader(TextReader innerReader) : base(innerReader) { }
      #endregion


      #region Implementation of TextReader
      public override int Read() {
         // 1. If there is no peeked character read the next one.
         if (!_peekedChar.HasValue)
            return ReadInternal();

         // 2. There is a peeked character: Return it.
         int result = _peekedChar.Value;
         _peekedChar = null;
         return result;
      }

      public override int Peek() {
         // 1. Check if we have a peeked character and return it if available.
         if (_peekedChar.HasValue)
            return _peekedChar.Value;

         // 2. Read character and store it for Read and Peek.
         _peekedChar = ReadInternal();

         return _peekedChar.Value;
      }
      #endregion


      #region Private Methods
      /// <summary>
      /// Reads a character from the inner reader, normalizing line endings.
      /// </summary>
      private int ReadInternal() {
         int ch = _innerReader.Read();

         // 1. Return immediately, if we have a NMC.
         if (ch == NoMoreCharacters)
            return ch;

         char c = (char)ch;

         // 2. Check, if we have a CR/LF pair.
         if (_lastWasCarriageReturn) {
            _lastWasCarriageReturn = false;

            // CR/LF -> Skip the LF, we already returned LF for CR.
            if (c == LineEndingHandler.LineFeed)
               return ReadInternal();  // This recursion will happen only once as _lastWasCarriageReturn is false now.
         }

         // 3. Process the character, if it is not the LF of a CR/LF pair.
         switch (c) {
            // If we have a CR, remember it for CR/LF processing and return LF, instead.
            case LineEndingHandler.CarriageReturn:
               _lastWasCarriageReturn = true;
               return LineEndingHandler.LineFeed;

            // If it is a NL, change it to a LF.
            case LineEndingHandler.NextLine:
               return LineEndingHandler.LineFeed;

            // Leave all other characters, as they are.
            default:
               return ch;
         }
      }
      #endregion
   }
}
