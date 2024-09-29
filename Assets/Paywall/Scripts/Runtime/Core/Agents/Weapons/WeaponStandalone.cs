using MoreMountains.Tools;
using System;
using UnityEngine;
using static MoreMountains.CorgiEngine.Weapon;

namespace Paywall
{

    public class WeaponStandalone : MonoBehaviour
    {
        [field: Header("General")]

        /// is this weapon on semi or full auto ?
		[Tooltip("is this weapon on semi or full auto ?")]
        [field: SerializeField] public TriggerModes TriggerMode { get; protected set; } = TriggerModes.Auto;

        [field: Header("Delays")]

        /// the delay before use, that will be applied for every shot
        [field: Tooltip("the delay before use, that will be applied for every shot")]
        [field: SerializeField] public float DelayBeforeUse { get; protected set; } = 0f;
        /// whether or not the delay before used can be interrupted by releasing the shoot button (if true, releasing the button will cancel the delayed shot)
        [field: Tooltip("whether or not the delay before used can be interrupted by releasing the shoot button (if true, releasing the button will cancel the delayed shot)")]
        [field: SerializeField] public bool DelayBeforeUseReleaseInterruption { get; protected set; } = true;
        /// the time (in seconds) between two shots		
        [field: Tooltip("the time (in seconds) between two shots")]
        [field: SerializeField] public float TimeBetweenUses { get; protected set; } = 1f;
        /// whether or not the time between uses can be interrupted by releasing the shoot button (if true, releasing the button will cancel the time between uses)
        [field: Tooltip("whether or not the time between uses can be interrupted by releasing the shoot button (if true, releasing the button will cancel the time between uses)")]
        [field: SerializeField] public bool TimeBetweenUsesReleaseInterruption { get; protected set; } = true;
        /// a duration, in seconds, at the end of the weapon's life cycle and before going back to Idle
        [field: Tooltip("a duration, in seconds, at the end of the weapon's life cycle and before going back to Idle")]
        [field: SerializeField] public float CooldownDuration { get; protected set; } = 0f;

        [field: Header("Magazine")]

        /// whether or not the weapon is magazine based. If it's not, it'll just take its ammo inside a global pool
        [field: Tooltip("whether or not the weapon is magazine based. If it's not, it'll just take its ammo inside a global pool")]
        [field: SerializeField] public bool MagazineBased { get; protected set; } = true;
        /// the size of the magazine
        [MMCondition("MagazineBased", true)]
        [field: Tooltip("the size of the magazine")]
        public int MagazineSize { get; protected set; } = 4;
        /// the time it takes to reload the weapon
        [MMCondition("MagazineBased", true)]
        [field: Tooltip("the time it takes to reload the weapon")]
        [field: SerializeField] public float ReloadTime { get; protected set; } = 0.25f;
        /// How long after a shot before the weapon begins to reload
		[field: Tooltip("How long after a shot before the weapon begins to reload")]
        [field: SerializeField] public float ReloadDelay { get; protected set; } = 0.75f;
        /// the amount of ammo consumed everytime the weapon fires
        [MMCondition("MagazineBased", true)]
        [field: Tooltip("the amount of ammo consumed everytime the weapon fires")]
        [field: SerializeField] public int AmmoConsumedPerShot { get; protected set; } = 1;
        /// the current amount of ammo loaded inside the weapon
        [MMCondition("MagazineBased", true)]
        [field: MMReadOnly]
        [field: Tooltip("the current amount of ammo loaded inside the weapon")]
        [field: SerializeField] public int CurrentAmmoLoaded { get; protected set; } = 0;

        [field: Header("Position")]


        /// should that weapon be flipped when the character flips ?
		[field: Tooltip("should that weapon be flipped when the character flips ?")]
        [field: SerializeField] public bool FlipWeaponOnCharacterFlip { get; protected set; } = true;
        /// the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs
        [field: Tooltip("the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs")]
        [field: SerializeField] public Vector3 FlipValue { get; protected set; } = new Vector3(-1, 1, 1);

        [field: Header("Projectile Properties")]

