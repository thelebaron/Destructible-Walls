using System;
using System.Security.Cryptography;
using System.Text;
using Hash128 = Unity.Entities.Hash128;
using Object = UnityEngine.Object;

namespace Junk.Entities
{
    public static class HashingUtility
    {
        public static unsafe Hash128 GenerateGuid(Object obj)
        {
            Guid guid;
            var  input = obj.name + obj.GetType();
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytehash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                guid = new Guid(bytehash);
            }
        
            var hash = new Hash128();
            hash = *(Hash128*)&guid;
            return hash;
        }
    }
}