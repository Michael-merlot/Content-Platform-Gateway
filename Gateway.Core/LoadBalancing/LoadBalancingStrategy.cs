namespace Gateway.Core.LoadBalancing
{
    public enum LoadBalancingStrategy
    {
        RoundRobin,
        LeastConnections,
        IpHash
    }
}