        /// the transform to use as the center reference point of the spawn
		[field: Tooltip("the transform to use as the center reference point of the spawn")]
        [field: SerializeField] public Transform ProjectileSpawnTransform { get; protected set; }
        /// the offset position at which the projectile will spawn
        [field: Tooltip("the offset position at which the projectile will spawn")]
        [field: SerializeField] public Vector3 ProjectileSpawnOffset { get; protected set; } = Vector3.zero;
        /// the number of projectiles to spawn per shot
        [field: Tooltip("the number of projectiles to spawn per shot")]
        [field: SerializeField] public int ProjectilesPerShot { get; protected set; } = 1;
        /// the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile
        [field: Tooltip("the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile")]
        [field: SerializeField] public Vector3 Spread { get; protected set; } = Vector3.zero;
        /// whether or not the weapon should rotate to align with the spread angle
        [field: Tooltip("whether or not the weapon should rotate to align with the spread angle")]
        [field: SerializeField] public bool RotateWeaponOnSpread { get; protected set; } = false;
        /// whether or not the spread should be random (if not it'll be equally distributed)
        [field: Tooltip("whether or not the spread should be random (if not it'll be equally distributed)")]
        [field: SerializeField] public bool RandomSpread { get; protected set; } = true;
        /// the object pooler used to spawn projectiles, if left empty, this component will try to find one on its game object
        [field: Tooltip("the object pooler used to spawn projectiles, if left empty, this component will try to find one on its game object")]
        [field: SerializeField] public MMObjectPooler ObjectPooler { get; protected set; }
        /// the local position at which this projectile weapon should spawn projectiles
        [field: MMReadOnly]
        [field: Tooltip("the local position at which this projectile weapon should spawn projectiles")]
        [field: SerializeField] public Vector3 SpawnPosition { get; protected set; } = Vector3.zero;

        [field: Header("Movement")]

        /// How long does this weapon cause the player to stall in the air when fired
        [field: Tooltip("How long does this weapon cause the player to stall in the air when fired")]
        [field: SerializeField] public float AirStallTime { get; protected set; } = 0.2f;

        public CharacterIRE Owner { get; protected set; }
        public CharacterHandleWeaponStandalone HandleWeapon { get; protected set; }
        public MMStateMachine<WeaponStates> WeaponState { get; protected set; }
        public bool Flipped;

        protected float _delayBeforeUseCounter = 0f;
        protected float _delayBetweenUsesCounter = 0f;
        protected float _delayCooldownCounter = 0f;
        protected float _reloadingCounter = 0f;
        protected bool _triggerReleased = false;
        protected bool _reloading = false;

        protected float _lastShot;
        protected float _lastReload;

        protected Vector3 _flippedProjectileSpawnOffset;
        protected Vector3 _randomSpreadDirection;
        protected Vector3 _spawnPositionCenter;
        protected bool _poolInitialized = false;
        protected bool _initialized = false;

        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            if (!_initialized)
            {
                Flipped = false;
                WeaponState = new MMStateMachine<WeaponStates>(gameObject, true);
                _initialized = true;
            }
        }

        public virtual void SetOwner(CharacterIRE owner, CharacterHandleWeaponStandalone handleWeapon)
        {
            Owner = owner;
            HandleWeapon = handleWeapon;
        }

        public virtual void SetReloadTime(float time)
        {
            ReloadTime = time;
        }

        public virtual void SetReloadDelay(float delay)
        {
            ReloadDelay = delay;
        }

        /// <summary>
        /// Ask this weapon component to shoot if possible
        /// </summary>
        public virtual void RequestShoot()
        {
            if (_reloading)
            {
                return;
            }
            if (MagazineBased)
            {
                if (CurrentAmmoLoaded > 0)
                {
                    WeaponState.ChangeState(WeaponStates.WeaponUse);
                }
            }
            else
            {
                WeaponState.ChangeState(WeaponStates.WeaponUse);
            }
        }

        /// <summary>
        /// Stops the weapon
        /// </summary>
        public virtual void StopWeapon()
        {
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }

        protected virtual void LateUpdate()
        {
            ProcessWeaponState();
            AutoReloadWeapon();
        }

        /// <summary>
        /// Every frame, shoot or stop depending on weapon state
        /// Processes in LateUpdate so that WeaponUse starts on the same frame as the input was received
        /// </summary>
        protected virtual void ProcessWeaponState()
        {
            switch (WeaponState.CurrentState)
            {
                case WeaponStates.WeaponDelayBetweenUses:
                    CaseWeaponDelayBetweenUses();
                    break;
                case WeaponStates.WeaponUse:
                    CaseWeaponUse();
                    break;
                case WeaponStates.WeaponIdle:
                    break;
            }
        }

