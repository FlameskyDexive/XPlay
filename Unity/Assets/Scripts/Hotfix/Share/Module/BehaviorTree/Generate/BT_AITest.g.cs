namespace ET
{
    [BTCompiledTreeProvider]
    public sealed class BTCompiledProvider_AITest : IBTCompiledTreeProvider
    {
        private const string PackageKeyValue = "AITest";

        private static BTCompiledTreeTemplate template;

        public string PackageKey => PackageKeyValue;

        public BTCompiledTreeTemplate Template => template ??= BTCompiledTreeTemplateBuilder.Build(PackageKeyValue, global::ET.Client.BTClientDemoFactory.CreateAITestPackage());
    }
}
