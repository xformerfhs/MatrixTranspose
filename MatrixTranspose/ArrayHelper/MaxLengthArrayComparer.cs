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

namespace ArrayHelper {
   public class MaxLengthArrayComparer<T> : Comparer<T[]> {
      private readonly int _maxLength;
      private readonly IComparer<T> _elementComparer;

      public MaxLengthArrayComparer(int maxLength, IComparer<T> elementComparer = null) {
         if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be non-negative.");

         _maxLength = maxLength;
         _elementComparer = elementComparer ?? Comparer<T>.Default;
      }

      public override int Compare(T[] x, T[] y) {
         if (ReferenceEquals(x, y)) return 0;
         if (x == null) return -1;
         if (y == null) return 1;

         int length = Math.Min(_maxLength, Math.Min(x.Length, y.Length));

         for (int i = 0; i < length; i++) {
            int cmp = _elementComparer.Compare(x[i], y[i]);
            if (cmp != 0)
               return cmp;
         }

         // If the maximum length has been reached the arrays are considered equal.
         if (length == _maxLength)
            return 0;

         // Compare lengths, if the arrays are shorther than _maxLength.
         return x.Length.CompareTo(y.Length);
      }
   }
}
