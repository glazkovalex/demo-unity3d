#region Поля и свойства
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
//using Wintellect.Threading.AsyncProgModel;
using Random = UnityEngine.Random;

/// <summary>
/// Фабрика объектов использующая менеджер ресурсов.
/// Перед использованием необходимо вызвать метод Init ! 
/// </summary>
public class FactoryPrefabs : MonoBehaviour
{
    /// <summary>
    /// Средний масштаб префаба. Свойство потокобезопасно. 
    /// </summary>
    public float AverageScale {
        get { return _averageScale; }
        set {
            if (Math.Abs(value - _averageScale) > float.Epsilon)
                _averageScale = value;
        }
    }
    [SerializeField]
    private volatile float _averageScale = 1;

    /// <summary>
    /// Средняя скорость для запускаемых объектов. Свойство потокобезопасно.
    /// </summary>
    public float AverageSpeed {
        get { return _averageSpeed; }
        set { _averageSpeed = value; }
    }
    [SerializeField]
    private volatile float _averageSpeed = 0.1f;
    
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
    [SerializeField]
    private float _deltaScale = 50;
    
   public enum Characteristic
    {
        Linear = 1, // Делить единожды
        Square = 2, // Делить на квадрат
        Cubic  = 3  // Делить на куб     
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
        /// Компонент содержащий настройки поведения префаба
        /// </summary>
        public BehaviourPrefab Behaviour { 
            get { return _behaviour ?? Prefab.GetComponent<BehaviourPrefab>(); }
            set { _behaviour = value; }
        }
        public GameObject Prefab { get; set; }
        
        private BehaviourPrefab _behaviour;
    }

    private List<Prefabs> prefabs = new List<Prefabs>();

    ///// <summary>
    ///// Шаблонный префаб, который задается через интерфейс
    ///// </summary>
    //public GameObject TemplatePrefab {
    //    get { return _templatePrefab; }
    //    set {
    //        if (_templatePrefab != null && _templatePrefab != value) {
    //            _templatePrefab = value;
    //        }
    //    }
    //}
    //[SerializeField]
    private GameObject _templatePrefab;

    private float _halfFlightWidth;

#endregion

    /// <summary>
    /// Инициализатор вместо конструктора
    /// </summary>
    public void Init(float halfFlightWidth) {
        _halfFlightWidth = halfFlightWidth;
    }
	
    void Awake() {
        if (RemoteData.DataValid) // Если данные уже загрузить, то проинициализароваться 
            RemoteDataOnAllComplete(new Object(), new EventArgs());
        else // Подождать загрузки данных
            RemoteData.AllComplete += RemoteDataOnAllComplete;
    }

    private void RemoteDataOnAllComplete(object sender, EventArgs eventArgs) {
        // Инициализация ресурсов загружаемых из удаленного источника и отписка
        _templatePrefab = RemoteData.RequiredAssets["DefSphere"].obj as GameObject;
        RemoteData.AllComplete -= RemoteDataOnAllComplete; 
    }

    // Use this for initialization
    void Start() {
        //transform.position = new Vector3(transform.localPosition.x, _halfLenghtPath, transform.localPosition.z);
    }

    // Update is called once per frame
    void Update() {
        
    }

    public BehaviourPrefab LaunchPrefab() {
        // Поискать свободный среди имеющихся.
        // Если нет нет, то заказать создание нового объекта
        // заказать запуск 
        return LaunchNewSphere();
        
    }

    public void RunDoWork(int n) {
        ThreadPool.QueueUserWorkItem(o => DoWork(n));
    }

    int DoWork(int n) {
        int resuIt = 1;
        for (int i = 0; i < n; i++) {
            resuIt++;
        }
        return resuIt;
    }

    private BehaviourPrefab LaunchNewSphere() {
        BehaviourPrefab behaviour = null;
        if (RemoteData.DataValid) {
            //Расчет масштаба префаба 
            float differentialScale = Random.Range(1 - _deltaScale/100, 1 + _deltaScale/100);
            float scale = _averageScale * differentialScale;
            
            // Размещение префаба
            float halfAvailableFlightWidth = _halfFlightWidth - scale / 2; //Чтобы помещались целиком
            GameObject prefab = Instantiate(_templatePrefab, transform.rotation *
                new Vector3(Random.Range(-halfAvailableFlightWidth, halfAvailableFlightWidth),
                    BehaviourPrefab.HalfFlightLength, 0) + transform.position, transform.rotation) as GameObject;
            
            if (prefab != null) {
                // Задание размера и скорости

                prefab.transform.localScale = Vector3.one * scale;
                behaviour = prefab.GetComponent<BehaviourPrefab>();
                if (behaviour != null) { // Реализую степенные зависимости скорости от размера
                    behaviour.Speed = _averageSpeed/Mathf.Pow(differentialScale, (int) _dividerSpeedOfSize);
                    //DebugF.Log("У нового префаба № {0} размер : {1}, скорость : {2}",
                    //    prefabs.Count, prefab.transform.localScale, behaviour.Speed);
                }
                else
                    DebugF.LogError("В назначенном префабе нет ожидаемого компонента потомка {0} ",
                                    behaviour.GetType().Name);
                prefab.transform.parent = gameObject.transform;
                prefabs.Add(new Prefabs
                {
                    Prefab = prefab,
                    Behaviour = behaviour, 
                });
            }
            else Debug.LogError("Не удалось разместить на сцене копию образцового префаба");
            //Debug.Log("AverageSpeed:" + _averageSpeed + ", CustomSpeed:" + _averageSpeed * UnityEngine.Random.Range(1 - _deltaSpeed / 100, 1 + _deltaSpeed / 100));
        }
        return behaviour;
    }

    
}


