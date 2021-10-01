using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSnake : MonoBehaviour
{
    public Animation rootAnim;
    public Animation modelAnim;
    public Renderer meshRenderer;

    public void Awake() {
        meshRenderer.material.color = new Color(1, 0, 0, 0);
    }

	public void PlayAnimation()
    {
        rootAnim.Play("tutorial_climb");
        modelAnim["slither"].speed = 0.4f;
        modelAnim.Play("slither");
	}
}
