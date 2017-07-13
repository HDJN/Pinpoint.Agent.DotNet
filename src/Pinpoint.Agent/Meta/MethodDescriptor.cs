namespace Pinpoint.Agent.Meta
{
    public abstract class MethodDescriptor
    {
        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public string[] ParameterTypes { get; set; }

        public string[] ParameterVariableName { get; set; }

        public string ParameterDescriptor { get; set; }

        public string ApiDescriptor { get; set; }

        public int LineNumber { get; set; }

        public int ApiId { get; set; }

        public int Type { get; set; }

        protected string fullName;

        public abstract string GetFullName();
    }
}
