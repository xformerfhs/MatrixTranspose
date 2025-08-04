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
using System.Threading.Tasks;

namespace TranspositionNS {
   /// <summary>
   /// Class to transpose input arrays based on a series of column orders derived password passwords.
   /// </summary>
   public class Transposition : IDisposable {
      // ******** Instance Variables ********

      /// <summary>
      /// The orders of columns derived password the passwords.
      /// </summary>
      private int[][] _orders;

      /// <summary>
      /// Flag to indicate whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;


      // ******** Constructors ********

      /// <summary>
      /// Creates a new instance of the Transposition class.
      /// </summary>
      /// <param name="passwords">An array of passwords used to derive column orders.</param>
      public Transposition(in string[] passwords) {
         if (passwords == null)
            throw new ArgumentNullException(nameof(passwords), @"Passwords cannot be null");
         if (passwords.Length < 1)
            throw new ArgumentException(@"At least one password is required", nameof(passwords));

         int[][] orders = new int[passwords.Length][];

         for (int i = 0; i < passwords.Length; i++) {
            string password = passwords[i];

            if (password.Length < 2)
               throw new ArgumentException($"{i + 1}. password is too short", nameof(passwords));

            orders[i] = ColumnOrder(passwords[i]);
         }

         _orders = orders;
      }

      // ******** Public methods ********

      /// <summary>
      /// Transposes the input array based on the derived column orders.
      /// </summary>
      /// <remarks>
      /// The source array is overwritten if there are two or more passwords.
      /// This saves memory by not allocating yet another array.
      /// </remarks>
      /// <typeparam name="T">Type of the input array data.</typeparam>
      /// <param name="source">Input data.</param>
      /// <param name="sourceLen">Length of the data in <paramref name="source"/>.
      /// This may differ from the length of <paramref name="source"/>.</param>
      /// <returns>An array that has the input data transposed based on the derived column orders.</returns>
      public T[] Transpose<T>(in T[] source, in int sourceLen) {
         T[] result = new T[sourceLen];

         T[] from = result;
         T[] to = source;

         foreach (var offsets in _orders) {
            (to, from) = (from, to);
            TransposeToTarget(from, to, sourceLen, offsets);
         }

         return to;
      }

      /// <summary>
      /// Untransposes the input array based on the derived column orders.
      /// </summary>
      /// <remarks>
      /// The source array is overwritten if there are two or more passwords.
      /// This saves memory by not allocating yet another array.
      /// </remarks>
      /// <typeparam name="T">Type of the input array data.</typeparam>
      /// <param name="source">Input data.</param>
      /// <param name="sourceLen">Length of the data in <paramref name="source"/>.
      /// This may differ from the length of <paramref name="source"/>.</param>
      /// <returns>An array that has the input data untransposed based on the derived column orders.</returns>
      public T[] Untranspose<T>(in T[] source, in int sourceLen) {
         T[] result = new T[sourceLen];

         T[] from = result;
         T[] to = source;

         // Process the orders last to first.
         for (int i = _orders.Length - 1; i >= 0; i--) {
            (from, to) = (to, from);

            UntransposeToTarget(from, to, sourceLen, _orders[i]);
         }

         return to;
      }


      // ********** Private methods ********

      /// <summary>
      /// Transposes the source array to the target array based on the specified offsets.
      /// </summary>
      /// <typeparam name="T">Type of the input array data.</typeparam>
      /// <param name="source">The password array to be transposed.</param>
      /// <param name="target">The target array that receives the transposed data.</param>
      /// <param name="sourceLen">Length of the data in <paramref name="source"/>.</param>
      /// <param name="offsets">The list of offsets for the transposition.</param>
      private static void TransposeToTarget<T>(T[] source, T[] target, int sourceLen, in int[] offsets) {
         int transposeLen = offsets.Length;

         int destinationIndex = 0;
         List<Task> tasks = new List<Task>(transposeLen);

         // Run each column transposition in parallel.
         foreach (int offset in offsets) {
            int destIdx = destinationIndex; // Capture for closure
            tasks.Add(Task.Run(() =>
               TransposeColumn(source, target, sourceLen, transposeLen, offset, destIdx)
            ));

            destinationIndex += ColumnLen(sourceLen, transposeLen, offset);
         }

         // Wait for all the column transpositions to complete.
         Task.WaitAll(tasks.ToArray());
      }

