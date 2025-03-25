using System.Security.Cryptography;

namespace Aslanta.Idgen.Job;

public static class ShortIdGenerator
{
    private static readonly char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private const int IdLength = 6;

    public static string GenerateId()
    {
        char[] id = new char[IdLength];
        for (int i = 0; i < IdLength; i++)
        {
            int index = RandomNumberGenerator.GetInt32(0, chars.Length);
            id[i] = chars[index];
        }
        return new string(id);
    }
}
