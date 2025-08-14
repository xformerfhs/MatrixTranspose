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
 *    2025-08-14: V1.0.0: Created. fhs
 */

using LineEndingHandling;
using System;
using System.IO;

namespace WriteHandling {
   /// <summary>
   /// Class that implements grouping of characters and a maximum line length as a text writer.
   /// </summary>
   public class GroupingWriter : ChainableTextWriter {
      #region Private constants
      /// <summary>
      /// Error message for parameter being negative.
      /// </summary>
      private const string FormatErrorNotNegative = "{0} must not be negative.";
      #endregion


      #region Instance variables
      /// <summary>
      /// The size of the group. 0 means no grouping.
      /// </summary>
      private readonly int _groupSize;

      /// <summary>
      /// The maximum line length. 0 means no maximum line length.
      /// </summary>
      private readonly int _maxLineLength;

      /// <summary>
      /// Marks whether grouping is enabled.
      /// </summary>
      private readonly bool _hasGrouping;

      /// <summary>
      /// Marks whether a maximum line length is set.
      /// </summary>
      private readonly bool _hasMaxLineLength;

      /// <summary>
      /// Current position in the group.
      /// </summary>
      private int _groupPos = 0;

      /// <summary>
      /// Current position in the line.
      /// </summary>
      private int _linePos = 0;
      #endregion


      #region Public constructors
      public GroupingWriter(TextWriter innerWriter, int groupSize, int maxLineLength) : base(innerWriter) {
         if (groupSize < 0)
            throw new ArgumentOutOfRangeException(nameof(groupSize), string.Format(FormatErrorNotNegative, "Group size"));

         if (maxLineLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLineLength), string.Format(FormatErrorNotNegative, "Maximum line length"));

         _hasGrouping = groupSize > 0;
         _hasMaxLineLength = maxLineLength > 0;

         // Make maxLineLength a multiple of groupSize + 1 (+1 because of the space following the group).
         if (_hasGrouping && _hasMaxLineLength)
            maxLineLength = (maxLineLength / (groupSize + 1)) * (groupSize + 1);

         _groupSize = groupSize;
         _maxLineLength = maxLineLength;
      }
      #endregion


      #region Implementation of TextWriter
      public override void Write(char value) {
         if (_hasGrouping && (_groupPos >= _groupSize)) {
            _linePos++;

            if (!_hasMaxLineLength || (_linePos < _maxLineLength))
               _innerWriter.Write(' ');

            _groupPos = 0;
         }

         if (_hasMaxLineLength && (_linePos >= _maxLineLength)) {
            _innerWriter.Write(LineEndingHandler.LineFeed);
            _linePos = 0;
         }

         _innerWriter.Write(value);

         _groupPos++;
         _linePos++;
      }
      #endregion
   }
}