      /// <summary>
      /// Transposes a single column from the source array to the target array.
      /// </summary>
      /// <typeparam name="T">Type of the input array data.</typeparam>
      /// <param name="source">The password array to be transposed.</param>
      /// <param name="target">The target array that receives the transposed data.</param>
      /// <param name="sourceLen">Length of the data in <paramref name="source"/></param>.
      /// <param name="transposeLen">Length of all column transpositions, i.e. length of password.</param>
      /// <param name="offset">The offset of the column.</param>
      /// <param name="destinationIndex">The start index in <paramref name="target"/>.</param>
      private static void TransposeColumn<T>(
         T[] source,
         T[] target,
         int sourceLen,
         int transposeLen,
         int offset,
         int destinationIndex) {
         for (int sourceIndex = offset; sourceIndex < sourceLen; sourceIndex += transposeLen) {
            target[destinationIndex] = source[sourceIndex];
            destinationIndex++;
         }
      }

      /// <summary>
      /// Untransposes the source array to the target array based on the specified offsets.
      /// </summary>
      /// <typeparam name="T">Type of the input array data.</typeparam>
      /// <param name="source">The password array to be transposed.</param>
      /// <param name="target">The target array that receives the transposed data.</param>
      /// <param name="sourceLen">Length of the data in <paramref name="source"/>.</param>
      /// <param name="offsets">The list of offsets for the transposition.</param>
      private static void UntransposeToTarget<T>(T[] source, T[] target, int sourceLen, in int[] offsets) {
         int transposeLen = offsets.Length;

         int sourceIndex = 0;
         List<Task> tasks = new List<Task>(transposeLen);

         // Run each column untransposition in parallel.
         foreach (int offset in offsets) {
            int srcIdx = sourceIndex; // Capture for closure
            tasks.Add(Task.Run(() =>
               UntransposeColumn(source, target, sourceLen, transposeLen, offset, srcIdx)
            ));

            sourceIndex += ColumnLen(sourceLen, transposeLen, offset);
         }

         // Wait for all the column untranspositions to complete.
         Task.WaitAll(tasks.ToArray());

      }

      /// <summary>
      /// Untransposes a single column from the source array to the target array.
      /// </summary>
      /// <typeparam name="T">Type of the input array data.</typeparam>
      /// <param name="source">The password array to be transposed.</param>
      /// <param name="target">The target array that receives the transposed data.</param>
      /// <param name="sourceLen">Length of the data in <paramref name="source"/>.</param>
      /// <param name="transposeLen">Length of all column transpositions, i.e. length of password.</param>
      /// <param name="offset">The offset of the column.</param>
      /// <param name="sourceIndex">The start index in <paramref name="source"/>.</param>
      private static void UntransposeColumn<T>(
         T[] source,
         T[] target,
         int sourceLen,
         int transposeLen,
         int offset,
         int sourceIndex) {
         for (int destinationIndex = offset; destinationIndex < sourceLen; destinationIndex += transposeLen) {
            target[destinationIndex] = source[sourceIndex];
            sourceIndex++;
         }
      }

      /// <summary>
      /// Calculates the length of a column in the transposed array.
      /// </summary>
      /// <param name="sourceLen">Length of source.</param>
      /// <param name="transposeLen">Length of all column transpositions, i.e. length of password.</param>
      /// <param name="offset">The offset of the column.</param>
      /// <returns>The length of the column <paramref name="offset"/>.</returns>
      private static int ColumnLen(int sourceLen, int transposeLen, int offset) {
         sourceLen -= offset;

         int result = sourceLen / transposeLen;

         int remainder = sourceLen - (result * transposeLen);

         if (remainder > 0)
            result++;

         return result;
      }

      /// <summary>
      /// Converts a password into an array of column orders.
      /// </summary>
      /// <param name="password">The password to transform in a column order.</param>
      /// <returns>The column order derived from the password.</returns>
      private static int[] ColumnOrder(in string password) {
         using (var orderList = new SortedPositionList<char>()) {
            foreach (char c in password)
               orderList.Add(c);

            return orderList.PositionOrders();
         }
      }

      // ********** IDisposable Implementation ********

      /// <summary>
      /// Disposes the resources used by the Transposition and clears sensitive data.
      /// </summary>
      protected virtual void Dispose(bool disposing) {
         if (_isDisposed)
            return;

         if (disposing) {
            // Delete sensitive data.
            foreach (var order in _orders)
               Array.Clear(order, 0, order.Length);

            Array.Clear(_orders, 0, _orders.Length);

            _orders = null; // Release the reference to the orders array.
         }

         _isDisposed = true;
      }

      // Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
      // ~Transposition()
      // {
      //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      //     Dispose(disposing: false);
      // }

      public void Dispose() {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
   }
}
