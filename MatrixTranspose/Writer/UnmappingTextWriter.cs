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

using ArrayHelper;
using System;
using System.Collections.Generic;
using System.IO;

namespace Writer {
   /// <summary>
   /// Class that replaces a sequence of characters by a character according to a mapping.
   /// </summary>
   public class UnmappingTextWriter : ChainableTextWriter {
      #region Instance variables
      /// <summary>
      /// Mapping from character sequences to their replacements, sorted by the lengths of the sequences.
      /// </summary>
      private readonly SortedDictionary<int, SortedDictionary<char[], char>> _mappingBySize;

      /// <summary>
      /// Minimum length of a key in the mapping.
      /// </summary>
      private readonly int _minLength;

      /// <summary>
      /// Maximum length of a key in the mapping.
      /// </summary>
      private readonly int _maxLength;

      /// <summary>
      /// Sliding window buffer that holds the current sequence of characters being processed.
      /// </summary>
      private readonly SlidingWindowCharBuffer _buffer;

      /// <summary>
      /// Marks whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of <see cref="UnmappingTextWriter"/>.
      /// </summary>
      /// <param name="innerWriter">Inner text writer to which this writer writes.</param>
      /// <param name="mapping">Mapping from character sequences to their replacement characters.</param>
      public UnmappingTextWriter(TextWriter innerWriter, SortedDictionary<char[], char> mapping)
          : base(innerWriter) {
         if (mapping == null) throw new ArgumentNullException(nameof(mapping));

         _mappingBySize = new SortedDictionary<int, SortedDictionary<char[], char>>();

         int minLength = int.MaxValue;
         int maxLength = int.MinValue;

         // Group mappings by key length.
         foreach (var kvp in mapping) {
            char[] key = kvp.Key;
            char value = kvp.Value;
            int length = key.Length;

            minLength = Math.Min(minLength, length);
            maxLength = Math.Max(maxLength, length);

            if (!_mappingBySize.TryGetValue(length, out var sizeMapping)) {
               sizeMapping = new SortedDictionary<char[], char>(new MaxLengthArrayComparer<char>(length));
               _mappingBySize[length] = sizeMapping;
            }

            sizeMapping[key] = value;
         }

         _minLength = minLength;
         _maxLength = maxLength;

         // Create buffer with maximum key length.
         _buffer = new SlidingWindowCharBuffer(_maxLength);
      }
      #endregion


      #region Implementation of TextWriter
      public override void Write(char value) {
         // Add character to buffer.
         if (!_buffer.AddChar(value))
            // Buffer is full. That must never happen.
            throw new InvalidOperationException("Internal buffer overflow");

         // Try to find a match starting from the current buffer length down to minimum length
         for (int currentLength = Math.Min(_buffer.Length, _maxLength); currentLength >= _minLength; currentLength--) {
            if (_mappingBySize.TryGetValue(currentLength, out var sizeMapping) &&
                sizeMapping.TryGetValue(_buffer.RawBuffer, out char mappedChar)) {
               // Found a match - write the mapped character and reset buffer.
               _innerWriter.Write(mappedChar);
               _buffer.Reset();
               return;
            }
         }

         // If buffer has maximum possible length and we did not find a mappable sequence, we have an unmappable sequence.
         if (_buffer.Length == _maxLength)
            throw new InvalidOperationException($"Unmappable character sequence found: \"{_buffer.ToStringContent()}\"");
      }

      public override void Flush() {
         // If there are remaining characters in buffer when flushing, this is an error.
         if (_buffer.Length > 0)
            throw new InvalidOperationException($"Incomplete character sequence at end of stream: \"{_buffer.ToStringContent()}\"");

         _innerWriter.Flush();
      }
      #endregion


      #region Implementation of IDisposable
      protected override void Dispose(bool disposing) {
         if (_isDisposed)
            return;

         if (disposing) {
            try {
               Flush();
            } catch {
               // Ignore flush errors during disposal.
            }

            _buffer?.Dispose();
         }

         base.Dispose(disposing);

         _isDisposed = true;
      }
      #endregion
   }
}
