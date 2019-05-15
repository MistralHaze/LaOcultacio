/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Visibility2D
{
	public class LightCollider : MonoBehaviour {

        public EnemyAI aI;
        public ShadowCasting2D lightCone;

        PolygonCollider2D lightCollider;

		// Use this for initialization
		void Start () {
			
			lightCollider = gameObject.AddComponent<PolygonCollider2D>();
			lightCollider.isTrigger = true;

		}
		
		// Update is called once per frame
		void Update () {

			//Add the light mesh points to the Polygon Collider
			lightCollider.points = lightCone.getLightPoints();
		
		}

		void OnTriggerEnter2D(Collider2D other) {

			if (other.tag == "Player") {

                //print("Light - Player collision");
                //alertSystem.PlayerDetected(this.transform.parent.gameObject);
                aI.EnemyInSight();
			}		
		}

	}
}
*/