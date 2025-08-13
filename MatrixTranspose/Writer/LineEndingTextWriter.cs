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
 *    2025-08-11: V2.0.0: Set type of line ending to write. fhs
 */

using LineEndingHandling;
using System;
using System.IO;

namespace WriteHandling {
   /// <summary>
   /// Class that converts line endings from LF to a specified line ending.
   /// </summary>
   public class LineEndingTextWriter : ChainableTextWriter {
      #region Instance variables
      /// <summary>
      /// The line ending character to put out.
      /// </summary>
      private readonly char _lineEndingChar;

      /// <summary>
      /// Indicate, if the line ending character shall be prefixed with a CR.
      /// </summary>
      private readonly bool _prefixWithCarriageReturn = false;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new instance of <see cref="LineEndingTextWriter"/>.
      /// </summary>
      public LineEndingTextWriter(TextWriter innerWriter, LineEndingHandler.Option lineEndingOption) : base(innerWriter) {
         switch (lineEndingOption) {
            case LineEndingHandler.Option.Windows:
               _lineEndingChar = LineEndingHandler.LineFeed;
               _prefixWithCarriageReturn = true;
               break;

            case LineEndingHandler.Option.Unix:
               _lineEndingChar = LineEndingHandler.LineFeed;
               break;

            case LineEndingHandler.Option.OldMac:
               _lineEndingChar = LineEndingHandler.CarriageReturn;
               break;

            case LineEndingHandler.Option.Ebcdic:
               _lineEndingChar = LineEndingHandler.NextLine;
               break;

            default:
               throw new ArgumentException("Unknown line ending option.", nameof(lineEndingOption));
         }
      }
      #endregion


      #region Implementation of TextWriter
      public override void Write(char value) {
         // Replace a LF with the wanted line ending.
         if (value == LineEndingHandler.LineFeed) {
            if (_prefixWithCarriageReturn)
               _innerWriter.Write(LineEndingHandler.CarriageReturn);
            
            _innerWriter.Write(_lineEndingChar);
         } else
            _innerWriter.Write(value);  // Other characters are not changed.
      }
      #endregion
   }
}
