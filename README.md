# MatrixTranspose

This is a dummy text for the real documentation which I will add a later time.

This is a small program that features a combined [Polybios (matrix) substitution](https://en.wikipedia.org/wiki/Polybius_square) and [transposition](https://en.wikipedia.org/wiki/Transposition_cipher) cipher.

First each letter of the clear text is substituted by 2 or more letter (i.e. [fractionation](https://en.wikipedia.org/wiki/Transposition_cipher#Fractionation)) and then the letters of this substitution cipher are transposed a number of times.

This has the effect that the character substitutions are widely separated from each other, making it very hard to crack the cipher.
I.e. it is hard if the clear text is long and there are at least two transpositions with very long random passwords.

A failed cipher of this kind was then German [ADFG(V)X cipher](https://en.wikipedia.org/wiki/ADFGVX_cipher) used in world war I.
It failed because the matrix substitution is easily cracked and there was only one transposition with short passwords.

The cipher needs two categories of passwords:

1. A password for the matrix substitution.
2. One or more passwords for the transposition.

## Contributing

Feel free to submit a pull request with new features, improvements on tests or documentation and bug fixes.

## Contact

Frank Schwab ([Mail](mailto:github.sfdhi@slmails.com "Mail"))

## License

MatrixTranspose is released under the Apache V2 license. See "LICENSE" for details.
