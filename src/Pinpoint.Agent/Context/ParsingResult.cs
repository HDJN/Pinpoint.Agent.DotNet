namespace Pinpoint.Profiler.Context
{
    using System;

    public interface ParsingResult
    {
        string Sql { get; set; }

        String GetOutput();

        int GetId();
    }
}
