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
 *    2025-08-11: V1.0.0: Created. fhs
 */

using System;
using System.Linq;
using System.Windows.Controls;

namespace MatrixTranspose {
   /// <summary>
   /// Helper class for Enum ComboBoxes.
   /// </summary>
   public static class EnumComboBoxHelper {
      /// <summary>
      /// Set up the Enum ComboBox.
      /// </summary>
      /// <typeparam name="T">Enum to fill the ComboBox with.</typeparam>
      /// <param name="comboBox">The ComboBox to fill.</param>
      /// <param name="textProvider">Function delegate to get a text for an Enum value.</param>
      public static void SetupEnumComboBox<T>(ComboBox comboBox, Func<T, string> textProvider)
          where T : Enum {
         var items = Enum.GetValues(typeof(T))
             .Cast<T>()
             .Select(value => new EnumItem<T>(value, textProvider(value)))
             .ToList();

         comboBox.ItemsSource = items;
         comboBox.DisplayMemberPath = "DisplayText";
         comboBox.SelectedValuePath = "Value";

         if (items.Any())
            comboBox.SelectedIndex = 0;
      }

      /// <summary>
      /// Get selected Enum value of a ComoboBox.
      /// </summary>
      /// <typeparam name="T">Enum type of the ComboBox.</typeparam>
      /// <param name="comboBox">The ComboBox to query.</param>
      /// <returns>Selected Enum value of the ComboBox</returns>
      public static T? GetSelectedEnumValue<T>(ComboBox comboBox) where T : struct, Enum {
         return comboBox.SelectedValue as T?;
      }
   }
}
