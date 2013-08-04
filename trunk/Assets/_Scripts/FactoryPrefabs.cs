#region Поля и свойства
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Фабрика объектов.
/// Перед использованием, как обычно, необходимо вызвать метод Init. 
/// </summary>
[RequireComponent(typeof(ResourceManager))]
public class FactoryPrefabs : MonoBehaviour
{
    /// <summary>
    /// Средний масштаб префаба. 
    /// </summary>
    public float AverageScale {
        get { return _averageScale; }
        set {
            if (Math.Abs(value - _averageScale) > float.Epsilon)
                _averageScale = value;
        }
    }
    [SerializeField]
    private float _averageScale = 1;

    /// <summary>
    /// Средняя скорость для запускаемых объектов.
    /// </summary>
    public float AverageSpeed {
        get { return _averageSpeed; }
        set { _averageSpeed = value; }
    }
    [SerializeField]
    private float _averageSpeed = 0.1f;

    /// <summary>
    /// Актуальный для текущего уровня набор текстур, в котором
    /// левое измерение соответствует размерам фигур, а правое номерам доступных текстур  
    /// </summary>
    public Texture2D[,] Textures { set { _textures = value; } }
    private Texture2D[,] _textures;

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
        Cubic = 3  // Делить на куб     
    }

    /// <summary>
    /// Зависимость скорости от уменьшения размера
    /// </summary>
    public Characteristic DividerSpeedOfSize {
        get { return (Characteristic)_dividerSpeedOfSize; }
        set {
            _dividerSpeedOfSize = (int)value;
        }
    }
    [SerializeField]
    private int _dividerSpeedOfSize;

    private int _lastNomber = 0;

    private readonly Queue<BehaviourPrefab> _fifoPrefabs = new Queue<BehaviourPrefab>();

    /// <summary>
    /// Шаблонный префаб
    /// </summary>
    private GameObject _templatePrefab;

    private float _halfFlightWidth;
#endregion

    /// <summary>
    /// Инициализатор вместо конструктора
    /// </summary>
    /// <param name="halfFlightWidth">половина ширины полетного пространства</param>
    /// <param name="textures">Массив текстур для запускаемых префабов</param>
    public void Init(float halfFlightWidth, Texture2D[,] textures) {
        _halfFlightWidth = halfFlightWidth;
        _textures = textures;
    }

    // Use this for initialization
    void Start() {
        InitFromRemoteData();
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
        // Инициализация объектов данными из удаленного источника и отписка:
        _templatePrefab = RemoteData.RequiredAssets["DefSphere"].obj as GameObject;
    }

    /// <summary>
    /// Запускает прифабы. Отлетавшие префабы помещаются в кэш для повторного использования.
    /// </summary>
    /// <returns>возвращает компонент запущенного префаба</returns>
    public BehaviourPrefab LaunchPrefab() {
        BehaviourPrefab behaviour = null;
        if (RemoteData.DataValid) {
            if (_fifoPrefabs.Count > 0)
                behaviour = _fifoPrefabs.Dequeue();
            
            //Расчет масштаба префаба 
            float maxScale = (1 + _deltaScale / 100);
            float minScale = (1 - _deltaScale / 100);
            float differentialScale = Random.Range(minScale, maxScale);
            float scale = _averageScale * differentialScale;
            
            // Расчет нового положения
            float halfAvailableFlightWidth = _halfFlightWidth - scale / 2; //Чтобы помещались целиком
            Vector3 newTransform = transform.rotation * new Vector3(Random.Range(-halfAvailableFlightWidth,
            halfAvailableFlightWidth), BehaviourPrefab.HalfFlightLength, 0) + transform.position;
            
            if (behaviour == null) { // т.е. нужно создать новый gameObject
                GameObject prefab = Instantiate(_templatePrefab, newTransform, transform.rotation) as GameObject;
                if (prefab == null)
                    Debug.LogError("Не удалось разместить на сцене копию образцового префаба");
                behaviour = prefab.GetComponent<BehaviourPrefab>(); // Должен быть, иначе NullReferenceException
                behaviour.Id = _lastNomber;
                _lastNomber++;
            }
            else { //иначе запускаем имеющийся gameObject
                behaviour.gameObject.SetActive(true);
                behaviour.gameObject.transform.position = newTransform;
            }
            
            // Задание размера
            behaviour.gameObject.transform.parent = gameObject.transform;
            behaviour.gameObject.transform.localScale = Vector3.one * scale;

            // Задаю степенные зависимости скорости от размера. 
            behaviour.Speed = _averageSpeed / Mathf.Pow(differentialScale, (int)_dividerSpeedOfSize);
            
            // Расчет и наложение текстур и цветов на префаб
            int textureSize = (int)((scale / _averageScale - minScale) / 
                (maxScale - minScale) * _textures.GetLength(0) - 1e-6);
            Color color = _colors[Random.Range(0, _colors.Length - 1)];
            behaviour.renderer.material.color = 0.2f * color.gamma + Color.white *0.8f;
            behaviour.renderer.material.mainTexture = _textures[textureSize, 
                Random.Range(0, _textures.GetLength(1) - 1)];
            
            behaviour.Burst += BehaviourOnBurst; // По возвращении засуну в кэш, про запас
            //Material quarMaterial = transform.FindChild("QuadGameField").renderer.material;
            //quarMaterial.color = color;
            //quarMaterial.mainTexture = texture;
        }
        return behaviour;
    }

    private readonly Color[] _colors = new[]
        {
            /*Color.black,*/ Color.blue, /*Color.clear,*/ Color.cyan, Color.gray, Color.green, 
            Color.grey, Color.magenta, Color.red, Color.white, Color.yellow
        };

    private void BehaviourOnBurst(object sender, EventArgs eventArgs) {
        // Объект освободился. Отписываюсь и помещаю его в кэш.  
        BehaviourPrefab behaviour = (BehaviourPrefab)sender;
        behaviour.Burst -= BehaviourOnBurst;
        _fifoPrefabs.Enqueue(behaviour);
    }
}


