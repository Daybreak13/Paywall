using MoreMountains.Tools;
using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Allows character to attract pickable items within a certain radius
    /// </summary>
    public class CharacterItemMagnet : CharacterAbilityIRE, MMEventListener<PaywallModuleEvent>
    {
        /// The collider that indicates magnet radius
        [field: Tooltip("The collider that indicates magnet radius")]
        [field: SerializeField] public CircleCollider2D MagnetCollider { get; protected set; }
        /// Default unupgraded magnet radius
        [field: Tooltip("Default unupgraded magnet radius")]
        [field: SerializeField] public float DefaultRadius { get; protected set; } = 0.7f;
        /// Radius with module active
        [field: Tooltip("Radius with module active")]
        [field: SerializeField] public float UpgradedRadius { get; protected set; } = 2f;
        /// Radius with enhanced module active
        [field: Tooltip("Radius with enhanced module active")]
        [field: SerializeField] public float EnhancedRadius { get; protected set; } = 4f;
        /// SO module that affects magnet ability
        [field: Tooltip("SO module that affects magnet ability")]
        [field: SerializeField] public ScriptableModule MagnetModule { get; protected set; }

        protected override void Awake()
        {
            base.Awake();
            if (MagnetCollider == null)
            {
                MagnetCollider = GetComponent<CircleCollider2D>();
            }
            if (!AbilityPermitted)
            {
                MagnetCollider.enabled = false;
            }
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();
            if (Character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Dodging)
            {
                MagnetCollider.enabled = false;
            }
            else
            {
                MagnetCollider.enabled = true;
            }
        }

        public virtual void OnMMEvent(PaywallModuleEvent moduleEvent)
        {
            if (moduleEvent.Module.Name.Equals(MagnetModule.Name))
            {
                if (moduleEvent.Module.IsActive)
                {
                    MagnetCollider.enabled = true;
                    MagnetCollider.radius = UpgradedRadius;
                }
                else
                {
                    MagnetCollider.radius = DefaultRadius;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening<PaywallModuleEvent>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening<PaywallModuleEvent>();
        }
    }
}
