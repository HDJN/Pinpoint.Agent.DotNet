namespace Pinpoint.Agent.Common
{
    using System;
    using System.Text;

    public sealed class ApiUtils
    {
        private const String EMPTY_ARRAY = "()";

        private ApiUtils()
        {
        }

        public static String mergeParameterVariableNameDescription(String[] parameterType, String[] variableName)
        {
            if (parameterType == null && variableName == null)
            {
                return EMPTY_ARRAY;
            }
            if (variableName != null && parameterType != null)
            {
                if (parameterType.Length != variableName.Length)
                {
                    throw new InvalidOperationException("args size not equal");
                }
                if (parameterType.Length == 0)
                {
                    return EMPTY_ARRAY;
                }
                StringBuilder sb = new StringBuilder(64);
                sb.Append('(');
                int end = parameterType.Length - 1;
                for (int i = 0; i < parameterType.Length; i++)
                {
                    sb.Append(parameterType[i]);
                    sb.Append(' ');
                    sb.Append(variableName[i]);
                    if (i < end)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(')');
                return sb.ToString();
            }
            throw new InvalidOperationException("invalid null pair parameterType:" + String.Join(",", parameterType) + ", variableName:" + String.Join(",", variableName));
        }

        public static String mergeApiDescriptor(String className, String methodName, String parameterDescriptor)
        {
            StringBuilder buffer = new StringBuilder(256);
            buffer.Append(className);
            buffer.Append('.');
            buffer.Append(methodName);
            buffer.Append(parameterDescriptor);
            return buffer.ToString();
        }
    }
}
