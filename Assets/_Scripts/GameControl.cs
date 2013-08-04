using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Основной класс игры.
/// Через фбрику запускает абстрактные префабы. Использую менеджер ресурсов
/// разнообразит внешний вид префабов префабов, настраивая фабрику. 
/// Считает очки и повышает уровень скорости игры. Пишет в GUI надписи.
/// </summary>
[RequireComponent(typeof(FactoryPrefabs)), RequireComponent(typeof(ResourceManager))]
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

    /// <summary>
    /// Фабрика объектов
    /// </summary>
    private FactoryPrefabs _factory;
    private ResourceManager _resourceManager;

    private const string DEFAULT_NANE_QUAD_GAME_FIELD = "QuadGameField";
    
    private Transform _quadGameField;

    enum SizesTextures //Чисто для наглядности
    {
        X32,
        X64,
        X128,
        X256,
        Count
    }

    class Header
    {
        public bool Show = false;
        public string Message = string.Empty;
    }
    /// <summary>
    /// Показать или нет сообщение
    /// </summary>
    readonly Header _header = new Header();

	// Use this for initialization
    void Start() {
        InitFromRemoteData();
        
        _resourceManager = GetComponent<ResourceManager>();
        _resourceManager.Init((int)SizesTextures.Count, 128, 32); //128 наборов текстур. 32 - наименьшей размер текстур  

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
        _factory = GetComponent<FactoryPrefabs>();
        _factory.Init(_quadGameField.localScale.x/2, _resourceManager.textures);

        // Запуск обновления прошедшего времени
        InvokeRepeating("UpdateElapsedTime", 0.0f, 1.0f);

        // Запускаю выдачу игровых объектов
        InvokeRepeating("RunPrefab", 0.0f, _currentStatus.launchRate);
    }

    /// <summary>
    /// Инициализация локальных объектов данными из удаленного источника
    /// </summary>
    void InitFromRemoteData() {
        if (!RemoteData.DataValid) { // Подождать загрузки данных
            RemoteData.AllComplete += InitFromRemoteData;
            return;
        }
        //Иначе данные уже загрузились, можно отписаться и проинициализароваться 
        RemoteData.AllComplete -= InitFromRemoteData;
        // Инициализация локальных объектов данными из удаленного источника и отписка:
        _skin = RemoteData.RequiredAssets["MenuSkin"].obj as GUISkin;
    }
	
    /// <summary>
    /// подписывается на подсчет очков и заказывает фабрике запуск очередного префаба
    /// </summary>
    void RunPrefab() {
        BehaviourPrefab bp = _factory.LaunchPrefab();
        if (bp != null)
            bp.Burst += CalcScore; // Подписываюсь на подсчет очков;
    }

    /// <summary>
    /// Перенастроить все необходимые параметры во всех затрагиваемых компонентах
    /// для перехода на следующий уровень сложности игры
    /// </summary>
    public IEnumerator ToNextLevel() {
        // ToDo: Потом можно вынести все параметры изменяемые в этом методы в абстрактные классы, для инверсии зависимости   
        // Отмена выдачи игровых объектов
        CancelInvoke("RunPrefab");
        
        // Изменение настроек для нового уровня
        _currentStatus.nCurrentlevel++;
        _factory.AverageSpeed *= _factorComplicationLevels;//factor; 
        _currentStatus.launchRate /= _factorComplicationLevels; // factor;

        // Ожидаю готовности очередного набора текстур, если он вдруг еще не готов
        _header.Message = string.Format("Вы достигли уровня сложности № {0}!", _currentStatus.nCurrentlevel);
        _header.Show = true;
        while (_resourceManager.NextSetEmpty) {
            yield return new WaitForSeconds(0.1f);
        }
       
        // Как дождался назначаю меняю актуальный набор текстур, сообщаю его фабрике и запускаю выдачу игровых объектов
        _resourceManager.ChangeSets();
        _factory.Textures = _resourceManager.textures;
        if (_currentStatus.launchRate < 0.0001f)
            _currentStatus.launchRate = 0.0001f;
        InvokeRepeating("RunPrefab", 0.0f, _currentStatus.launchRate);

        //Двух секундная задержка убирания надписи
        yield return new WaitForSeconds(2.0f);
        _header.Show = false;
    }
    
    private int _currentScore = 0;
    private int _currentNBurst = 0;
    // В этом обработчике событий от префабов подсчитываю очки
    private void CalcScore(object sender, EventArgs eventArgs) {
        BehaviourPrefab behaviour = sender as BehaviourPrefab;
        // Суммирую счет и отписываюсь. Очки счета = 100*скорость, которая есть функция от масштаба.
        behaviour.Burst -= CalcScore;
        if (behaviour.Speed > float.Epsilon) {
            _currentScore += (int) (behaviour.Speed*100); // Если лопнулся сам, дойдя до края, то скорость = 0. 
            if (_currentNBurst < _nBurstToNexnLevel)
                _currentNBurst++;
            else {
                _currentNBurst = 0;
                StartCoroutine(ToNextLevel());
                //DebugF.Log("Осуществлен переход на уровень " + _currentStatus.nCurrentlevel);
            }
        }
    }
    
    /// <summary>
    /// OnGUI вызывается для отрисовки и обработки событий GUI
    /// </summary>
    void OnGUI() {
        if (!RemoteData.DataValid) return;
        if (_header.Show)
            GUI.Label(new Rect(Screen.width/2-200, 100, 400, 40), _header.Message ?? "null",
                _skin.GetStyle("Header")); 
        GUI.Label(new Rect(0, Screen.height - 60, 200, 30), _elapsedTime ?? "null", 
                _skin.GetStyle("Time")); 
        GUI.Label(new Rect(0, Screen.height - 30, 200, 30), _currentScore.ToString(), 
            _skin.GetStyle("Score"));
    }

    // Чуть-чуть разгружаю OnGUI
    private string _elapsedTime = "00:00";
    void UpdateElapsedTime() {
        _elapsedTime = string.Format("{0}:{1}", // Используется памяти : {2} КБ",
            (Time.timeSinceLevelLoad % 3600 / 60).ToString("N0"), // Минуты
            (Time.timeSinceLevelLoad % 60).ToString("N0") // Секунды
            //,(GC.GetTotalMemory(true)/1024).ToString("N0")
            );
    }

    /// <summary>
    /// Метод для тестирования из из интерфейса редактора Unity3D
    /// </summary>
    public void Test() {
        StartCoroutine(ToNextLevel());
    }
}

