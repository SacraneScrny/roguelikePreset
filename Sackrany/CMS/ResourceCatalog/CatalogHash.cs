using System.Security.Cryptography;
using System.Text;

namespace Sackrany.CMS.ResourceCatalog
{
    public static class CatalogHash
    {
        public static uint StableHash(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return System.BitConverter.ToUInt32(hashBytes, 0);
            }
        }
    }
}