        /// <summary>
        /// Use the weapon
        /// </summary>
        protected virtual void CaseWeaponUse()
        {
            if (MagazineBased)
            {
                if (CurrentAmmoLoaded <= 0)
                {
                    WeaponState.ChangeState(WeaponStates.WeaponIdle);
                    return;
                }
                CurrentAmmoLoaded -= AmmoConsumedPerShot;
            }

            _lastShot = Time.time;
            _delayBetweenUsesCounter = TimeBetweenUses;
            WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
            if (HandleWeapon != null) HandleWeapon.AirStall(AirStallTime);
            DetermineSpawnPosition();
            for (int i = 0; i < ProjectilesPerShot; i++)
            {
                SpawnProjectile(SpawnPosition, i, ProjectilesPerShot, true);
            }
        }

        /// <summary>
        /// Delay between weapon uses
        /// Use weapon once delay is up
        /// </summary>
        protected virtual void CaseWeaponDelayBetweenUses()
        {
            _delayBetweenUsesCounter -= Time.deltaTime;
            if (_delayBetweenUsesCounter <= 0)
            {
                WeaponState.ChangeState(WeaponStates.WeaponUse);
                CaseWeaponUse();
            }
        }

        /// <summary>
        /// Sets current ammo value
        /// </summary>
        /// <param name="newAmmo"></param>
        public virtual void SetCurrentAmmo(int newAmmo)
        {
            CurrentAmmoLoaded = newAmmo;
        }

        /// <summary>
        /// Sets magazine size
        /// </summary>
        /// <param name="magSize"></param>
        public virtual void SetMagazineSize(int magSize)
        {
            MagazineSize = magSize;
        }

        /// <summary>
		/// Spawns a new object and positions/resizes it
		/// </summary>
		public virtual GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            /// we get the next object in the pool and make sure it's not null
            GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

            // mandatory checks
            if (nextGameObject == null) { return null; }
            if (nextGameObject.GetComponent<MMPoolableObject>() == null)
            {
                throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
            }
            // we position the object
            nextGameObject.transform.position = spawnPosition;
            // we set its direction

            nextGameObject.TryGetComponent(out Projectile_PW projectile);
            if (projectile != null)
            {
                projectile.SetWeapon(this);
                if (Owner != null)
                {
                    projectile.SetOwner(Owner.gameObject);
                }
            }
            // we activate the object
            nextGameObject.SetActive(true);


            if (projectile != null)
            {
                if (RandomSpread)
                {
                    _randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
                    _randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
                    _randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
                }
                else
                {
                    if (totalProjectiles > 1)
                    {
                        _randomSpreadDirection.x = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.x, Spread.x);
                        _randomSpreadDirection.y = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.y, Spread.y);
                        _randomSpreadDirection.z = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.z, Spread.z);
                    }
                    else
                    {
                        _randomSpreadDirection = Vector3.zero;
                    }
                }

                Quaternion spread = Quaternion.Euler(_randomSpreadDirection);
                bool facingRight = Owner != null;
                projectile.SetDirection(spread * transform.right * (Flipped ? -1 : 1), transform.rotation, facingRight);
                if (RotateWeaponOnSpread)
                {
                    this.transform.rotation = this.transform.rotation * spread;
                }
            }

            if (triggerObjectActivation)
            {
                if (nextGameObject.GetComponent<MMPoolableObject>() != null)
                {
                    nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
                }
            }

            return (nextGameObject);
        }

        /// <summary>
        /// Determines the spawn position based on the spawn offset and whether or not the weapon is flipped
        /// </summary>
        public virtual void DetermineSpawnPosition()
        {
            _spawnPositionCenter = (ProjectileSpawnTransform == null) ? this.transform.position : ProjectileSpawnTransform.transform.position;

            if (Flipped && FlipWeaponOnCharacterFlip)
            {
                SpawnPosition = _spawnPositionCenter - this.transform.rotation * _flippedProjectileSpawnOffset;
            }
            else
            {
                SpawnPosition = _spawnPositionCenter + this.transform.rotation * ProjectileSpawnOffset;
            }
        }

        /// <summary>
		/// Autoreload weapon
		/// Should use coroutine for autoreload??
		/// </summary>
		protected virtual void AutoReloadWeapon()
        {
            if (((Time.time - _lastShot) < ReloadDelay)
                || (CurrentAmmoLoaded == MagazineSize)
                || ((Time.time - _lastReload) < ReloadTime)
                || WeaponState.CurrentState != WeaponStates.WeaponIdle)
            {
                return;
            }
            CurrentAmmoLoaded++;
            _lastReload = Time.time;
        }

        /// <summary>
        /// When the weapon is selected, draws a circle at the spawn's position
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            DetermineSpawnPosition();

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
        }
    }
}
