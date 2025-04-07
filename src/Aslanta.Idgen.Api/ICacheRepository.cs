namespace Aslanta.Idgen.Api;

public interface ICacheRepository
{
    Task<List<string>> GetIds(int count);
}
