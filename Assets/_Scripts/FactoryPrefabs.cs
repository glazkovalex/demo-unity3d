using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class FactoryPrefabs : MonoBehaviour
{
    /// <summary>
    /// Средний масштаб префаба
    /// </summary>
    [SerializeField]
    private float _averageScale = 1;
    /// <summary>
    /// Средний масштаб префаба
    /// </summary>
    public float AverageScale {
        get { return _averageScale; }
        set {
            if (Math.Abs(value - _averageScale) > float.Epsilon)
                _averageScale = value;
        }
    }

    /// <summary>
    /// Разброс масштаба префабов
    /// </summary>
    public float DeltaScale {
        get { return _deltaScale; }
        set {
            if (Math.Abs(value - _deltaScale) > float.Epsilon)
                _deltaScale = value;
        }
    }
    /// <summary>
    /// Разброс масштабов префабов
    /// </summary>
    [SerializeField]
    private float _deltaScale = 50;
    

    /// <summary>
    /// Средняя скорость, с которой перемещается префабы
    /// </summary>
    public float AverageSpeed {
        get { return _averageSpeed; }
        set {
            if (Math.Abs(value - _averageSpeed) > float.Epsilon)
                _averageSpeed = value;
        }
    }
    [SerializeField]
    private float _averageSpeed;

    public enum Characteristic
    {
        Linear, // Делить единожды
        Square, // Делить на квадрат
        Cubic   // Делить на куб     
    }

    /// <summary>
    /// Зависимость скорости от уменьшения размера
    /// </summary>
    public Characteristic DividerSpeedOfSize {
        get { return _dividerSpeedOfSize; }
        set {
            _dividerSpeedOfSize = value;
            //foreach (var item in prefabs) {
            //    item.Value.speedFactor = UnityEngine.Random.Range(1 - _dividerSpeedOfSize / 100, 1 + _dividerSpeedOfSize / 100);
            //}
        }
    }
    [SerializeField]
    private Characteristic _dividerSpeedOfSize;
    
    private class Prefabs
    {
        /// <summary>
        /// префаб
        /// </summary>
        public GameObject Prefab { get; set; }

        /// <summary>
        /// Компонент содержащий настройки поведения префаба
        /// </summary>
        public BehaviourPrefab Behaviour { 
            get { return _behaviour ?? Prefab.GetComponent<BehaviourPrefab>(); }
            set { _behaviour = value; }
        }
        private BehaviourPrefab _behaviour;
    }

    private List<Prefabs> prefabs = new List<Prefabs>();

    /// <summary>
    /// Шаблонный префаб, который задается через интерфейс 
    /// </summary>
    [SerializeField]
    private GameObject _templatePrefab;
    /// <summary>
    /// Шаблонный префаб, который задается через интерфейс
    /// </summary>
    public GameObject TemplatePrefab {
        get { return _templatePrefab; }
        set {
            if (_templatePrefab != null && _templatePrefab != value) {
                _templatePrefab = value;
            }
        }
    }

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(1)) {// Добавить фигуру
            NewSphere();
        }
    }

    void NewSphere() {
        if (_templatePrefab != null) {
            //float tempHeightFactor;
            //запоминание свежеразмещенного префаба в случайных координатах при выравнивании поворота по родительскому объекту.
            //tempHeightFactor = UnityEngine.Random.Range(1 - _deltaHeight / 100, 1 + _deltaHeight / 100);
            GameObject prefab = Instantiate(_templatePrefab,
                transform.rotation * new Vector3(Random.Range(-5, 5), 5, 0) +
                    transform.position, transform.rotation/*Quaternion.identity*/) as GameObject;
            
            if (prefab != null) {
                // Задание размера и скорости
                float differentialScale = Random.Range(1 - _deltaScale/100, 1 + _deltaScale/100);
                prefab.transform.localScale = Vector3.one * _averageScale * differentialScale;
                BehaviourPrefab behaviour = prefab.GetComponent<BehaviourPrefab>();
                if (behaviour != null) { // Реализую степенные зависимости скорости от размера
                    behaviour.Speed = _averageSpeed/Mathf.Pow(differentialScale, (int) _dividerSpeedOfSize);
                    DebugF.Log("У нового префаба № {0} размер : {1}, скорость : {2}",
                        prefabs.Count, prefab.transform.localScale, behaviour.Speed);
                    // Подписываюсь на подсчет очков
                    behaviour.Burst += CalcScore;
                }
                else
                    DebugF.LogError("В назначенном префабе нет ожидаемого компонента потомка {0} ",
                                    behaviour.GetType().Name);
                prefab.transform.parent = gameObject.transform;
                prefabs.Add(new Prefabs
                {
                    Prefab = prefab,
                    Behaviour = behaviour
                });
            }
            else Debug.LogError("Не удалось разместить на сцене копию образцового префаба");
            //Debug.Log("AverageSpeed:" + _averageSpeed + ", CustomSpeed:" + _averageSpeed * UnityEngine.Random.Range(1 - _deltaSpeed / 100, 1 + _deltaSpeed / 100));
        }
        else Debug.LogWarning("Не задан образец префаба. Задайте его в интерфейсе");
    }

    private int _score = 0;
    
    private void CalcScore(object sender, EventArgs eventArgs) {
        BehaviourPrefab behaviour = sender as BehaviourPrefab;
        // Суммирую счет и отписываюсь. Очки счета = 100*скорость, которая есть функция от масштаба.
        DebugF.Log("Лопнули префаб со скоростью : " + behaviour.Speed);
        _score += (int)(behaviour.Speed * 100); 
        behaviour.Burst -= CalcScore;
    }

    /// <summary>
    /// OnGUI вызывается для отрисовки и обработки событий GUI
    /// </summary>
    void OnGUI() {
         GUI.Label(new Rect(0,0,200,30), _score.ToString());
    }
}

/// <summary>
/// Базовый класс определяющий поведение префабов на сцене
/// </summary>
public abstract class BehaviourPrefab : MonoBehaviour
{
    /// <summary>
    /// Текущая скорость префаба
    /// </summary>
    public virtual float Speed { get; set; }

    /// <summary>
    /// Префаб был "лопнут"
    /// </summary>
    public event EventHandler Burst;

    /// <summary>
    /// Оболочка для зажигания событие префаб "лопнули" из потомков.
    /// </summary>
    /// <param name="e">Аргументы события</param>
    protected virtual void OnBurst(EventArgs e) {
        if (Burst != null)
            Burst(this, e);
        else
            DebugF.Log("Подписчиков нет :(");
    }
}
