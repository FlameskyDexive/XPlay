namespace ET
{
    public interface IBTCompiledTreeProvider
    {
        string PackageKey { get; }

        BTCompiledTreeTemplate Template { get; }
    }
}
