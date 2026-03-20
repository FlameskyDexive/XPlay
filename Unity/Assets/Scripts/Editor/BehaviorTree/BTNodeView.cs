using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ET
{
    public sealed class BTNodeView : Node
    {
        private const string SetCombatStateTypeId = "combat.action.set_state";
        private const string CheckStateChangeResultTypeId = "combat.condition.check_state_change_result";
        private const string CombatStateChangeResultKey = "Combat.StateChangeResult";

        private readonly Label summaryLabel;
        private readonly Label debugBadgeLabel;
        private readonly Action onChanged;
        private readonly Action<BTNodeView> onSelected;
        private readonly Action<BTNodeView> onDoubleClicked;
        private readonly VisualElement topPortContainer;
        private readonly VisualElement bottomPortContainer;

        public BTNodeView(BTEditorNodeData data, Action<BTNodeView> onSelected, Action onChanged,
            Action<BTNodeView> onDoubleClicked = null)
        {
            this.Data = data;
            this.onSelected = onSelected;
            this.onChanged = onChanged;
            this.onDoubleClicked = onDoubleClicked;
            this.viewDataKey = data.NodeId;

            this.style.overflow = Overflow.Visible;

            this.inputContainer.style.display = DisplayStyle.None;
            this.outputContainer.style.display = DisplayStyle.None;

            this.topPortContainer = CreatePortContainer(true);
            this.bottomPortContainer = CreatePortContainer(false);
            this.Add(this.topPortContainer);
            this.Add(this.bottomPortContainer);

            if (BTEditorUtility.HasInputPort(data.NodeKind))
            {
                this.InputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                this.InputPort.portName = string.Empty;
                ConfigurePort(this.InputPort);
                this.topPortContainer.Add(this.InputPort);
            }

            if (BTEditorUtility.HasOutputPort(data.NodeKind))
            {
                this.OutputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, BTEditorUtility.GetOutputCapacity(data.NodeKind), typeof(bool));
                this.OutputPort.portName = string.Empty;
                ConfigurePort(this.OutputPort);
                this.bottomPortContainer.Add(this.OutputPort);
            }

            this.summaryLabel = new Label();
            this.summaryLabel.style.whiteSpace = WhiteSpace.Normal;
            this.summaryLabel.style.unityTextAlign = TextAnchor.UpperLeft;
            this.extensionContainer.Add(this.summaryLabel);

            this.debugBadgeLabel = new Label();
            this.debugBadgeLabel.style.display = DisplayStyle.None;
            this.debugBadgeLabel.style.marginLeft = StyleKeyword.Auto;
            this.debugBadgeLabel.style.marginRight = 4;
            this.debugBadgeLabel.style.marginTop = 2;
            this.debugBadgeLabel.style.marginBottom = 2;
            this.debugBadgeLabel.style.paddingLeft = 6;
            this.debugBadgeLabel.style.paddingRight = 6;
            this.debugBadgeLabel.style.paddingTop = 1;
            this.debugBadgeLabel.style.paddingBottom = 1;
            this.debugBadgeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.debugBadgeLabel.style.fontSize = 10;
            this.debugBadgeLabel.style.color = Color.white;
            this.debugBadgeLabel.style.borderTopLeftRadius = 8;
            this.debugBadgeLabel.style.borderTopRightRadius = 8;
            this.debugBadgeLabel.style.borderBottomLeftRadius = 8;
            this.debugBadgeLabel.style.borderBottomRightRadius = 8;
            this.titleContainer.Add(this.debugBadgeLabel);

            this.mainContainer.style.marginTop = 6;
            this.mainContainer.style.marginBottom = 6;

            if (!BTEditorUtility.CanDelete(data))
            {
                this.capabilities &= ~Capabilities.Deletable;
            }

            this.RefreshView(BTNodeState.Inactive, null);
            this.SetPosition(data.Position);
            this.RegisterCallback<MouseDownEvent>(this.OnMouseDownEvent, TrickleDown.TrickleDown);
        }

        public BTEditorNodeData Data { get; }

        public Port InputPort { get; }

        public Port OutputPort { get; }

        public Vector2 GetInputAnchorWorldPosition()
        {
            Rect worldRect = this.worldBound;
            return new Vector2(worldRect.center.x, worldRect.yMin);
        }

        public Vector2 GetInputAnchorContentPosition()
        {
            Rect rect = this.GetPosition();
            return new Vector2(rect.center.x, rect.yMin);
        }

        public Vector2 GetOutputAnchorWorldPosition()
        {
            Rect worldRect = this.worldBound;
            return new Vector2(worldRect.center.x, worldRect.yMax);
        }

        public Vector2 GetOutputAnchorContentPosition()
        {
            Rect rect = this.GetPosition();
            return new Vector2(rect.center.x, rect.yMax);
        }

        public Rect GetNodeWorldRect()
        {
            return this.worldBound;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            this.Data.Position = newPos;
            this.onChanged?.Invoke();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            this.onSelected?.Invoke(this);
        }

        private void OnMouseDownEvent(MouseDownEvent evt)
        {
            if (evt.button != 0 || evt.clickCount != 2)
            {
                return;
            }

            this.onDoubleClicked?.Invoke(this);
            evt.StopPropagation();
        }

        public void RefreshView(BTNodeState debugState, BTDebugSnapshot snapshot)
        {
            this.title = BTEditorUtility.GetNodeTitle(this.Data);
            this.summaryLabel.text = BTEditorUtility.GetNodeSummary(this.Data);
            this.titleContainer.style.backgroundColor = BTEditorUtility.GetNodeHeaderColor(this.Data.NodeKind, debugState);
            this.RefreshDebugBadge(snapshot);
            this.RefreshExpandedState();
            this.RefreshPorts();
        }

        private void RefreshDebugBadge(BTDebugSnapshot snapshot)
        {
            if (snapshot?.BlackboardValues == null)
            {
                this.HideDebugBadge();
                return;
            }

            if (!snapshot.BlackboardValues.TryGetValue(CombatStateChangeResultKey, out string rawValue))
            {
                this.HideDebugBadge();
                return;
            }

            int? result = ParseStateChangeResult(rawValue);
            if (string.Equals(this.Data.NodeTypeId, SetCombatStateTypeId, StringComparison.OrdinalIgnoreCase))
            {
                this.ShowDebugBadge($"Result {GetStateChangeResultName(result, rawValue)}", GetStateChangeResultColor(result));
                return;
            }

            if (string.Equals(this.Data.NodeTypeId, CheckStateChangeResultTypeId, StringComparison.OrdinalIgnoreCase))
            {
                int? expected = this.Data.Arguments?.Find(argument => string.Equals(argument.Name, "result", StringComparison.OrdinalIgnoreCase))?.Value?.IntValue;
                bool matched = result.HasValue && expected.HasValue && result.Value == expected.Value;
                string badgeText = $"Cur {GetStateChangeResultName(result, rawValue)}";
                this.ShowDebugBadge(badgeText, matched ? new Color(0.15f, 0.55f, 0.2f) : new Color(0.45f, 0.45f, 0.45f));
                this.debugBadgeLabel.tooltip = expected.HasValue
                    ? $"Expected: {GetStateChangeResultName(expected, expected.Value.ToString())} ({expected.Value})\nCurrent: {GetStateChangeResultName(result, rawValue)}"
                    : this.debugBadgeLabel.tooltip;
                return;
            }

            this.HideDebugBadge();
        }

        private void ShowDebugBadge(string text, Color backgroundColor)
        {
            this.debugBadgeLabel.text = text;
            this.debugBadgeLabel.tooltip = text;
            this.debugBadgeLabel.style.backgroundColor = backgroundColor;
            this.debugBadgeLabel.style.display = DisplayStyle.Flex;
        }

        private void HideDebugBadge()
        {
            this.debugBadgeLabel.text = string.Empty;
            this.debugBadgeLabel.tooltip = string.Empty;
            this.debugBadgeLabel.style.display = DisplayStyle.None;
        }

        private static int? ParseStateChangeResult(string rawValue)
        {
            return int.TryParse(rawValue, out int intValue) ? intValue : null;
        }

        private static string GetStateChangeResultName(int? value, string fallback)
        {
            if (!value.HasValue)
            {
                return fallback ?? "Unknown";
            }

            return value.Value switch
            {
                0 => "Success",
                1 => "InvalidState",
                2 => "Dead",
                3 => "BlockedByTag",
                4 => "SkillNotFound",
                5 => "NoTarget",
                6 => "InCd",
                7 => "Controlled",
                8 => "InsufficientMp",
                9 => "OutOfRange",
                _ => fallback ?? "Unknown",
            };
        }

        private static Color GetStateChangeResultColor(int? value)
        {
            return value switch
            {
                0 => new Color(0.15f, 0.55f, 0.2f),
                5 => new Color(0.8f, 0.45f, 0.1f),
                6 => new Color(0.55f, 0.35f, 0.1f),
                7 => new Color(0.6f, 0.2f, 0.2f),
                8 => new Color(0.45f, 0.2f, 0.55f),
                9 => new Color(0.8f, 0.45f, 0.1f),
                _ => new Color(0.45f, 0.2f, 0.2f),
            };
        }

        private static VisualElement CreatePortContainer(bool isTop)
        {
            VisualElement container = new();
            container.style.position = Position.Absolute;
            container.style.left = 0;
            container.style.right = 0;
            container.style.height = 18;
            container.style.justifyContent = Justify.Center;
            container.style.alignItems = Align.Center;
            container.style.flexDirection = FlexDirection.Row;
            container.style.overflow = Overflow.Visible;
            if (isTop)
            {
                container.style.top = -9;
            }
            else
            {
                container.style.bottom = -9;
            }

            return container;
        }

        private static void ConfigurePort(Port port)
        {
            port.style.alignSelf = Align.Center;
            port.style.justifyContent = Justify.Center;
            port.style.alignItems = Align.Center;
            port.style.width = 16;
            port.style.minWidth = 16;
            port.style.maxWidth = 16;
            port.style.height = 16;
            port.style.minHeight = 16;
            port.style.maxHeight = 16;
            port.style.marginLeft = 0;
            port.style.marginRight = 0;
            port.style.marginTop = 0;
            port.style.marginBottom = 0;
            port.style.paddingLeft = 0;
            port.style.paddingRight = 0;
            port.style.paddingTop = 0;
            port.style.paddingBottom = 0;
            port.style.position = Position.Relative;
            port.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            port.style.borderBottomWidth = 0;
            port.style.borderTopWidth = 0;
            port.style.borderLeftWidth = 0;
            port.style.borderRightWidth = 0;

            foreach (VisualElement child in port.Children())
            {
                if (child is Label)
                {
                    child.style.display = DisplayStyle.None;
                    continue;
                }

                if (child.ClassListContains("connector"))
                {
                    child.style.width = 10;
                    child.style.height = 10;
                    child.style.minWidth = 10;
                    child.style.minHeight = 10;
                    child.style.maxWidth = 10;
                    child.style.maxHeight = 10;
                    child.style.marginLeft = 0;
                    child.style.marginRight = 0;
                    child.style.marginTop = 0;
                    child.style.marginBottom = 0;
                }
            }
        }
    }
}
