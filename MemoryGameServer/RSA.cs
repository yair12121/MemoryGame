using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MemoryGameServer
{
    public class RSA
    {
        private string _privateKey;
        private string _publicKey;
        private UnicodeEncoding _encoder;
        private RSACryptoServiceProvider _rsa;

        public RSA()
        {
            _encoder = new UnicodeEncoding();
            _rsa = new RSACryptoServiceProvider();
            //create new private key and make private key as the xml string that contain the key
            _privateKey = _rsa.ToXmlString(true);
            //create new public key and make private key as the xml string that contain the key
            _publicKey = _rsa.ToXmlString(false);
        }
        /// <summary>
        /// decript data by privateKey
        /// </summary>
        /// <param name="data">data to decript</param>
        /// <returns>decripted data</returns>

        //get encrypt string(the data) and return decrypt of the data by using the private key
        public string Decrypt(string data)
        {
            var dataArray = data.Split(new char[] { ',' });
            byte[] dataByte = new byte[dataArray.Length];
            for (int i = 0; i < dataArray.Length; i++)
            {
                dataByte[i] = Convert.ToByte(dataArray[i]);
            }

            _rsa.FromXmlString(_privateKey);
            var decryptedByte = _rsa.Decrypt(dataByte, false);
            return _encoder.GetString(decryptedByte);
        }
        /// <summary>
        /// Encrypt the data by public key
        /// </summary>
        /// <param name="data">data to encrypt</param>
        /// <param name="_publicKey"></param>
        /// <returns>encripted data</returns>

        //get data to encrypt and public key(of the other side that he send to) and return the data encrypt using the public key that he get
        public string Encrypt(string data, string _publicKey)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(_publicKey);
            var dataToEncrypt = _encoder.GetBytes(data);
            var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false);
            var length = encryptedByteArray.Length;
            var item = 0;
            var sb = new StringBuilder();
            foreach (var x in encryptedByteArray)
            {
                item++;
                sb.Append(x);

                if (item < length)
                    sb.Append(",");
            }

            return sb.ToString();
        }
        /// <summary>
        /// return PrivateKey
        /// </summary>
        /// <returns>PrivateKey</returns>
        public string GetPrivateKey()
        {
            return this._privateKey;
        }
        /// <summary>
        /// return PublicKey
        /// </summary>
        /// <returns>PublicKey</returns>
        public string GetPublicKey()
        {
            return this._publicKey;
        }
    }
}