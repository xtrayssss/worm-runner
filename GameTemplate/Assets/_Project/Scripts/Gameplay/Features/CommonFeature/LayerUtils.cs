using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature
{
    public static class LayerUtils
    {
        public static readonly int ENEMY_PROJECTILE_LAYER = LayerMask.NameToLayer("EnemyProjectile");
        public static readonly int MEMBER_PROJECTILE_LAYER = LayerMask.NameToLayer("MemberProjectile");
        public static readonly int CROWD_MEMBER_LAYER = LayerMask.NameToLayer("Member");
        public static readonly int INTERACTIVE_OBJECT_LAYER = LayerMask.NameToLayer("InteractiveObject");

        public static void IgnoreCollisionBetweenLayers(int layer1, int layer2, bool ignore)
        {
            Physics2D.IgnoreLayerCollision(layer1, layer2, ignore);
        }

        public static void SetLayer(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
        }

        public static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
        
        public static int CreateMask(params int[] layers) => 
            layers.Aggregate(0, static (current, t) => current | 1 << t);
    }
}