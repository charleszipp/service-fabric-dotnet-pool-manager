﻿using Ninject;
using Ninject.Modules;
using PoolManager.Core.Mediators;
using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Instances.Interfaces;
using PoolManager.Domains.Partitions.Interfaces;

namespace PoolManager.Domains.Partitions
{
    public static class NinjectKernelExtensions
    {
        public static IKernel WithPartitions<TPartitionsRepository, TPopVacantInstance, TOccupyInstance>(this IKernel kernel)
            where TPartitionsRepository : class, IPartitionRepository
            where TPopVacantInstance : class, IHandleCommand<Pools.Interfaces.PopVacantInstance, Pools.Interfaces.PopVacantInstanceResult>
            where TOccupyInstance : class, IHandleCommand<OccupyInstance, OccupyInstanceResult>
        {
            kernel.Load(new PartitionsDomainModule<TPartitionsRepository, TPopVacantInstance, TOccupyInstance>());
            return kernel;
        }
    }

    public class PartitionsDomainModule<TPartitionsRepository, TPopVacantInstance, TOccupyInstance> : NinjectModule
            where TPartitionsRepository : class, IPartitionRepository
            where TPopVacantInstance : class, IHandleCommand<Pools.Interfaces.PopVacantInstance, Pools.Interfaces.PopVacantInstanceResult>
            where TOccupyInstance : class, IHandleCommand<OccupyInstance, OccupyInstanceResult>
    {
        public override void Load()
        {
            Bind<IPartitionRepository>().To<TPartitionsRepository>();
            Kernel
                .WithCommandHandler<TPopVacantInstance, Pools.Interfaces.PopVacantInstance, Pools.Interfaces.PopVacantInstanceResult>()
                .WithCommandHandler<TOccupyInstance, OccupyInstance, OccupyInstanceResult>()
                .WithCommandHandler<GetInstanceHandler, GetInstance, GetInstanceResult>();
        }
    }
}