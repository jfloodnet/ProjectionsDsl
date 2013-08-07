using System;
using System.Collections.Generic;
using ProjectionsDsl.Core;

namespace ProjectionsDsl.Tests.Fakes
{
    public class FakeDocumentWriter : IDocumentWriter<Guid, ListOfProjectedState>
    {
        public Dictionary<Guid, ListOfProjectedState> DB = new Dictionary<Guid, ListOfProjectedState>();
        public ListOfProjectedState AddOrUpdate(Guid key, Func<ListOfProjectedState> addFactory, Func<ListOfProjectedState, ListOfProjectedState> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
        {
            ListOfProjectedState document;
            if (DB.ContainsKey(key))
            {
                document = DB[key];
                update(document);
            }
            else
            {
                document = addFactory();
                DB.Add(key, document);
            }
            return document;
        }

        public bool TryDelete(Guid key)
        {
            DB.Remove(key);
            return true;
        }
    }
}