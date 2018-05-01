// <copyright file="EventMap.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CR.AggregateRepository.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Matches a type to an action.
    /// </summary>
    public class EventMap : Dictionary<Type, Action<object>>
    {
    }
}
