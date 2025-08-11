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
   using System.Collections.Generic;

namespace ArrayHandling {
   /// <summary>
   /// A class to compare arrays up to a given maximum length.
   /// </summary>
   public class MaxLengthArrayComparer<T> : Comparer<T[]> {
      #region Instance variables
      /// <summary>
      /// Element comparer.
      /// </summary>
      private readonly IComparer<T> _elementComparer;

      /// <summary>
      /// The maximum length to compare.
      /// </summary>
      private readonly int _maxLength;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates an instance of <see cref="MaxLengthArrayComparer{T}"/>.
      /// </summary>
      /// <param name="maxLength">The maximum length to compare.</param>
      /// <param name="elementComparer">Element comparer to use. If <c>null</c> the default comparer is used.</param>
      public MaxLengthArrayComparer(int maxLength, IComparer<T> elementComparer = null) {
         if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be non-negative.");

         _maxLength = maxLength;
         _elementComparer = elementComparer ?? Comparer<T>.Default;
      }
      #endregion

      #region Public overridden methods
      public override int Compare(T[] x, T[] y) {
         if (ReferenceEquals(x, y))
            return 0;
         if (x == null)
            return -1;
         if (y == null)
            return 1;

         int length = Math.Min(_maxLength, Math.Min(x.Length, y.Length));

         for (int i = 0; i < length; i++) {
            int cmp = _elementComparer.Compare(x[i], y[i]);
            if (cmp != 0)
               return cmp;
         }

         // If the maximum length has been reached the arrays are considered equal.
         if (length == _maxLength)
            return 0;

         // Compare lengths, if the arrays are shorter than _maxLength.
         return x.Length.CompareTo(y.Length);
      }
      #endregion
   }
}
