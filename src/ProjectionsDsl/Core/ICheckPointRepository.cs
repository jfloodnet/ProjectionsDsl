using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectionsDsl.Core
{
    public interface ICheckPointRepository
    {
        Position? GetLastPosition();
        void StorePosition(Position position);
    }
}
