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

namespace MathHelperNS {
   /// <summary>
   /// Class to provide mathematical helper functions.
   /// </summary>
   public static class MathHelper {
      #region Public static methods
      /// <summary>
      /// Calculates the greatest common divisor (GCD) of two integers using the Euclidean algorithm.
      /// </summary>
      /// <param name="a">First integer.</param>
      /// <param name="b">Second integer.</param>
      /// <returns>Greatest common divisor of <paramref name="a"/> and <paramref name="b"/>.</returns>
      public static int Gcd(int a, int b) {
         while (b != 0) {
            int temp = b;
            b = a % b;
            a = temp;
         }

         return a;
      }
      #endregion
   }
}
