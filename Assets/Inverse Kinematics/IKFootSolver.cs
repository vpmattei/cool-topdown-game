using Unity.VisualScripting;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [SerializeField] LayerMask terrainLayer;
    [SerializeField] IKFootSolver otherFoot = default;
    [SerializeField] Transform body = default;
    [SerializeField] float speed = 1;
    [SerializeField] float stepDistance = 4;
    [SerializeField] float stepLength = 0.4f;
    [SerializeField] float stepHeight = 1;
    [SerializeField] Vector3 footOffset = new Vector3(0, 0.1f, 0);
    [SerializeField] float footSpacing;


    Vector3 oldPosition, currentPosition, newPosition;
    // Vector3 oldNormal, currentNormal, newNormal;
    float lerp;

    private void Start()
    {
        currentPosition = newPosition = oldPosition = transform.position;
        // currentNormal = newNormal = oldNormal = transform.up;
        // lerp = 1;
    }

    void Update()
    {
        transform.position = currentPosition;
        // transform.up = currentNormal;

        Ray ray = new Ray(transform.position + new Vector3(0, .5f, 0), Vector3.down);
        Ray bodyRay = new Ray(body.position + new Vector3(0, .5f, 0) + (body.right * footSpacing), Vector3.down);
        Debug.DrawRay(body.position + new Vector3(0, .5f, 0), new Vector3(0, -30, 0), Color.green);

        if (Physics.Raycast(bodyRay, out RaycastHit hit, 30, terrainLayer.value))
        {
            Debug.Log("Distance: " + Vector3.Distance(newPosition, hit.point));
            if (Vector3.Distance(newPosition, hit.point) > stepDistance)
            {
                int direction = transform.InverseTransformPoint(hit.point).z > transform.InverseTransformPoint(newPosition).z ? 1 : -1;
                currentPosition = newPosition = hit.point + footOffset; //+ (transform.forward * stepDistance * direction);
                lerp = 0;
                // newNormal = hit.normal;
            }

            // if (lerp < 1)
            // {
            //     Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            //     tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            //     currentPosition = tempPosition;
            //     // currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);
            //     lerp += Time.deltaTime * speed;
            // }
            else
            {
                oldPosition = newPosition;
                // oldNormal = newNormal;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.05f);
    }

    // public bool IsMoving()
    // {
    //     // return lerp < 1;
    // }
}
