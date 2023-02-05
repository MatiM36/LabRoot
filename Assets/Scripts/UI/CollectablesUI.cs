using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectablesUI : MonoBehaviour
{
    public TMP_Text countText;
    public Animator animator;

    public string showAnimation;
    public string hideAnimation;

    public float showTime = 2f;

    private bool showing = false;
    private bool forceShowing = false;
    private float showTimer;

    private void Start()
    {
        animator.Play(hideAnimation, 0, 1f);
    }

    public void OnCollectiblePick(int current, int max)
    {
        countText.text = $"{current}/{max}";

        if (!showing)
        {
            animator.Play(showAnimation);
            showing = true;
        }

        showTimer = showTime;
    }

    public void ForceShow(bool show)
    {
        forceShowing = show;
        if (show)
            animator.Play(showAnimation);
        else
        {
            if (!showing)
                animator.Play(hideAnimation);
        }
    }

    private void Update()
    {
        if (forceShowing) return;

        if (showing)
        {
            if (showTimer > 0f)
                showTimer -= Time.deltaTime;
            else
            {
                showing = false;
                animator.Play(hideAnimation);
            }
        }
    }
}
