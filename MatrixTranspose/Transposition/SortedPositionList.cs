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

using System;
using System.Collections.Generic;

namespace TranspositionHandling {
   /// <summary>
   /// Class to sort positions of a list by their keys.
   /// </summary>
   /// <remarks>
   /// Multiple positions can be associated with the same key.
   /// </remarks>
   /// <typeparam name="K">Key type.</typeparam>
   internal class SortedPositionList<K> : IDisposable where K : IComparable {
      #region Instance variables
      /// <summary>
      /// The list of tuples containing keys and their corresponding indices.
      /// </summary>
      private LinkedList<ClearableKeyValuePair<K, int>> _list;

      /// <summary>
      /// Flag to indicate whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new instance of the SortedPositionList class.
      /// </summary>
      public SortedPositionList() {
         _list = new LinkedList<ClearableKeyValuePair<K, int>>();
      }
      #endregion


      #region Public methods
      /// <summary>
      /// Adds a new key and its index to the sorted list.
      /// </summary>
      /// <param name="key">The key to be used in sorting.</param>
      public void Add(K key) {
         var newItem = new ClearableKeyValuePair<K, int>(key, _list.Count);

         // Add new elements from the end of the list, so that elements
         // with the same key are added in the order they were added.
         var current = _list.Last;

         while (current != null &&
                current.Value.Key.CompareTo(key) > 0)
            current = current.Previous;

         if (current == null)
            // This key is smaller than all existing keys, so add it to the front.
            _list.AddFirst(newItem);
         else
            // The new key is larger than the current key. Insert the new item after the current item.
            _list.AddAfter(current, newItem);
      }

      /// <summary>
      /// Retrieves the positions of the items in the order of their keys.
      /// </summary>
      /// <returns>Array of the order of each position in the list of keys.</returns>
      public int[] PositionOrders() {
         int[] result = new int[_list.Count];

         int pos = 0;
         foreach (var item in _list)
            result[item.Value] = pos++;

         return result;
      }

      /// <summary>
      /// Clears the sorted index list.
      /// </summary>
      public void Clear() {
         foreach (var item in _list)
            item.Clear(); // Clear the key and index references

         _list.Clear(); // Clear the linked list
      }
      #endregion


      #region Public properties
      /// <summary>
      /// Number of items in the sorted index list.
      /// </summary>
      public int Count => _list.Count;
      #endregion


      #region Implementation of IDisposable
      /// <summary>
      /// Disposes the resources used by the SortedPositionList.
      /// </summary>
      protected virtual void Dispose(bool disposing) {
         if (_isDisposed)
            return;

         if (disposing) {
            Clear(); // Clear the list so sensitive data is overwritten and release the key-value-pairs.
            _list = null; // Set the list to null to release the list itself.
         }

         _isDisposed = true;
      }

      // // Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
      // ~SortedPositionList()
      // {
      //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      //     Dispose(disposing: false);
      // }

      public void Dispose() {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
      #endregion
   }
}
