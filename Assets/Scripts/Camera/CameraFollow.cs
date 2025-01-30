using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smooth = 3;
    [SerializeField] private bool followForward = true;
    [SerializeField] private bool checkForward = true;
    [SerializeField] private bool forwardBlocked;
    [SerializeField] private float distance = 1;
    [SerializeField] private float distanceToForwardBlock;
    [SerializeField] private LayerMask forwardLayer;
    
    [SerializeField] private float posY = 0;
    [SerializeField] private float posZ = 0;

    private Router _router;

    private void Start()
    {
        _router = FindFirstObjectByType<Router>();
    }

    private void Update()
    {
        if (checkForward)
            forwardBlocked = CheckForwardSurface();
    }

    private void FixedUpdate()
    {
        if (target == null) return;
        
        Follow();
    }

    void Follow()
    {
        Vector3 targetPos;
        float multiple = followForward && target.localScale.z < 0 ? -1 : 1;
        if (forwardBlocked && distanceToForwardBlock > 0.15f) multiple *= -0.4f / distanceToForwardBlock;
        
        targetPos.x = target.position.x + (distance * multiple);
        targetPos.y = (_router.player.combat.mode ? posY : posY + 1.5f) + _router.player.movement.transform.position.y;
        targetPos.z = posZ;
        Vector3 velocity = Vector3.zero;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smooth * Time.deltaTime);
    }
    
    private bool CheckForwardSurface()
    {
        Ray ray = new Ray(_router.player.movement.transform.position - _router.player.movement.transform.TransformVector(Vector3.forward).normalized, _router.player.movement.transform.TransformVector(Vector3.forward));
        bool isHit = Physics.SphereCast(ray, 1, out RaycastHit hit, 5, forwardLayer);
        
        if (isHit)
            distanceToForwardBlock = Vector3.Distance(hit.point, _router.player.movement.transform.position);
        
        return isHit;
    }
}
