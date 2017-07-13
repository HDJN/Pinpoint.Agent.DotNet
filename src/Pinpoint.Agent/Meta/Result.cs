namespace Pinpoint.Agent.Meta
{
    public class Result
    {
        public bool NewValue { get; private set; }
        public int Id { get; private set; }

        public Result(bool newValue, int id)
        {
            this.NewValue = newValue;
            this.Id = id;
        }
    }
}
