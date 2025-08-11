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
 *    2025-08-10: V2.0.0: Added "Contains" with "StringComparison". fhs
 */

using System;
using System.Linq;

namespace StringHandling {
   public static class StringExtension {
      /// <summary>
      /// Returns a value indicating whether a specified substring occurs within this string.
      /// The comparison is made with the specified <see cref="StringComparison"/>.
      /// </summary>
      /// <param name="value">String to search for.</param>
      /// <param name="comparison">The <see cref="StringComparison"/> to use.</param>
      /// <returns><c>true</c>, if the <paramref name="value"/> parameter occurs
      /// within this string, or if <paramref name="value"/> is the empty string ("");
      /// otherwise <c>false</c>.
      /// </returns>
      public static bool Contains(this string self, string value, StringComparison comparison) {
         return self?.IndexOf(value, comparison) >= 0;
      }
      
      /// <summary>
             /// Removes all whitespace characters from the string.
             /// </summary>
             /// <param name="self">String to process.</param>
             /// <returns>New string with all whitespace characters removed.</returns>
      public static string RemoveWhiteSpace(this string self) {
         return new string(self.Where(c => !char.IsWhiteSpace(c)).ToArray());
      }
   }
}
