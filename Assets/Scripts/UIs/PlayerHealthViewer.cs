﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthViewer : Player.Viewer
{
    [SerializeField, Range(0, 1)] float health;
    [SerializeField] float dangerThreshold;
    [SerializeField] UnityEngine.UI.Image dangerImage;
    [SerializeField] AudioClip dangerClip;
    [SerializeField] AudioSource audioSource;

    //[SerializeField] UnityEngine.UI.Slider _healthSlider;
    [SerializeField] UnityEngine.UI.Image healthGaugeImage;

    bool isInDanger = false;
    bool IsInDanger 
    {
        set 
        {
            if(value && !isInDanger)
            {
                dangerImage.gameObject.SetActive(true);
                audioSource.PlayOneShot(dangerClip);
            }
            if(!value && isInDanger)
            {
                dangerImage.gameObject.SetActive(false);
            }
            isInDanger = value;
        }
    }

    private void Start()
    {
        isInDanger = true;
        IsInDanger = false;
    }

    // Update is called once per frame
    void Update()
    {
        health = Health / MaxHealth;
        //_healthSlider.value = health;
        healthGaugeImage.fillAmount = health;

        IsInDanger = Health <= dangerThreshold;
    }

}
