﻿using PoolManager.SDK.Pools.Requests;
using System.Threading.Tasks;
using PoolManager.SDK.Pools.Responses;

namespace PoolManager.SDK.Pools
{
    public interface IPoolProxy
    {
        Task StartPoolAsync(string serviceTypeUri, StartPoolRequest request);
        Task<ConfigurationResponse> GetConfigurationAsync(string serviceTypeUri);
    }
}