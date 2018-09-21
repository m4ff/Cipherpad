# Cipherpad
Cipherpad is a text editor that automatically decrypts and encrypts text (using the provided passphrase) when opening and svaing a file.

## Development
Cipherpad was developed using WPF and C#. The current version is quite stable and has basic plain text editing functionalities.

## Encryption
The encryption passphrase goes through the PBKDF2 key derivation function with a 16 byte salt and 128000 iterations to generate a 256 bit key. The derived key is then used to encrypt the text using AES in CBC mode with a randomly generated IV that is stored with the file.
