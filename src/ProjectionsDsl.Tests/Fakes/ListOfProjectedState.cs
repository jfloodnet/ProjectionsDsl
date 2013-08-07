using System.Collections.Generic;

namespace ProjectionsDsl.Tests.Fakes
{
    public class ListOfProjectedState
    {
        public List<TestProjectionState> Events = new List<TestProjectionState>();

        public ListOfProjectedState AddState(TestProjectionState state)
        {
            Events.Add(state);
            return this;
        }
    }
}