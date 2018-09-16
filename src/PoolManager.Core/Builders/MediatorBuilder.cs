﻿using PoolManager.Core.Mediators;
using PoolManager.Core.Resolvers;

namespace PoolManager.Core.Builders
{
    public class MediatorBuilder : IMediatorBuilder
    {
        private readonly DependencyResolver _resolver;

        public MediatorBuilder(DependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public Mediator Build() => new Mediator(_resolver);
    }
}
