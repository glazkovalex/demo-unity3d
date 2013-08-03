using System;
using UnityEngine;
using System.Collections;

public class BehaviourSphere : BehaviourPrefab
{
    public Transform QuadGameField;
    /// <summary>
    /// Текущая скорость сферы. 
    /// </summary>
    public override float Speed {
        get { return _speed; }
        set { _speed = value; }
    }
    //[SerializeField]
    private float _speed = 1.0f;

    private const string DEFAULT_NANE_QUAD_GAME_FIELD = "QuadGameField";

    public override void Launch() {
        
    }

    // Use this for initialization
    private void Start() {
        renderer.material.mainTexture = RemoteData.RequiredAssets["Checker.Black-White"].obj as Texture;
    }

    // Update is called once per frame
    private void Update() {
        // Перемещаю объект до дна игрового поля
        if (transform.localPosition.y > - HalfFlightLength) {
            transform.Translate(0, - Time.deltaTime * _speed, 0, Space.Self);
        }
        else {
            gameObject.SetActive(false);
        }
    }

    void OnMouseDown() {
        OnBurst(new EventArgs()); // Оповещаю о лопании
        gameObject.SetActive(false);
    }
}