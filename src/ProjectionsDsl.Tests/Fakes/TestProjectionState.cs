namespace ProjectionsDsl.Tests.Fakes
{
    public class TestProjectionState
    {
        public TestProjectionState(string someState)
        {
            SomeState = someState;
        }

        public string SomeState { get; set; }
    }
}