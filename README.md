# MatrixTranspose

This is a dummy text for the real documentation which I will add a later time.

This is a small program that features a combined [Polybios (matrix) substitution](https://en.wikipedia.org/wiki/Polybius_square) and [transposition](https://en.wikipedia.org/wiki/Transposition_cipher) cipher.

First each letter of the clear text is substituted by 2 or more letters (i.e. [fractionation](https://en.wikipedia.org/wiki/Transposition_cipher#Fractionation)) and then the letters of this substitution cipher are transposed a number of times.

This has the effect that the character substitutions are widely separated from each other, making it very hard to crack the cipher.
I.e. it is hard if the clear text is long and there are at least two transpositions with very long random passwords.

A failed cipher of this kind was the German [ADFG(V)X cipher](https://en.wikipedia.org/wiki/ADFGVX_cipher) used during World War I.
It failed because the matrix substitution is easily cracked and there was only one transposition with short passwords.

The cipher needs two categories of passwords:

1. A password for the matrix substitution.
2. One or more passwords for the transposition.

## Encryption methods

### Matrix encoding (Polybios square)

The first encryption method is a replacement of one character by a combination of multiple characters.
One parameter is the number of places (denoted by `p`) that the substitution is made of, i.e. how many characters a substitution has.
The second parameter is the number of different characters (denoted by `n`).

The number of possible combinations is `n`<sup>`p`</sup>.
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

Decryption works the other way around.
One takes a character combination, e.g. `FG` and reads it as "the character at row `F` and column `G`" which is `N`.

This encryption is easily to break by analyzing the frequencies of the 2-grams (two-letter combinations) which is the same as the frequencies of the letters in the clear text.

When there are more places the dimension of the substitution matrix changes accordingly.
I.e. when there are 3 places the matrix becomes a cube and when there are 4 places the matrix becomes a 4-dimensional cube, which is a bit hard to imagine ;-).

Nevertheless, the method of filling these `p`-dimensional matrices is always the one that is described above.

### Transposition

Another encryption method that is fundamentally different from the substitution of the matrix encryption is transposition.
The characters are not encoded but move around.
They change there position in the text.

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
| 6 | 3 | 1 | 2 | 5 | 4 |
| - | - | - | - | - | - |
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
| 6 | 3 | 1 | 2 | 5 | 4 |
| - | - | - | - | - | - |
| A | B | C | D | E | F |
| G | H | I | J | K | L |
| M | N | O | P | Q | R |
| S | T | U | V | W | X |
| Y | Z |   |   |   |   |

Then the text is read out in column order and becomes: `CIOUDJPVBHNTZFLRXEKQWAGMSY`.

It`s is as easy as that!

However, this is easily cracked, as well.
Take, for example, the clear text letters `ABCDEF`.
The end up at positions 1, 5, 9, 14, 18, 23`.
The differences between these positions are 4, 4, 5, 4, 5.
This regular spacing makes it easy to reverse the transposition.

Transpositions can be chained.
I.e., one can take the text just produced and feed it into another transposition.
This will make it much harder to reverse the transposition.
However, this is only the case, when the lengths of the passwords do not have a common factor.
I.e. the lengths must be relatively prime.
If they have a common factor it will show up as regular spacings of clear text characters in the resulting text.

### Combining matrix encryption and transposition

It is possible to construct a quite good encryption by combining matrix encryption and transposition.
The idea is to first use a matrix substitution and then split the character combinations up by transposing the letters.

Here is a combined example of the two examples above:

The cleartext is `CRYPTOGRAPHYBENDSYOURMIND`.
Note that `J` has been replaced by `I`, as the alphabet for the matrix substitution does not contain the letter `J`.

With the matrix substitution this clear text becomes
`XDAGGGAADADXDFAGAXAADDGGXAAFFGFDDGGGDXGAAGXXADFGFD`.

This is filled into the transposition table:

| **M** | **E** | **D** | **D** | **L** | **E** |
| 6 | 3 | 1 | 2 | 5 | 4 |
| - | - | - | - | - | - |
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

## Contributing

Feel free to submit a pull request with new features, improvements on tests or documentation and bug fixes.

## Contact

Frank Schwab ([Mail](mailto:github.sfdhi@slmails.com "Mail"))

## License

MatrixTranspose is released under the Apache V2 license. See "LICENSE.txt" for details.
