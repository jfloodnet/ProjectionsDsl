using System;
using ProjectionsDsl.Core;

namespace ProjectionsDsl.Tests.Fakes
{
    public class TestEvent : IEvent
    {
        public TestEvent(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
        public string SomeState { get; set; }
    }
}