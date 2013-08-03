using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FactoryPrefabs))]
public class GameControl : MonoBehaviour
{
    private GUISkin _skin; // Скин для кнопок основного меню
    
    /// <summary>
    /// Начальная частота запуска префабов
    /// </summary>
    public float InitialLaunchRate {
        get { return _initialLaunchRate; }
        set { _initialLaunchRate = value; }
    }
    [SerializeField]
    private float _initialLaunchRate = 1;
    
    /// <summary>
    /// Коэффициент усложнения уровня
    /// </summary>
    public float FactorComplicationLevels {
        get { return _factorComplicationLevels; }
        set { _factorComplicationLevels = value; }
    }
    [SerializeField]
    private float _factorComplicationLevels = 2;

    /// <summary>
    /// Количество "лопаний" префабов до перехода на новый уровень 
    /// </summary>
    public int NBurstToNexnLevel {
        get { return _nBurstToNexnLevel; }
        set { _nBurstToNexnLevel = value; }
    }
    [SerializeField]
    private int _nBurstToNexnLevel = 10;

    /// <summary>
    /// Состояние контроллера игры соответствующее текущему уровню сложности игры
    /// </summary>
    class StatusGame
    {
        public int nCurrentlevel;
        public float launchRate;

        public StatusGame(float initialLaunchRate) {
            nCurrentlevel = 0;
            launchRate = initialLaunchRate;
        }
    }
    
    private StatusGame _currentStatus;

    void Awake() {
        if (RemoteData.DataValid) // Если данные уже загрузить, то проинициализароваться 
            RemoteDataOnAllComplete(new UnityEngine.Object(), new EventArgs());
        else // Подождать загрузки данных
            RemoteData.AllComplete += RemoteDataOnAllComplete;
    }

    private void RemoteDataOnAllComplete(object sender, EventArgs eventArgs) {
        // Инициализация ресурсов загружаемых из удаленного источника и отписка
        _skin = RemoteData.RequiredAssets["MenuSkin"].obj as GUISkin;
        RemoteData.AllComplete -= RemoteDataOnAllComplete;
    }

    /// <summary>
    /// Фабрика объектов использующая менеджер ресурсов
    /// </summary>
    private FactoryPrefabs factory;
    private const string DEFAULT_NANE_QUAD_GAME_FIELD = "QuadGameField";
    
    private Transform _quadGameField;

	// Use this for initialization
    private void Start() {
        // Выяснение размеров игрового поля и инициализация полетной длинны в абстрактном классе префабов
        if (_quadGameField == null) {
            _quadGameField = transform.FindChild(DEFAULT_NANE_QUAD_GAME_FIELD);
            if (_quadGameField == null)
                DebugF.LogError(
                    "Корректная работа не возможна т.к. поле {0} не инициализировано" +
                    " и в текущем объекте нет Quad'а c именем по умолчанию \"{1}\"",
                    _quadGameField.GetType().GetProperties()[0].Name, DEFAULT_NANE_QUAD_GAME_FIELD);
        }
        BehaviourPrefab.HalfFlightLength = _quadGameField.localScale.y/2;

        // Инициализация статуса игры
        _currentStatus = new StatusGame(_initialLaunchRate);

        // Инициализация фабрики
        factory = GetComponent<FactoryPrefabs>();
        factory.Init(_quadGameField.localScale.x/2);

        // Запуск обновления прошедшего времени
        InvokeRepeating("UpdateElapsedTime", 0.0f, 1.0f);

        // Запуск выдачи игровых объектов
        InvokeRepeating("RunPrefab", 0.0f, _currentStatus.launchRate);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1)) { // Правый клик - добавить фигуру
            factory.LaunchPrefab().Burst += CalcScore; // Подписываюсь на подсчет очков;
        }
	}

    void RunPrefab() {
        BehaviourPrefab bp = factory.LaunchPrefab();
        if (bp != null)
            bp.Burst += CalcScore; // Подписываюсь на подсчет очков;
        //factory.RunDoWork(temp);
    }

    /// <summary>
    /// Перенастроить все необходимые параметры во всех затрагиваемых компонентах
    /// для перехода на следующий уровень сложности игры
    /// </summary>
    public void ToNextLevel() {
        // ToDo: Потом вынести все параметры изменяемые в этом методы в абстрактные классы для инверсии зависимости   
        // Отмена выдачи игровых объектов
        CancelInvoke("RunPrefab");
        // Изменение настроек для нового уровня
        _currentStatus.nCurrentlevel++;
        float factor = Mathf.Pow(_factorComplicationLevels, _currentStatus.nCurrentlevel);
        factory.AverageSpeed *= factor; 
        _currentStatus.launchRate /= factor;
        // Запуск выдачи игровых объектов
        InvokeRepeating("RunPrefab", 0.0f, _currentStatus.launchRate);
    }

    private int _currentScore = 0;
    private int _currentNBurst = 0;

    private void CalcScore(object sender, EventArgs eventArgs) {
        BehaviourPrefab behaviour = sender as BehaviourPrefab;
        // Суммирую счет и отписываюсь. Очки счета = 100*скорость, которая есть функция от масштаба.
        //DebugF.Log("Лопнули префаб со скоростью : " + behaviour.Speed);
        _currentScore += (int)(behaviour.Speed * 100);
        behaviour.Burst -= CalcScore;
        if (_currentNBurst < _nBurstToNexnLevel)
            _currentNBurst++;
        else {
            _currentNBurst = 0;
            ToNextLevel();
            DebugF.Log("Осуществлен переход на уровень " + _currentStatus.nCurrentlevel);
        }
    }

    //public Rigidbody projectile;
    //void Update() {
    //    if (Input.GetButton("Fire1"))
    //        CancelInvoke("LaunchProjectile");
        
    //}
    //void LaunchProjectile() {
    //    instance = Instantiate(projectile);
    //    instance.velocity = Random.insideUnitSphere * 5;
    //}
    //void Example() {
    //    InvokeRepeating("LaunchProjectile", 2, 0.3F);
    //}

    // Разгрузка OnGUI
    private string _elapsedTime = "00:00";
    void UpdateElapsedTime() {
        _elapsedTime = string.Format("{0}:{1}", 
            (Time.timeSinceLevelLoad % 3600 / 60).ToString("N0"), // Минуты
            (Time.timeSinceLevelLoad % 60).ToString("N0")); // Секунды
    }

    /// <summary>
    /// OnGUI вызывается для отрисовки и обработки событий GUI
    /// </summary>
    void OnGUI() {
        try {
            GUI.Label(new Rect(0, Screen.height - 60, 200, 30), _elapsedTime ?? "null", _skin.GetStyle("Time")); 
        }
        catch (NullReferenceException) {
            DebugF.LogError("Null : {0} или {1}", _elapsedTime ?? "Null", 
                _skin != null ? _skin.ToString() : "Null");
        }
        GUI.Label(new Rect(0, Screen.height - 30, 200, 30), _currentScore.ToString(), _skin.GetStyle("Score"));
    }
}

/// <summary>
/// Базовый класс определяющий поведение префабов на сцене.
/// Перед экземпляров класса необходимо инициализировать статическое свойство HalfFlightLength!
/// </summary>
public abstract class BehaviourPrefab : MonoBehaviour
{
    /// <summary>
    /// Текущая скорость префаба
    /// </summary>
    public virtual float Speed { get; set; }

    /// <summary>
    /// Длинна полета
    /// </summary>
    public static float HalfFlightLength { get; set; }

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
    }

    public virtual void Launch() {
        
    }
}
