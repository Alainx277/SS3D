using System;
using System.Collections.Generic;
using System.Linq;
using SS3D.Content.Systems.Interactions;
using SS3D.Content.Systems.Player.Body;
using SS3D.Engine.Health;
using SS3D.Engine.Interactions;
using SS3D.Engine.Interactions.Extensions;
using SS3D.Engine.Inventory;
using UnityEngine;
using UnityEngine.Assertions;

namespace SS3D.Content.Systems.Combat
{
    public class Gun : Item
    {
        /// <summary>
        /// What kinds of ammunition this gun can use
        /// </summary>
        public string[] SupportedAmmunition;
        /// <summary>
        /// Which layers block projectiles
        /// </summary>
        public LayerMask BlockProjectiles;
        /// <summary>
        /// Called when the gun is fired
        /// </summary>
        public event EventHandler FireCallback;
        /// <summary>
        /// Called when the gun is fired without ammunition
        /// </summary>
        public event EventHandler EmptyFire;

        public Magazine Magazine { get; private set; }

        void Start()
        {
            Assert.IsNotNull(SupportedAmmunition);
        }

        public override void CreateInteractions(IInteractionTarget[] targets, List<InteractionEntry> interactions)
        {
            base.CreateInteractions(targets, interactions);
            interactions.Insert(0, new InteractionEntry(targets[0], new SimpleInteraction
            {
                Name = "Shoot",
                RangeCheck = false,
                Interact = PressTrigger
            }));
        }

        public override IInteraction[] GenerateInteractions(InteractionEvent interactionEvent)
        {
            List<IInteraction> list = base.GenerateInteractions(interactionEvent).ToList();
            list.Insert(0, new SimpleInteraction
            {
                CanInteractCallback = e =>
                {
                    Magazine magazine = e.Source.GetComponent<Magazine>();
                    return magazine != null && SupportedAmmunition.Contains(magazine.Type);
                },
                Name = "Insert magazine",
                Interact = (e, r) => InsertMagazine(e.Source.GetComponent<Magazine>())
            });
            list.Insert(1, new SimpleInteraction
            {
                CanInteractCallback = e => Magazine != null,
                Name = "Eject magazine",
                Interact = (e, r) => EjectMagazine()
            });
            return list.ToArray();
        }

        /// <summary>
        /// Called when the gun is used
        /// </summary>
        private void PressTrigger(InteractionEvent interactionEvent, InteractionReference reference)
        {
            if (Magazine == null || Magazine.Ammo < 1)
            {
                OnEmptyFire();
                return;
            }
            
            Fire(interactionEvent.Point, (interactionEvent.Target as IGameObjectProvider)?.GameObject);
        }

        /// <summary>
        /// Causes the gun to fire
        /// </summary>
        private void Fire(Vector3 point, GameObject target)
        {
            target = IntersectsBulletPath(transform.position, point) ?? target;
            BodyPart bodyPart = target.GetComponent<BodyPart>();
            if (bodyPart == null)
            {
                // TODO: Non organic damage
            }
            else
            {
                // TODO: Remove hardcoded values
                bodyPart.Damage(DamageType.Brute, 10);
            }
            
            Magazine.Ammo--;
            OnFire();
        }
        
        private GameObject IntersectsBulletPath(Vector3 origin, Vector3 target)
        {
            if (BlockProjectiles == 0)
            {
                return null;
            }

            return Physics.Linecast(origin, target, out RaycastHit hit, BlockProjectiles, QueryTriggerInteraction.Ignore) ? hit.transform.gameObject : null;
        }

        /// <summary>
        /// Inserts a magazine into the gun
        /// </summary>
        private void InsertMagazine(Magazine newMagazine)
        {
            EjectMagazine();
            Magazine = newMagazine;
            SetMagazineState(true);
        }

        /// <summary>
        /// Ejects the current magazine into world space
        /// </summary>
        private void EjectMagazine()
        {
            if (Magazine == null)
            {
                return;
            }
            SetMagazineState(false);
            Magazine.transform.position = transform.position;
        }

        // TODO: Replace with single slot gun inventory
        private void SetMagazineState(bool inserted)
        {
            Item item = Magazine.gameObject.GetComponent<Item>();
            if (item != null)
            {
                item.RemoveFromContainer();
            }
            Collider collider = Magazine.gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = !inserted;
            }
            Rigidbody rigidbody = Magazine.gameObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.isKinematic = inserted;
            }

            MeshRenderer renderer = Magazine.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = !inserted;
            }
        }

        protected virtual void OnFire()
        {
            FireCallback?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnEmptyFire()
        {
            EmptyFire?.Invoke(this, EventArgs.Empty);
        }
    }
}