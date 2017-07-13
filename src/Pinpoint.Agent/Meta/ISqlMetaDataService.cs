namespace Pinpoint.Agent.Meta
{
    public interface ISqlMetaDataService
    {
        DefaultParsingResult ParseSql(string sql);

        int CacheSql(DefaultParsingResult parsingResult);
    }
}
