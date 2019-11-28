// <copyright file="EventMap.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace CorshamScience.AggregateRepository.Core
{
    using System;
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>
    /// A dictionary of <see cref="Type"/>s to <see cref="Action"/>s which take a single <see cref="object"/> as an argument.
    /// </summary>
    /// <remarks>
    /// This is used to select the 'Apply' method which corresponds to each event provided to an <see cref="IAggregate"/>.
    /// </remarks>
    public class EventMap : Dictionary<Type, Action<object>>
    {
    }
}
