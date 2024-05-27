using UnityEngine;

public class TruckMovement
{
    private TruckManager truck;
    public TruckMovement (TruckManager truck)
    {
        this.truck = truck;
    }
    public void MoveTruck()
    {
        // ターゲット位置へ移動
        truck.transform.Translate(truck.speed * Time.deltaTime * Vector3.forward);
    }
}
