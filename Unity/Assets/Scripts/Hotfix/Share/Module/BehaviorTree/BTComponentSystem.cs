namespace ET
{
    [EntitySystemOf(typeof(BTComponent))]
    [FriendOf(typeof(BTComponent))]
    public static partial class BTComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BTComponent self, byte[] treeBytes, string treeIdOrName)
        {
            self.TreeBytes = treeBytes;
            self.TreePackageKey = string.Empty;
            self.TreeIdOrName = treeIdOrName;
            self.StartTree();
        }

        [EntitySystem]
        private static void Awake(this BTComponent self, string treePackageKey, string treeIdOrName)
        {
            self.TreeBytes = null;
            self.TreePackageKey = treePackageKey;
            self.TreeIdOrName = treeIdOrName;
            self.StartTree();
        }

        [EntitySystem]
        private static void Destroy(this BTComponent self)
        {
            self.StopTree();
            self.TreeBytes = null;
            self.TreePackageKey = string.Empty;
            self.TreeIdOrName = null;
            self.BlackboardOverrides.Clear();
        }

        public static void Restart(this BTComponent self)
        {
            self.StopTree();
            self.StartTree();
        }

        public static void Reload(this BTComponent self, byte[] treeBytes, string treeIdOrName = "")
        {
            self.TreeBytes = treeBytes;
            self.TreePackageKey = string.Empty;
            self.TreeIdOrName = treeIdOrName;
            self.Restart();
        }

        public static void Reload(this BTComponent self, string treePackageKey, string treeIdOrName = "")
        {
            self.TreeBytes = null;
            self.TreePackageKey = treePackageKey;
            self.TreeIdOrName = treeIdOrName;
            self.Restart();
        }

        public static void SetBlackboardValue(this BTComponent self, string key, object value)
        {
            BTExecutionSession session = self.GetSession();
            session?.Blackboard.SetBoxed(key, value);
        }

        public static BTExecutionSession GetSession(this BTComponent self)
        {
            return BTExecutionSessionManager.Instance.Get(self.RuntimeId);
        }

        private static void StartTree(this BTComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            BTExecutionSession session = null;

            if (self.TreeBytes != null && self.TreeBytes.Length > 0)
            {
                session = BTRuntime.Create(unit, self.TreeBytes, self.TreeIdOrName);
            }
            else if (!string.IsNullOrWhiteSpace(self.TreePackageKey))
            {
                if (BTCompiledTreeRegistry.Instance.TryGetTemplate(self.TreePackageKey, out BTCompiledTreeTemplate template))
                {
                    session = BTRuntime.Create(unit, template, self.TreeIdOrName);
                }
                else
                {
                    byte[] bytes = BTLoader.Instance.LoadBytes(self.TreePackageKey, false);
                    if (bytes != null && bytes.Length > 0)
                    {
                        session = BTRuntime.Create(unit, bytes, self.TreeIdOrName);
                    }
                }
            }
            else
            {
                Log.Warning($"behavior tree source empty: {self.Id}");
                return;
            }

            if (session == null)
            {
                Log.Error($"behavior tree create failed: packageKey={self.TreePackageKey} tree={self.TreeIdOrName}");
                return;
            }

            foreach ((string key, BTSerializedValue value) in self.BlackboardOverrides)
            {
                session.Blackboard.SetBoxed(key, BTValueUtility.GetValue(value));
            }

            BTFlowDriver.RunRoot(session);
            self.RuntimeId = session.RuntimeId;
            BTExecutionSessionManager.Instance.Add(session);
        }

        private static void StopTree(this BTComponent self)
        {
            BTExecutionSession session = BTExecutionSessionManager.Instance.Remove(self.RuntimeId);
            self.RuntimeId = 0;
            BTFlowDriver.Dispose(session);
        }
    }
}
