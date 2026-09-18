using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace _Project.Scripts.Gameplay.Features.TweenFeature.Editor
{
    public class TweenInspectorProcessor : OdinAttributeProcessor<PrimeTween.Tween>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            if (member.Name == "isAlive")
            {
                attributes.Add(new ShowInInspectorAttribute());
                attributes.Add(new ReadOnlyAttribute());
                attributes.Add(new LabelTextAttribute("Is Alive"));
                attributes.Add(new BoxGroupAttribute("Tween State"));
                attributes.Add(new GUIColorAttribute(0.4f, 0.8f, 0.5f));
            }
        }
    }
}