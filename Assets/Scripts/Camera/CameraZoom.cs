using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private float normalZoom;
    [SerializeField] private float combatZoom;
    [SerializeField] private float speed = 10;

    private Router _router;

    private void Start()
    {
        _router = FindFirstObjectByType<Router>();

        StartCoroutine(UpdateZoom());
    }

    public IEnumerator UpdateZoom()
    {
        float targetZoom = _router.player.combat.mode ? combatZoom : normalZoom;

        while (Mathf.Abs(Camera.main.fieldOfView - targetZoom) > 1f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetZoom, speed * Time.deltaTime);
        }
    }
}
