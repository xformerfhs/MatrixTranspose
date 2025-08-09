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
One takes a character combination, e.g. `FG` and reads it as "the character at row `F` and column `G` which is `N`.

This encryption is easily to break by analyzing the frequencies of the 2-grams (two-letter combinations) which is the same as the frequencies of the letters in the clear text.

## Contributing

Feel free to submit a pull request with new features, improvements on tests or documentation and bug fixes.

## Contact

Frank Schwab ([Mail](mailto:github.sfdhi@slmails.com "Mail"))

## License

MatrixTranspose is released under the Apache V2 license. See "LICENSE.txt" for details.
