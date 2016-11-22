using System;
using System.Collections.Generic;

namespace CR.AggregateRepository.Core
{
    public class EventMap : Dictionary<Type, Action<object>>
    {

    }
}