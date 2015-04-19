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
        public Actor actor;
        public NavMeshAgent agent { get; private set; } // the navmesh agent required for the path finding
        public ThirdPersonCharacter character { get; private set; } // the character we are controlling
        public Transform target; // target to aim for
        private Transform firstTarget; // target to aim for

        public BasicTimer majorTimer = new BasicTimer(10f, true);
        public BasicTimer minorTimer = new BasicTimer(1f, true);
        public BasicTimer runTimer = new BasicTimer(8f, true);
        public AnimationCurve runCurve;

        private float aiDesiredSpeed = 1f;
        // Use this for initialization
        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            actor = GetComponent<Actor>();
            agent = GetComponentInChildren<NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();

	        agent.updateRotation = false;
	        agent.updatePosition = true;
        }


        // Update is called once per frame
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if( firstTarget == null )
            {
                DoMajor(0);
            }
            if( majorTimer.Tick(deltaTime) )
            {
                DoMajor();
            }
            if( minorTimer.Tick(deltaTime) )
            {
                DoMinor();
            }

            runTimer.Tick(deltaTime);

            float throttle = runCurve.Evaluate(runTimer.Percent);
            float actorSpeed = actor.body.GetSpeed();
            float runSpeed = Mathf.Clamp(throttle, 0f, actorSpeed);

            agent.speed = runSpeed*aiDesiredSpeed*1f;
            agent.angularSpeed = (1f-throttle)*120f;



            character.Move(agent.desiredVelocity, 1f, false, false);

        }

        void DoMinor()
        {
            if( target != null )
            {
                float distSq = (this.transform.position - target.position).sqrMagnitude;
                if( distSq < 3f*3f )
                {
                    aiDesiredSpeed = 0.1f;
                    agent.SetDestination(target.position);
                    actor.AIThrow(true);
                }
                else
                {
                    aiDesiredSpeed = 1f;
                    agent.SetDestination(target.position);
                    actor.AIThrow(false);
                }
            }
            else
            {
                aiDesiredSpeed = 0.25f;
                agent.SetDestination(Vector3.up*1f);   
            }
        }


        void DoMajor(int roll = -1)
        {
            if( roll < 0 )
            {
                roll = UnityEngine.Random.Range(0,5);
            } 
            switch(roll)
            {
                case 0:
                    TargetRandom(); 
                    break;
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
            if( firstTarget == null )
            {
                firstTarget = target;
            }
        }

        void TargetRandom()
        {
            List<Actor> livingActors = Game.Instance.livingActors;
            this.target = null;
            for(int i=0; i<livingActors.Count && target == null; ++i)
            {
                int roll = UnityEngine.Random.Range(0, livingActors.Count);
                if( livingActors[roll].team != this.actor.team )
                {
                    this.target = livingActors[roll].transform;
                }
            }
        }

        void TargetNearest()
        {
            List<Actor> livingActors = Game.Instance.livingActors;
            float bestDistSq = 999f;
            Actor bestActor = null;
            foreach(Actor actor in livingActors)
            {
                if( actor.team != this.actor.team )
                {
                    float distSq = (this.transform.position - actor.transform.position).sqrMagnitude;
                    if( distSq < bestDistSq )
                    {
                        bestDistSq = distSq;
                        bestActor = actor;
                    }
                }
            }
            if( bestActor != null )
            {
                this.target = bestActor.transform;
            }
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
            if( bestItem != null )
            {
                this.target = bestItem.transform;
            }
        }
    }
}
