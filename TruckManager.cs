using System.Collections.Generic;
using UnityEngine;

public class TruckManager : MonoBehaviour
{
    public float speed = 8f;
    [SerializeField] private Vector3 startPosition = new Vector3(-1, 2, -100);
    private Transform currentTrack;
    TruckMovement truckMovement;
    void Start()
    {
      truckMovement  = new TruckMovement(this);
    }

    void Update()
    {
        truckMovement.MoveTruck();
        var truckCenter = this.transform.position + Vector3.up + Vector3.forward; 

        if (Physics.Raycast(truckCenter, Vector3.down, out var hit, 2f))
        {
            //Debug.Log(hit.collider.name);
            var newHitTile = hit.transform;
            if(currentTrack == null)
            {
                currentTrack = newHitTile;
            }
            else if (newHitTile != currentTrack)
            {
                currentTrack = newHitTile;
                if(hit.collider.tag == "Respawn") Respawn();
                if(hit.collider.tag == "endTile") Destroy(this.gameObject);
            }
        }
    }
    void Respawn()
    {
        var gameObject = Instantiate(this, startPosition,this.transform.localRotation);
    }
}
