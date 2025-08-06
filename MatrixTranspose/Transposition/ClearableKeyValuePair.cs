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
 *    2025-07-24: V1.0.0: Created. fhs
 */

namespace TranspositionNS {
   /// <summary>
   /// Class representing a key-value pair that can be cleared.
   /// </summary>
   /// <typeparam name="K">Type of key.</typeparam>
   /// <typeparam name="V">Type of value.</typeparam>
   internal class ClearableKeyValuePair<K, V> {
      #region Public properties
      /// <summary>
      /// Key.
      /// </summary>
      public K Key { get; private set; }

      /// <summary>
      /// Value.
      /// </summary>
      public V Value { get; private set; }
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of the ClearableKeyValuePair.
      /// </summary>
      /// <param name="key">Key.</param>
      /// <param name="value">Value.</param>
      public ClearableKeyValuePair(K key, V value) {
         Key = key;
         Value = value;
      }
      #endregion


      #region Public methods
      /// <summary>
      /// Clears the key-value-pair to overwrite sensitive data.
      /// </summary>
      public void Clear() {
         Key = default;
         Value = default;
      }
      #endregion
   }
}
