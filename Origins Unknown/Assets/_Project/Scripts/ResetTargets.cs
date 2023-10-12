using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukewarmLobsters.OriginsUnknown
{
    public class ResetTargets : MonoBehaviour
    {
        public List<Target> targetList = new List<Target>();

        private void Start()
        {

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Target"))
            {
                targetList.Add(new Target(obj, obj.transform.position, obj.transform.rotation));
            }
        }

        private void OnCollisionEnter(Collision other)
        {

            if (other.gameObject.tag == "Player")
            {
                print("HE'S TOUCHING ME!!!");
                foreach (Target obj in targetList)
                {
                    obj.target.transform.position = obj.position;
                    obj.target.transform.rotation = obj.rotation;
                    obj.target.GetComponent<Rigidbody>().velocity = new Vector3();
                }
            }
        }
    }

    public class Target
    {
        public GameObject target;
        public Vector3 position;
        public Quaternion rotation;

        public Target(GameObject _target, Vector3 _position, Quaternion _rotation)
        {
            target = _target;
            position = _position;
            rotation = _rotation;
        }

        public override string ToString()
        {
            return $"name: {target.name}";
        }
    }
}