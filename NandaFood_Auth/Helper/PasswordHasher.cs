using System.Security.Cryptography;

namespace NandaFood_Auth.Helper;

public class PasswordHasher
{
    private const int SaltSize = 16; // 16 bytes for salt
    private const int HashSize = 20; // 20 bytes for PBKDF2-HMAC-SHA-1 hash
    private const int Iterations = 10000; // Number of PBKDF2 iterations

    public static string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

        // Hash the password and encode the salt
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        // Combine salt and hash
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        // Convert to base64
        string base64Hash = Convert.ToBase64String(hashBytes);

        return base64Hash;
    }
    
    public static bool VerifyPassword(string inputPassword, string hashedPassword)
    {
        // Convert hashed password from base64
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);

        // Extract salt
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // Compute hash on the input password using the same salt
        var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, Iterations);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        // Compare hashes
        for (int i = 0; i < HashSize; i++)
        {
            if (hashBytes[i + SaltSize] != hash[i])
            {
                return false;
            }
        }

        return true;
    }
}