using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        if (Input.GetKeyDown(KeyCode.Q))
            Application.Quit();
    }

    public IEnumerator UpdateTimeScale(float target)
    {
        while (Mathf.Abs(Time.timeScale - target) > 0.1f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            Time.timeScale = Mathf.Lerp(Time.timeScale, target, 10 * Time.deltaTime);
        }
    }

    public IEnumerator SlowMotion(float delay)
    {
        StartCoroutine(UpdateTimeScale(0.3f));
        yield return new WaitForSeconds(delay);
        StartCoroutine(UpdateTimeScale(1.1f));
    }
}
