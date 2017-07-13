namespace Pinpoint.Agent.Meta
{
    using Common;
    using System;
    using System.Text;

    public class DefaultMethodDescriptor : MethodDescriptor
    {
        public DefaultMethodDescriptor()
        {

        }

        public DefaultMethodDescriptor(String className, String methodName, String[] parameterTypes, String[] parameterVariableName)
        {
            this.ClassName = className;
            this.MethodName = methodName;
            this.ParameterTypes = parameterTypes;
            this.ParameterVariableName = parameterVariableName;
            this.ParameterDescriptor = ApiUtils.mergeParameterVariableNameDescription(parameterTypes, parameterVariableName);
            this.ApiDescriptor = ApiUtils.mergeApiDescriptor(className, methodName, ParameterDescriptor);
        }

        public override string GetFullName()
        {
            if (fullName != null)
            {
                return fullName;
            }
            StringBuilder buffer = new StringBuilder(256);
            buffer.Append(ClassName);
            buffer.Append(".");
            buffer.Append(MethodName);
            buffer.Append(ParameterDescriptor);
            if (LineNumber != -1)
            {
                buffer.Append(":");
                buffer.Append(LineNumber);
            }
            fullName = buffer.ToString();
            return fullName;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{className=");
            builder.Append(ClassName);
            builder.Append(", methodName=");
            builder.Append(MethodName);
            builder.Append(", parameterTypes=");
            builder.Append(String.Join(",", ParameterTypes));
            builder.Append(", parameterVariableName=");
            builder.Append(String.Join(",", ParameterVariableName));
            builder.Append(", parameterDescriptor=");
            builder.Append(ParameterDescriptor);
            builder.Append(", apiDescriptor=");
            builder.Append(ApiDescriptor);
            builder.Append(", lineNumber=");
            builder.Append(LineNumber);
            builder.Append(", apiId=");
            builder.Append(ApiId);
            builder.Append(", fullName=");
            builder.Append(fullName);
            builder.Append(", type=");
            builder.Append(Type);
            builder.Append("}");
            return builder.ToString();
        }
    }
}
