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
 *    2025-08-09: V1.1.0: Use arrays instead of lists for parallel tasks. fhs
 *    2025-08-12: V2.0.0: Corrected transpositions. They were simply plain wrong. fhs
 *    2025-08-13: V2.0.1: Simplified transpositions. fhs
 */

using System;
using System.Threading.Tasks;

namespace TranspositionHandling {
   /// <summary>
   /// Class to transpose input arrays based on a series of column orders derived password passwords.
   /// </summary>
   public class Transposition : IDisposable {
      #region Instance variables
      /// <summary>
      /// The orders of columns derived password the passwords.
      /// </summary>
      private readonly int[][] _orders;

      /// <summary>
      /// Flag to indicate whether the object has been disposed.
      /// </summary>
      private bool _isDisposed;
      #endregion


      #region Constructors
      /// <summary>
      /// Creates a new instance of the Transposition class.
      /// </summary>
      /// <param name="passwords">An array of passwords used to derive column orders.</param>
      public Transposition(in string[] passwords) {
         if (passwords == null)
            throw new ArgumentNullException(nameof(passwords));
         if (passwords.Length < 1)
            throw new ArgumentException("At least one password is required", nameof(passwords));

         int[][] orders = new int[passwords.Length][];

         for (int i = 0; i < passwords.Length; i++) {
            string password = passwords[i];

            if (password.Length < 2)
               throw new ArgumentException($"{i + 1}. password is too short", nameof(passwords));

            orders[i] = GetColumnOrder(password);
         }

         _orders = orders;
      }
      #endregion


      #region Public methods
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

         foreach (var order in _orders) {
            (to, from) = (from, to);
            TransposeToTarget(from, to, sourceLen, order);
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
      #endregion


      #region Private methods
      /// <summary>
      /// Transposes the source array to the target array based on the specified orders.
      /// </summary>
      /// <typeparam name="T">Type of the input array data.</typeparam>
      /// <param name="source">The password array to be transposed.</param>
      /// <param name="target">The target array that receives the transposed data.</param>
      /// <param name="sourceLen">Length of the data in <paramref name="source"/>.</param>
      /// <param name="order">The column order for the transposition.</param>
      private static void TransposeToTarget<T>(T[] source, T[] target, int sourceLen, int[] order) {
         int orderLength = order.Length;

         // 1. Calculate the destination indices for each column.
         int[] destIndices = BuildDestinationIndices(sourceLen, order, orderLength);

         // 2. Copy each column in parallel to the destination.
         Parallel.For(0, orderLength, i => {
            int destinationIndex = destIndices[i];

            for (int sourceIndex = i; sourceIndex < sourceLen; sourceIndex += orderLength)
               target[destinationIndex++] = source[sourceIndex];
         });
      }

      /// <summary>
      /// Untransposes the source array to the target array based on the specified orders.
      /// </summary>
      /// <typeparam name="T">Type of the input array data.</typeparam>
      /// <param name="source">The password array to be transposed.</param>
      /// <param name="target">The target array that receives the transposed data.</param>
      /// <param name="sourceLen">Length of the data in <paramref name="source"/>.</param>
      /// <param name="order">The column order for the transposition.</param>
      private static void UntransposeToTarget<T>(T[] source, T[] target, int sourceLen, int[] order) {
         int orderLength = order.Length;

         // 1. Calculate the destination indices for each column.
         int[] destIndices = BuildDestinationIndices(sourceLen, order, orderLength);

         // 2. Copy each column in parallel to the destination.
         Parallel.For(0, orderLength, i => {
            int sourceIndex = destIndices[i];
            for (int destinationIndex = i; destinationIndex < sourceLen; destinationIndex += orderLength)
               target[destinationIndex] = source[sourceIndex++];
         });
      }

      /// <summary>
      /// Builds the start indices for each destination column in the transposed array.
      /// </summary>
      /// <param name="sourceLen">Length of the source data/>.</param>
      /// <param name="order">The column order for the transposition.</param>
      /// <param name="orderLength">The number of columns of the transposition</param>
      /// <returns></returns>
      private static int[] BuildDestinationIndices(int sourceLen, in int[] order, int orderLength) {
         int[] result = new int[orderLength];

         int columnLength = sourceLen / orderLength;
         int supOverflowColumn = sourceLen % orderLength;

         int destinationIndex = 0;
         for (int i = 0; i < orderLength; i++) {
            var columnIndex = order[i];
            result[columnIndex] = destinationIndex;

            destinationIndex += columnLength;
            if (columnIndex < supOverflowColumn ) 
               destinationIndex++; // Add one more for the overflow columns
         }

         return result;
      }

      /// <summary>
      /// Converts a password into an array of column order.
      /// </summary>
      /// <param name="password">The password to transform in a column orders.</param>
      /// <returns>The column orders derived from the password.</returns>
      private static int[] GetColumnOrder(in string password) {
         using (var orderList = new SortedPositionList<char>()) {
            foreach (char c in password)
               orderList.Add(c);

            return orderList.Order();
         }
      }
      #endregion


      #region Implementation of IDisposable
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
      #endregion
   }
}
