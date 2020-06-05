using DirectoryZync;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Assistant.Security {
	internal static class Crypto {
		private static readonly Logger Logger = new Logger(nameof(Crypto));

		public static string Encrypt(string data) {
			if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data)) {
				return string.Empty;
			}

			try {
				using (RijndaelManaged rijindael = new RijndaelManaged()) {
					rijindael.GenerateKey();
					rijindael.GenerateIV();
					return Convert.ToBase64String(EncryptStringToBytes(data, rijindael.Key, rijindael.IV));
				}
			}
			catch (Exception e) {
				Logger.Exception(e);
				return string.Empty;
			}
		}

		internal static string Decrypt(byte[] encryptedBytes) {
			if (encryptedBytes.Length <= 0) {
				return string.Empty;
			}

			try {
				using (RijndaelManaged myRijndael = new RijndaelManaged()) {
					string base64String = DecryptStringFromBytes(encryptedBytes, myRijndael.Key, myRijndael.IV);
					return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
				}
			}
			catch (Exception e) {
				Logger.Exception(e);
				return string.Empty;
			}
		}

		private static byte[] EncryptStringToBytes(string _plainText, byte[] _key, byte[] _iv) {
			if (_plainText == null || _plainText.Length <= 0) {
				throw new ArgumentNullException(nameof(_plainText));
			}

			if (_key == null || _key.Length <= 0) {
				throw new ArgumentNullException(nameof(_key));
			}

			if (_iv == null || _iv.Length <= 0) {
				throw new ArgumentNullException(nameof(_iv));
			}

			byte[] encrypted;
			using (RijndaelManaged rijAlg = new RijndaelManaged()) {
				rijAlg.Key = _key;
				rijAlg.IV = _iv;
				ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
				using (MemoryStream msEncrypt = new MemoryStream()) {
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
							swEncrypt.Write(_plainText);
						}
						encrypted = msEncrypt.ToArray();
					}
				}
			}
			return encrypted;
		}

		private static string DecryptStringFromBytes(byte[] _cipherText, byte[] _key, byte[] _iv) {
			if (_cipherText == null || _cipherText.Length <= 0) {
				throw new ArgumentNullException(nameof(_cipherText));
			}

			if (_key == null || _key.Length <= 0) {
				throw new ArgumentNullException(nameof(_key));
			}

			if (_iv == null || _iv.Length <= 0) {
				throw new ArgumentNullException(nameof(_iv));
			}

			string plaintext = string.Empty;
			using (RijndaelManaged rijAlg = new RijndaelManaged()) {
				rijAlg.Key = _key;
				rijAlg.IV = _iv;
				ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
				using (MemoryStream msDecrypt = new MemoryStream(_cipherText)) {
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
						using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}
			}
			return plaintext;
		}
	}
}
