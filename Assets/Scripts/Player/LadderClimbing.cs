using System;
using System.Collections;
using System.Timers;
using UnityEngine;

public class LadderClimbing : MonoBehaviour
{
    public LayerMask ladderMask;
    public float climbSpeed = 1.0f;
    public float climbWeight = 1.0f;

    private bool isClimbing = false;
    private Transform[] ladderSteps;
    private int currentStepIndex = 0;

    private Transform top;
    private Transform bot;
    
    private Vector3 rightFootIKPosition = new Vector3();
    private Vector3 leftFootIKPosition = new Vector3();
    private Vector3 leftHandIKPosition = new Vector3();
    private Vector3 rightHandIKPosition = new Vector3();

    private Transform rightFootTarget;
    private Transform leftFootTarget;
    private Transform leftHandTarget;
    private Transform rightHandTarget;

    private Router _router;
    private Animator _animator;

    private void Start()
    {
        _router = FindFirstObjectByType<Router>();
        _animator = _router.player.status.anim;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isClimbing)
        {
            bool isNearLadder = CheckLadderForward();
            if (isNearLadder)
                StartClimbing();
        }
    }

    /*private void LateUpdate()
    {
        if (isClimbing)
            SetIK();
    }*/

    void SetIK()
    {
        float smoothSpeed = 30 * Time.deltaTime;
        
        float stepDistanceZ = 0.1f;
        float stepDistanceX = 0.1f;
        
        if (rightFootTarget != null)
            rightFootIKPosition = Vector3.Lerp(rightFootIKPosition, rightFootTarget.position + new Vector3(-stepDistanceX, 0, -stepDistanceZ), smoothSpeed * Time.deltaTime);
        
        if (rightHandTarget != null)
            rightHandIKPosition = Vector3.Lerp(rightHandIKPosition, rightHandTarget.position + new Vector3(-stepDistanceX, 0, -stepDistanceZ), smoothSpeed * Time.deltaTime);
        
        if (leftFootTarget != null)
            leftFootIKPosition = Vector3.Lerp(leftFootIKPosition, leftFootTarget.position + new Vector3(-stepDistanceX, 0, stepDistanceZ), smoothSpeed * Time.deltaTime);
        
        if (leftHandTarget != null)
            leftHandIKPosition = Vector3.Lerp(leftHandIKPosition, leftHandTarget.position + new Vector3(-stepDistanceX, 0, stepDistanceZ), smoothSpeed * Time.deltaTime);
        
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, climbWeight);
        _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, climbWeight);
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, climbWeight);
        _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, climbWeight);
        
        _animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIKPosition);
        _animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIKPosition);
        _animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKPosition);
        _animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKPosition);
    }

    private bool CheckLadderForward()
    {
        Ray ray = new Ray(_router.player.movement.transform.position - _router.player.movement.transform.TransformVector(Vector3.forward).normalized, _router.player.movement.transform.TransformVector(Vector3.forward));
        
        if (Physics.SphereCast(ray, 1, out RaycastHit hit, 1, ladderMask))
        {
            Transform ladder = hit.transform;
            ladderSteps = ladder.GetComponentsInChildren<Transform>();
            
            ladderSteps = Array.FindAll(ladderSteps, step => step.name.StartsWith("stand"));
            
            Array.Sort(ladderSteps, (a, b) => a.position.y.CompareTo(b.position.y));

            currentStepIndex = 0;

            top = hit.transform.Find("top");
            bot = hit.transform.Find("bot");

            return true;
        }
        return false;
    }

    public void StartClimbing()
    {
        if (ladderSteps == null || ladderSteps.Length == 0)
            return;

        StartCoroutine(ClimbLadder());
    }

    private IEnumerator ClimbLadder()
    {
        isClimbing = true;
        _router.player.status.rigid.isKinematic = true;

        while (currentStepIndex < ladderSteps.Length - 3)
        {
            yield return new WaitForSeconds(0.3f);
            currentStepIndex++;
        }

        isClimbing = false;
        _router.player.climb.forceClimb = true;
        _router.player.status.rigid.isKinematic = false;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (isClimbing && ladderSteps != null && ladderSteps.Length > 0)
        {
            int stepIndex = currentStepIndex;
            bool stepTurn = stepIndex % 2 == 0;
            int[] add = new [] { stepTurn ? 1 : 0, stepTurn ? 0 : 1 };

            leftHandTarget = ladderSteps[Mathf.Clamp(stepIndex + add[0] + 3, 0, ladderSteps.Length - 1)];
            rightHandTarget = ladderSteps[Mathf.Clamp(stepIndex + add[1] + 3, 0, ladderSteps.Length - 1)];
            leftFootTarget = ladderSteps[Mathf.Clamp(stepIndex + add[0], 0, ladderSteps.Length - 1)];
            rightFootTarget = ladderSteps[Mathf.Clamp(stepIndex + add[1], 0, ladderSteps.Length - 1)];
            
            float smoothSpeed = 2f;
            
            float posX = ladderSteps[stepIndex].position.x - 0.5f;
            float posY = ladderSteps[stepIndex].position.y;
            float posZ = _router.player.movement.transform.position.z;

            _router.player.movement.transform.position = Vector3.Lerp(_router.player.movement.transform.position, new Vector3(posX, posY, posZ), smoothSpeed * Time.deltaTime);
        
            if (rightFootIKPosition == Vector3.zero) rightFootIKPosition = rightFootTarget.position;
            if (rightHandIKPosition == Vector3.zero) rightHandIKPosition = rightHandTarget.position;
            if (leftFootIKPosition == Vector3.zero) leftFootIKPosition = leftFootTarget.position;
            if (leftHandIKPosition == Vector3.zero) leftHandIKPosition = leftHandTarget.position;
            
            float stepDistanceZ = 0.1f;
            float stepDistanceX = 0.1f;

            rightFootIKPosition = Vector3.Lerp(rightFootIKPosition, rightFootTarget.position + new Vector3(-stepDistanceX, 0, -stepDistanceZ), Time.deltaTime * smoothSpeed);
            rightHandIKPosition = Vector3.Lerp(rightHandIKPosition, rightHandTarget.position + new Vector3(-stepDistanceX, 0, -stepDistanceZ), Time.deltaTime * smoothSpeed);
            leftFootIKPosition = Vector3.Lerp(leftFootIKPosition, leftFootTarget.position + new Vector3(-stepDistanceX, 0, stepDistanceZ), Time.deltaTime * smoothSpeed);
            leftHandIKPosition = Vector3.Lerp(leftHandIKPosition, leftHandTarget.position + new Vector3(-stepDistanceX, 0, stepDistanceZ), Time.deltaTime * smoothSpeed);
        
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, climbWeight);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, climbWeight);
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, climbWeight);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, climbWeight);
        
            _animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIKPosition);
            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIKPosition);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKPosition);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKPosition);
        }
    }
}
