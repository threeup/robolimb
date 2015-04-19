using System;
using System.Collections.Generic;
using UnityEngine;
using BasicCommon;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (NavMeshAgent))]
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class AICharacterControl : MonoBehaviour
    {
        public NavMeshAgent agent { get; private set; } // the navmesh agent required for the path finding
        public ThirdPersonCharacter character { get; private set; } // the character we are controlling
        public Transform target; // target to aim for

        public BasicTimer majorTimer = new BasicTimer(10f, true);
        public BasicTimer minorTimer = new BasicTimer(1f, true);

        // Use this for initialization
        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();

	        agent.updateRotation = false;
	        agent.updatePosition = true;
        }


        // Update is called once per frame
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if( majorTimer.Tick(deltaTime) )
            {
                DoMajor();
            }
            if( minorTimer.Tick(deltaTime) )
            {
                DoMinor();
            }


            character.Move(agent.desiredVelocity, false, false);

        }

        void DoMinor()
        {
            if( target != null )
            {
                agent.SetDestination(target.position);
            }
            else
            {
                agent.SetDestination(Vector3.up*1f);   
            }
        }


        void DoMajor()
        {
            int roll = UnityEngine.Random.Range(0,5);
            switch(roll)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    TargetNearest(); 
                    break;
                case 4:
                    TargetItem();
                    break;
                case 5:
                    target = null;
                    break;
            }
        }

        void TargetNearest()
        {
            List<Actor> livingActors = Game.Instance.livingActors;
            float bestDistSq = 999f;
            Actor bestActor = null;
            foreach(Actor actor in livingActors)
            {
                float distSq = (this.transform.position - actor.transform.position).sqrMagnitude;
                if( distSq < bestDistSq )
                {
                    bestDistSq = distSq;
                    bestActor = actor;
                }
            }
            this.target = bestActor.transform;
        }

        void TargetItem()
        {
            List<Item> livingItems = Game.Instance.livingItems;
            float bestDistSq = 999f;
            Item bestItem = null;
            foreach(Item item in livingItems)
            {
                float distSq = (this.transform.position - item.transform.position).sqrMagnitude;
                if( distSq < bestDistSq )
                {
                    bestDistSq = distSq;
                    bestItem = item;
                }
            }
            this.target = bestItem.transform;
        }
    }
}
