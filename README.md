# MatrixTranspose

This is a small program that features a combined [Polybios (matrix) substitution](https://en.wikipedia.org/wiki/Polybius_square) and [transposition](https://en.wikipedia.org/wiki/Transposition_cipher) cipher.

It is intended for educational purposes.
It shows how cryptography works and how the security or insecurity of a cipher occurs.
Modern ciphers like [ChaCha20-Poly1305](https://en.wikipedia.org/wiki/ChaCha20-Poly1305) or [AES-GCM](https://en.wikipedia.org/wiki/Galois/Counter_Mode) are far better suited for doing real encryption.

Here is how the encryption in this program works:

First each letter of the clear text is substituted by 2 or more letters (i.e. [fractionation](https://en.wikipedia.org/wiki/Transposition_cipher#Fractionation)) and then the letters of this substitution cipher are transposed a number of times.

This has the effect that the characters of the multi-letter-substitutions are ripped apart, making it very hard to crack the cipher.
I.e. it is hard if the clear text is long and there are at least two transpositions with very long random passwords.

An example of this cipher was the German [ADFG(V)X cipher](https://en.wikipedia.org/wiki/ADFGVX_cipher) used during World War I.

The cipher needs two categories of passwords:

1. A password for the matrix substitution.
2. One or more passwords for the transposition.

## Encryption methods

### Matrix encoding (Polybios square)

The first encryption method is a replacement of one character by a combination of multiple characters.
This encryption method has two parameters:
One parameter is the number of places (denoted by `p`) that the substitution is made of, i.e. how many characters are used to encode one character.
The second parameter is the number of different characters (denoted by `n`).

Then the number of possible character combinations is `n`<sup>`p`</sup>.
One has to define a list of allowed characters with a length that must be at most the number of combinations.
This list of allowed characters is called an "alphabet".

This substitution needs a password which must only contain characters from the alphabet.

This password is filled into a `p`-dimensional matrix with only unique characters.
I.e. characters from the password are not repeated.

Here is an example:

Input for the substitution construction:

1. The alphabet is all upper-case letters without `J`: `ABCDEFHIKLMNOPQRSTUVWXYZ`.
2. The number of places is 2: `p=2`.
3. The number of different encoding characters is 5: `ADFGX`.
This yields `5`<sup>`2`</sup> = `25` character combinations which is exactly the length of the alphabet.
4. The password is `PIPERATTHEGATESOFDAWN`

Processing:

1. All repeating characters in the password are removed: `PIPERATTHEGATESOFDAWN` ==> `PIERATHGSOFDWN`

2. A matrix is constructed from the encoding characters and the processed password filled in starting at the first cell in the upper left corner from left to right and top to bottom:

|   | **A** | **D** | **F** | **G** | **X** |
| - | - | - | - | - | - |
| **A** | P | I | E | R | A |
| **D** | T | H | G | S | O |
| **F** | F | D | W | N |   |
| **G** |   |   |   |   |   |
| **X** |   |   |   |   |   |

3. Then the remaining characters of the alphabet are filled in, starting from the first unused letter after the last one from the processed password:

|   | **A** | **D** | **F** | **G** | **X** |
| - | - | - | - | - | - |
| **A** | P | I | E | R | A |
| **D** | T | H | G | S | O |
| **F** | F | D | W | N | Q |
| **G** | U | V | X | Y | Z |
| **X** | B | C | K | L | M |

This is the substitution matrix.
A letter is encoded by its row and column position.

I.e. the letter `E` is in row `A` and column `F`, so `E` is encoded as `AF`.
`U` is encoded as `GA`.
And so on.

Decryption works the other way around.
One takes a character combination, e.g. `FG` and reads it as "the character at row `F` and column `G`" which is `N`.

This encryption is easy to break by analyzing the frequencies of the 2-grams (two-letter combinations) which is the same as the frequencies of the letters in the clear text.

When there are more places the dimension of the substitution matrix changes accordingly.
I.e. when there are 3 places the matrix becomes a cube and when there are 4 places the matrix becomes a 4-dimensional cube, which is a bit hard to imagine ;-).

Nevertheless, the method of filling these `p`-dimensional matrices is always the one that is described above.

### Transposition

Another encryption method that is fundamentally different from the substitution of the matrix encryption is transposition.
The characters are not encoded but moved around.
They change their positions in the text.

The simplest transposition works like this:

1. One chooses a password.
2. It is used as the the header of a table, that has as many columns as the password has letters.
3. Then one assigns a number to each letter of the password which corresponds to the order of the letter in the word according to the alphabet.
4. The text is filled into the table from left to right and top to bottom.
5. Then the text is read out in **columns** in the order that is given by the number beneath each letter of the password.

Here is an example:

Let the password be `MEDDLE`.

This is written as the header of a table:

| **M** | **E** | **D** | **D** | **L** | **E** |
| - | - | - | - | - | - |
| |  |  |  |  |  |

Then the order of the letter according to the alphabet is written under each letter of the password:

| **M** | **E** | **D** | **D** | **L** | **E** |
| - | - | - | - | - | - |
| **6** | **3** | **1** | **2** | **5** | **4** |
| |  |  |  |  |  |

This means:
- The first `D` comes first in the alphabet.
- The second `D` comes after the first `D`.
- The first `E` is the third letter

and so on until the `M` which comes last when the letters of the password are ordered according to the alphabet.

Then the text is filled into the table.
We assume a very simple clear text: `ABCDEFGHIJKLMNOPQRSTUVWXYZ`.

The table then looks like this:

| **M** | **E** | **D** | **D** | **L** | **E** |
| - | - | - | - | - | - |
| **6** | **3** | **1** | **2** | **5** | **4** |
| A | B | C | D | E | F |
| G | H | I | J | K | L |
| M | N | O | P | Q | R |
| S | T | U | V | W | X |
| Y | Z |   |   |   |   |

Then the text is read out in column order and becomes: `CIOUDJPVBHNTZFLRXEKQWAGMSY`.

It`s is as easy as that!

However, this is easily cracked, as well.
Take, for example, the clear text letters `ABCDEF`.
They end up at positions 1, 5, 9, 14, 18, 23.
The differences between these positions are 4, 4, 5, 4, 5.
This regular spacing makes it easy to reverse the transposition.

Transpositions can be chained.
I.e., one can take the text just produced and feed it into another transposition.
This will make it much harder to reverse the transposition.
However, this is only the case, when the lengths of the passwords do not have a common factor.
I.e. the lengths must be relatively prime.
If they have a common factor it will show up as regular spacings of clear text characters in the resulting text.

Decrypting works the other way around.
I.e. one fills the encrypted text column-wise into the table with the order of the columns assigned by their numbers.
Then one reads the decrypted text as the rows from top to bottom.

In this example two columns have the length 5 and 4 have length 4.
How does one know how long a specific column is?
As the total length of the encrypted text is known, it is also known how long the last row is.
So it is also known which columns are longer than the others.

### Combining matrix encryption and transposition

It is possible to construct a quite good encryption by combining matrix encryption and transposition.
The idea is to first use a matrix substitution and then split the character combinations up by transposing the letters.

Here is a combined example of the two examples above:

The cleartext is `CRYPTOGRAPHYBENDSYOURMIND`.

With the matrix substitution this clear text becomes
`XDAGGGAADADXDFAGAXAADDGGXAAFFGFDDGGGDXGAAGXXADFGFD`.

This is filled into the transposition table:

| **M** | **E** | **D** | **D** | **L** | **E** |
| - | - | - | - | - | - |
| **6** | **3** | **1** | **2** | **5** | **4** |
| X | D | A | G | G | G |
| A | A | D | A | D | X |
| D | F | A | G | A | X |
| A | A | D | D | G | G |
| X | A | A | F | F | G |
| F | D | D | G | G | G |
| D | X | G | A | A | G |
| X | X | A | D | F | G |
| F | D |   |   |   |   |


After the transposition this becomes:
`ADADADGAGAGDFGADDAFAADXXDGXXGGGGGGDAGFGAFXADAXFDXF`

If one uses just the substitution matrix without reversing the transposition this would translate to 
`IIIUUVNUTFIMSMYYYTXUQIADK`.

It has to be said that the encryption in this example is **not** secure.
There are 3 reasons for this:

1. The plain text is very short.
2. There is only one transposition.
3. The transposition password is short.

However, if a long plain text is encrypted and there are at least two transpositions and these transpositions use long random passwords with lengths 30 or more, than this encryption is indeed secure.

Then one has to solve the problem of somehow securely storing long random passwords...

## Usage

When the program is started the main windows appears.
It has three tabs named `Matrix Substitution`, `Transposition` and `Files`.

### Matrix substitution

The `Matrix Substitution` deals with the first part of the encryption, the substitution.
The window has three sections: `Character Classes`, `Matrix Characters` and `Password`.

#### Character Classes

This section has 6 entries that deal with the characters that are encrypted and how they are matrix encrypted.
An entry can be chosen by clicking its radio button.
An entry first states how many characters are used to encode a character, followed by an `x` and then the number of different characters.
I.e. the entry that starts with `3x6` means that each character is encoded by three characters and there are 6 different characters.

| **Places** | **#&nbsp;Characters** | **Combinations** | **Allowed characters** | **Remarks** |
| :--------: | :--------------: | ---------------: | :----------------- | :------ |
| 2 | 5 | 25 | ABCDEFGHIKLMNOPRSTUVWXYZ | Note that there is no J. Each J is replaced by an I. |
| 2 | 6 | 36 | ABCDEFGHIJKLMNOPRSTUVWXYZ0123456890 | ./. |
| 3 | 3 | 27 | ABCDEFGHIJKLMNOPRSTUVWXYZ  | All upper-case characters and a blank. |
| 3 | 4 | 64 | ABCDEFGHIJKLMNOPRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 . | This is the first character class with lower-case letters. |
| 4 | 3 | 81 |  {LF}.,;:-!?'"%&+#*/()ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 | ./. |
| 3 | 6 | 216 |  {LF}.,;:-!?'"§%&+#*/()|°^~<=>–—«»¡¿¥£¤€$¢ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ÀÁÂÃÄÅĄÆÇĆČÐÞĎÈÉÊËĘĚÌÍÎÏŁÑŃŇÒÓÔÕÖØŘŚŠŤÙÚÛÜŮÝŸŹŻŽàáâãäåąæçćčðþďèéêëęěìíîïłñńňòóôõöøřśšßťùúûüůýÿźżž | ./. |

When a character class with only upper-case letters is chosen, all characters read will be converted to upper-case.

#### Matrix Characters

Here the characters for the substitution are specified.
They can only be upper-case latin characters.
There is a button `Random Alphabet` that generates a random alphabet of the allowed length.
The characters can be edited.

#### Password

Here the password for the matrix substitution is specified.
It can be built with the characters of the chosen character class.

### Transposition

The `Transposition` tab has only two sections: `Password` and `Passwords`.

One can enter a paassword in the `Password` field.
Upon pressing the `Enter` key, the entered password is converted to lower-case and copied into the `Passwords` section.
The passwords can be reordered by clicking on a password and dragging it to a new position.
Each password in the `Passwords` section can be deleted by clicking the `X` behind it.

Since it is essential that the password lengths do not have a common divisor, a password is rejected that has a length that has a common divisor with a password that is already present.

### File

The `File` tab is the most complex one as the handling of text files is complex and complicated.
It features four sections entitled `Operation`, `Files`, `Encodings` and `Text Layout` and two buttons `Go` and `Exit`.

#### Operation

Here one can decide whether encryption or decryption should be performed.

#### Files

There is a button `Source File`.
When one clicks it, a file dialog appears where one can select the source file to be used for the operation chosen in the section above.

When a file is chosen its path appears in the `Source` subsection.
The name of the destination file appears below in the `Destination` subsection.

When encrypting, the destination file path is constructed by appending `_encrypted` to the base file name.
When decrypting, `_decrypted` is appended.

#### Encodings

This is the most complex setting.
Files are just bytes with no inherent meaning.
They become text files by assigning a meaning to byte sequences.
Characters are encoded by a specific byte sequence.
This is called an encoding.

There are two encoding fields.
One for the source file and one for the destination file.

Each is a combo box that shows all encodings that can be handled by the program.

Here are the most important encodings:

| Name           | Meaning                                                                       |
| :------------- | :---------------------------------------------------------------------------- |
| `IBM273`       | [IBM Code Page 273](https://en.wikipedia.org/wiki/EBCDIC) (EBCDIC)            |
| `IBM437`       | [IBM Code Page 437](https://en.wikipedia.org/wiki/Code_page_437)              |
| `IBM850`       | [IBM Code Page 850](https://en.wikipedia.org/wiki/Code_page_850)              |
| `IBM852`       | [IBM Code Page 852](https://en.wikipedia.org/wiki/Code_page_852)              |
| `ISO-8859-1`   | [ISO 8859-1](https://en.wikipedia.org/wiki/ISO/IEC_8859-1)                    |
| `ISO-8859-15`  | [ISO 8859-15](https://en.wikipedia.org/wiki/ISO/IEC_8859-15)                  |
| `UTF-16BE`     | [UTF-16BE](https://en.wikipedia.org/wiki/UTF-16) (`BE` means "big-endian")    |
| `UTF-16`       | [UTF-16LE](https://en.wikipedia.org/wiki/UTF-16) (`LE` means "little-endian") |
| `UTF-8`        | [UTF-8](https://en.wikipedia.org/wiki/UTF-8)                                  |
| `WINDOWS-1250` | [Windows 1250](https://en.wikipedia.org/wiki/Windows-1250)                    |
| `WINDOWS-1252` | [Windows 1252](https://en.wikipedia.org/wiki/Windows-1252)                    |

As can be seen there are quite a lot of different ways to encode characters in bytes.
Since files are a sequence of bytes and there is no way to read the encoding of a file, it has to be specified by the user.

The default is `UTF-8` as it is the most commonly used encoding.

And - in fact - there are five common encodings that __may__ include an indication of the used encoding by means of a "[Byte Order Mark (BOM)](https://en.wikipedia.org/wiki/Byte_order_mark)".
These encodings are UTF-8, UTF-16BE, UTF-16LE, UTF-32BE and UTF32-LE.
They are the only encodings that can be recognized automatically.
A byte order mark may be present.
It does not have to be present for these encodings.

If the program encounters a byte order mark at the beginning of a file, it uses this encoding and discards the encoding that is specified by the user.

Below the two combo boxes is a check box that reads "Destination encoding same as source encoding".
If this is checked, the destination encoding will be the same as the source encoding.
This is default.

Below this is another combo box that is labeled "Destination BOM".
It has the following values:

| Setting        | Meaning                                                                      |
| :------------- | :--------------------------------------------------------------------------- |
| Same as source | The destination file has a BOM if - and only if - the source file has a BOM. |
| No BOM         | The destination has no BOM, regardless of a BOM presence in the source file. |
| With BOM       | The destination has a BOM, regardless of a BOM presence in the source file.  |

This combo box is only enabled if the destination encoding supports a BOM.

As if this would be not complicated enough, there is a further complication: Line endings.

Historically there have been many different ways to indicate the end of text line:

| Line Ending                       | Byte value(s) | Meaning                                  |
| :-------------------------------- | :----------- | :---------------------------------------- |
| ASCII Line Feed                   | `0A`         | Line ending of Unix and Linux text files. |
| ASCII Carriage Return / Line Feed | `0D 0A`      | Line ending of Windows text files.        |
| ASCII Carriage Return             | `0D`         | Line ending of old Macintosh text files.  |
| EBCDIC New Line                   | `15`         | Line ending of IBM EBCDIC files.          |

So, there is another combo box at the end of this section to specify the line ending of the destination file.
The default is "Windows (CR/LF)".
If an EBCDIC encoding is chosen as the destination encoding, "EBCDIC (NL)" is chosen as the default.

The line endings can only be specified for the destination file.
The program recognizes all possible line endings and converts them into a standardized internal form before further processing.

#### Text Layout

When encrypting a file, the output is just a long stream of characters.

In order to structure these characters, it is possible to specify a group size and a maximum line length.
If any of these is different from `0`, the text is structured accordingly.
I.e. if the group size is larger than 0, the encrypted text will be grouped.
If the maximum line length is set, there will be line breaks after this length.

If both group size and maximum line length are set, the effective maximum line length is the largest multiple of "group size + 1" that is less than or equal to the specified maximum line length.
This is done, so that groups are not split between lines.

The "Text Layout" section is only enabled, if encryption is chosen.
It has no effect on decryption.

#### Buttons

Clicking the green `GO` button starts the selected operation.

Clicking the red `Exit` button exits the program.

## Contributing

Feel free to submit a pull request with new features, improvements on tests or documentation and bug fixes.

## Contact

Frank Schwab ([Mail](mailto:github.sfdhi@slmails.com "Mail"))

## License

MatrixTranspose is released under the Apache V2 license. See "LICENSE.txt" for details.
