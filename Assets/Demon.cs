﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demon : MonoBehaviour
{

    public string doneMsg = "Final Score: ";

    public int mode;

    public float visibleTime;
    public float selectTime;
    public float returnTime;
    public float totalTime;

    public int direction;
    public int trialNum = 0;

    public int[] contexts;
    public float[] holds;

    public int[] leftObjects;
    public int[] leftRewards;
    public int[] rightObjects;
    public int[] rightRewards;

    public float westXPos = 184.2314f;
    public float eastXPos = 304.2314f;
    public float yPos = 3f;
    public float zPos = 255f;

    public float east = 90f;
    public float west = 270f;

    public float holdTurnRate = 90f;
    public float returnRate = 180f;
    public float returnSpeedX = 20f;
    public float returnSpeedZ = 20f;

    public Text rewardText;
    public Text scoreText;

    SimpleMovement move;

    public enum segments
    {
        Hallway,
        HoldA,
        Selection,
        Reward,
        Return,
        HoldB,
    };

    public segments segment;

    public GameObject contextN;
    public GameObject contextS;

    public GameObject objectNE;
    public GameObject objectSE;
    public GameObject objectSW;
    public GameObject objectNW;

    public string context0 = "Gray";
    public string context1 = "Wood";
    public string context2 = "Brick";

    private float selectStart;
    private bool vis;

    private int score;

    // Start is called before the first frame update
    void Start()
    {
        segment = segments.Hallway;
        move = GetComponent<SimpleMovement>();
        rewardText.enabled = false;
        scoreText.text = 0.ToString();
        trialNum = 0;
        SetContexts();
    }

    // Update is called once per frame
    void Update()
    {
        if(segment == segments.Hallway)
        {
            if(direction == 1 && transform.position.x >= eastXPos)
            {
                segment = segments.HoldA;
                move.BeginHold(holds[trialNum]);
                transform.position = new Vector3(eastXPos, transform.position.y, zPos);
                ClearContexts();
            }
            else if(direction == 2 && transform.position.x <= westXPos)
            {
                segment = segments.HoldA;
                move.BeginHold(holds[trialNum]);
                transform.position = new Vector3(westXPos, transform.position.y, zPos);
                ClearContexts();
            }
        }
        
        else if(segment == segments.HoldA)
        {
            Rotate(holdTurnRate);

            if(!move.IsHolding())
            {
                segment = segments.Selection;
                selectStart = Time.time;
                vis = true;
                if(direction == 1)
                {
                    objectNE.SendMessage("Sprite", leftObjects[trialNum]);
                    objectSE.SendMessage("Sprite", rightObjects[trialNum]);
                }
                else if(direction == 2)
                {
                    objectSW.SendMessage("Sprite", leftObjects[trialNum]);
                    objectNW.SendMessage("Sprite", rightObjects[trialNum]);
                }
                else{Debug.Log("Direction machine broke.");}
            }
        }
        
        else if(segment == segments.Selection)
        {
            if(vis && Time.time - selectStart >= visibleTime)
            {
                ClearVisibility();
            }
            if(Time.time - selectStart >= selectTime)
            {
                GiveReward(null);
            }
        }
        
        else if(segment == segments.Reward)
        {
            if(vis) //Need to wait?
            {
                ClearVisibility();
            }
            
            if(direction == 2)
            {
                if(transform.position.x > eastXPos)
                {
                    move.move.x -= returnSpeedX * Time.deltaTime;
                    if(transform.position.x + move.move.x < eastXPos)
                    {
                        move.move.x = eastXPos - transform.position.x;
                    }
                }
            }
            else if(direction == 1)
            {
                if(transform.position.x < westXPos)
                {
                    move.move.x += returnSpeedX * Time.deltaTime;
                    if(transform.position.x + move.move.x > eastXPos)
                    {
                        move.move.x = eastXPos - transform.position.x;
                    }
                }
            }
            else{Debug.Log("Direction machine broke.");}

            if(transform.position.z > zPos)
            {
                move.move.z -= returnSpeedZ * Time.deltaTime;
                if(transform.position.z + move.move.z < zPos)
                {
                    move.move.z = zPos - transform.position.z;
                }
            }
            else if(transform.position.z < zPos)
            {
                move.move.z += returnSpeedZ * Time.deltaTime;
                if(transform.position.z + move.move.z > zPos)
                {
                    move.move.z = zPos - transform.position.z;
                }
            }

            Rotate(returnRate);

            if(!move.IsHolding()){
                segment = segments.HoldB;
                move.BeginHold(999f);
                rewardText.enabled = false;
            }
        }
        
        else if(segment == segments.HoldB)
        {
            if(trialNum + 1 == contexts.Length)
            {
                rewardText.text = doneMsg + score.ToString();
                rewardText.enabled = true;
                scoreText.enabled = false;
            }
            if(Time.time - selectStart >= totalTime)
            {
                move.EndHold();
                segment = segments.Hallway;
                trialNum++;
                try{
                    SetContexts();
                }
                catch(Exception e)
                {
                    //Debug.Log("Final Score: " + score);
                    Application.Quit();
                }
            }
        }
    }

    void GiveReward(GameObject obj)
    {
        if(segment == segments.Selection)
        {
            int reward = 0;
            if(obj == objectNE)
            {
                reward = leftRewards[trialNum];
            }
            else if(obj == objectSE)
            {
                reward = rightRewards[trialNum];
            }
            else if(obj == objectSW)
            {
                reward = leftRewards[trialNum];
            }
            else if(obj == objectNW)
            {
                reward = rightRewards[trialNum];
            }

            segment = segments.Reward;
            rewardText.text = reward.ToString();
            rewardText.enabled = true;
            score += reward;
            scoreText.text = score.ToString();
            move.BeginHold(returnTime);

            if(direction == 1)
            {
                direction = 2;
            }
            else if(direction == 2)
            {
                direction = 1;
            }
            else{Debug.Log("Direction machine broke.");}
        }
    }

    void ClearVisibility()
    {
        int zero = 0;
        objectNE.SendMessage("Sprite", zero);
        objectSE.SendMessage("Sprite", zero);
        objectSW.SendMessage("Sprite", zero);
        objectNW.SendMessage("Sprite", zero);
        vis = false;
    }

    void Rotate(float rate)
    {
        if(direction == 1)
        {
            if(transform.eulerAngles.y < east || transform.eulerAngles.y > west)
            {
                move.rotate.y += Time.deltaTime * rate;
                if(transform.eulerAngles.y + move.rotate.y > east &&
                    transform.eulerAngles.y + move.rotate.y < west)
                {
                    move.rotate.y = east - transform.eulerAngles.y;
                }
            }
            else if(transform.eulerAngles.y > east)
            {
                move.rotate.y -= Time.deltaTime * rate;
                if(transform.eulerAngles.y + move.rotate.y < east &&
                    transform.eulerAngles.y + move.rotate.y > west)
                {
                    move.rotate.y = east - transform.eulerAngles.y;
                }
            }
        }
        else if(direction == 2)
        {
            if(transform.eulerAngles.y > west || transform.eulerAngles.y < east)
            {
                move.rotate.y -= Time.deltaTime * rate;
                if(transform.eulerAngles.y + move.rotate.y < west &&
                    transform.eulerAngles.y + move.rotate.y > east)
                {
                    move.rotate.y = west - transform.eulerAngles.y;
                }
            }
            else if(transform.eulerAngles.y < west)
            {
                move.rotate.y += Time.deltaTime * rate;
                if(transform.eulerAngles.y + move.rotate.y > west &&
                    transform.eulerAngles.y + move.rotate.y < east)
                {
                    move.rotate.y = west - transform.eulerAngles.y;
                }
            }
        }
        else{Debug.Log("Direction machine broke.");}
    }

    void SetContexts()
    {
        if(contexts[trialNum] == 1)
        {
            contextN.SendMessage(context1);
            contextS.SendMessage(context1);
        }
        else if(contexts[trialNum] == 2)
        {
            contextN.SendMessage(context2);
            contextS.SendMessage(context2);
        }
        else{Debug.Log("Context machine broke.");}
    }

    void ClearContexts()
    {
        contextN.SendMessage(context0);
        contextS.SendMessage(context0);
    }

}
