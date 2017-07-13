namespace Pinpoint.Agent.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ConfigManager
    {
        public static Dictionary<string, string> Load(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                var keyVals = new Dictionary<string, string>();
                while (!reader.EndOfStream)
                {
                    var row = reader.ReadLine();
                    if (String.IsNullOrWhiteSpace(row) || row.StartsWith("#"))
                    {
                        continue;
                    }

                    var keyVal = row.Split(new char[] { '=' }, StringSplitOptions.None);
                    keyVals.Add(keyVal[0].Trim(), keyVal[1].Trim());
                }

                return keyVals;
            }
        }
    }
}
