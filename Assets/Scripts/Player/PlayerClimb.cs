using System;
using System.Collections;
using UnityEngine;

public class PlayerClimb : MonoBehaviour
{
    [Header("Status")]
    public bool climbing;
    public bool checkClimb;
    public bool forceClimb;
    
    [Header("Options")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private LayerMask climbLayer;
    [SerializeField] private Vector3 climbOffset = new Vector3(0.2f, 0.38f, 0f);
    [SerializeField] private Vector3 climbSphereCastDir = Vector3.forward;
    [SerializeField] private float climbSphereCastRadius = 0.5f;
    [SerializeField] private float climbSphereCastDis = 1.5f;
    [SerializeField] private float climbEndDis = 0.1f;

    [Header("Access")]
    [SerializeField] private Transform head;
    
    private Vector3 climbPos;
    private float defaultColliderHeight;
    private float climbLayerWeight;

    private Router _router;

    private void Start()
    {
        _router = FindFirstObjectByType<Router>();
        defaultColliderHeight = _router.player.status.collider.height;
    }

    private void Update()
    {
        SetClimbAnimation();
        HandleClimbing();
    }
    
    private void SetClimbAnimation()
    {
        climbLayerWeight = Mathf.Lerp(climbLayerWeight, climbing ? 1 : 0, 10f * Time.deltaTime);
        _router.player.status.anim.SetLayerWeight(_router.player.status.anim.GetLayerIndex("Climb"), climbLayerWeight);
    }

    private void HandleClimbing()
    {
        if (climbing)
        {
            Vector3 pos = transform.position;
            Debug.DrawLine(pos, climbPos, Color.red);

            if (Vector3.Distance(pos, climbPos) < climbEndDis)
            {
                climbing = false;
                return;
            }

            _router.player.status.rigid.velocity = (climbPos - pos).normalized * climbSpeed;
            _router.player.status.collider.height = Mathf.Max(_router.player.status.collider.height - 3.5f * Time.deltaTime, defaultColliderHeight / 1.5f);
        }
        else
        {
            if ((Input.GetKeyDown(KeyCode.W) && _router.player.movement.isGrounded && !_router.player.combat.death) || forceClimb)
            {
                checkClimb = CheckClimbableSurface();
                bool canClimb = checkClimb;

                if (canClimb)
                {
                    _router.player.status.anim.SetTrigger("ClimbTrigger");
                    climbing = true;
                }
                else
                    _router.player.movement.Jump();

                forceClimb = false;
            }

            _router.player.status.collider.height = Mathf.Min(_router.player.status.collider.height + 3.5f * Time.deltaTime, defaultColliderHeight);
        }
    }

    private bool CheckClimbableSurface()
    {
        Ray ray = new Ray(head.position - head.TransformVector(climbSphereCastDir).normalized, head.TransformVector(climbSphereCastDir));
        if (Physics.SphereCast(ray, climbSphereCastRadius, out RaycastHit hit, climbSphereCastDis, climbLayer))
        {
            int directionMultiplier = _router.player.movement.direction == PlayerMovement.Direction.Right ? 1 : -1;
            climbOffset.x = Mathf.Abs(climbOffset.x) * directionMultiplier;
            
            climbPos = hit.point + hit.transform.TransformVector(climbOffset);
            Debug.DrawLine(ray.origin, hit.point, Color.green);

            return true;
        }
        return false;
    }
}
