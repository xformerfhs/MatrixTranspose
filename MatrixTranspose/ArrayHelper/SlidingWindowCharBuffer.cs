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

using System;

namespace ArrayHelper {
   /// <summary>
   /// Class to implement a sliding window buffer for characters.
   /// </summary>
   public class SlidingWindowCharBuffer : IDisposable {
      #region Instance variables
      /// <summary>
      /// Buffer to hold the characters.
      /// </summary>
      private readonly char[] _buffer;

      /// <summary>
      /// Marks whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;
      #endregion


      #region Public properties
      /// <summary>
      /// Length of the current content in the buffer.
      /// </summary>
      public int Length { get; private set; }

      /// <summary>
      /// Capacity of the buffer.
      /// </summary>
      public int Capacity => _buffer.Length;

      /// <summary>
      /// The raw character buffer for operations with the buffer.
      /// </summary>
      public char[] RawBuffer => _buffer;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of the SlidingWindowCharBuffer with the specified capacity.
      /// </summary>
      /// <param name="capacity">Capacity of buffer.</param>
      /// <exception cref="ArgumentOutOfRangeException">Thrown, if the capacity is 0 or less.</exception>
      public SlidingWindowCharBuffer(int capacity) {
         if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), @"Capacity must be > 0");

         _buffer = new char[capacity];
         Length = 0;
      }
      #endregion


      #region Public methods
      /// <summary>
      /// Adds a character to the end of the buffer.
      /// </summary>
      /// <param name="c">Character to add.</param>
      /// <returns><c>True</c>, if the character was added successfully; <c>False</c>, if the buffer is full.</returns>
      public bool AddChar(char c) {
         if (Length >= _buffer.Length)
            return false; // Buffer is full.

         _buffer[Length] = c;
         Length++;

         return true;
      }

      /// <summary>
      /// Removes the specified number of characters from the start of the buffer.
      /// </summary>
      public void RemoveFromStart(int count) {
         if (count <= 0 ||
             Length == 0)
            return;

         count = Math.Min(count, Length);

         // Shift the remaining characters to the start of the buffer.
         Array.Copy(_buffer, count, _buffer, 0, Length - count);

         // Correct length.
         Length -= count;
      }

      /// <summary>
      /// Converts the current content of the buffer to a string.
      /// </summary>
      public string ToStringContent() {
         return new string(_buffer, 0, Length);
      }

      /// <summary>
      /// Resets the buffer to be reused.
      /// </summary>
      public void Reset() {
         Length = 0;
      }

      /// <summary>
      /// Clears and resets the buffer.
      /// </summary>
      public void Clear() {
         Array.Clear(_buffer, 0, Length);
         Length = 0;
      }
      #endregion


      #region Implementation of IDisposable
      protected virtual void Dispose(bool disposing) {
         if (_isDisposed)
            return;

         if (disposing) {
            Array.Clear(_buffer, 0, _buffer.Length);
         }

         _isDisposed = true;
      }

      public void Dispose() {
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
      #endregion
   }
}
