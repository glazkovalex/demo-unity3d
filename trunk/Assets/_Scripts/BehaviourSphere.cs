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
    [SerializeField]
    private float _speed = 0.1f;

    private float _halfLenghtPath;
    private const string DEFAULT_NANE_QUAD_GAME_FIELD = "QuadGameField";
    
    // Use this for initialization
    private void Start() {
        if (QuadGameField == null) {
            QuadGameField = transform.parent.FindChild(DEFAULT_NANE_QUAD_GAME_FIELD);
            if (QuadGameField == null)
                DebugF.LogError(
                    "Корректная работа не возможна т.к. поле {0} не инициализировано" +
                    " и в текущем объекте нет Quad'а c именем по умолчанию \"{1}\"",
                    QuadGameField.GetType().GetProperties()[0].Name, DEFAULT_NANE_QUAD_GAME_FIELD);
        }
        _halfLenghtPath = QuadGameField.localScale.y / 2;
        transform.position = new Vector3(transform.localPosition.x, _halfLenghtPath, transform.localPosition.z);
    }

    // Update is called once per frame
    private void Update() {
        // Перемещаю объект до дна игрового поля
        if (transform.localPosition.y > -_halfLenghtPath) {
            transform.position = new Vector3(transform.position.x,
            transform.position.y - _halfLenghtPath * Time.deltaTime * _speed, transform.position.z);
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