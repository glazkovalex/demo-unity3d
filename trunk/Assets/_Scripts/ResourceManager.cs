using System;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

/// <summary>
/// Генерирует набор текстур разного размера в процессе выполнения работы основной части программы. 
/// Используемое классов время заданно в константе LIMIT_LOAD_MS. Перегенерировав набор текстур,
/// класс будет сохранять прежний набор до следующего заказа перегенерации текстур. 
/// Не нужные текстуры утилизируются естественным образом.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    private const int LIMIT_LOAD_MS = 5; // На сколько мс можно загружать каждый кадр

    private int _countEachTexture;
    private int _countSizes;
    private int _minSizeTexture;

    /// <summary>
    /// Актуальный для текущего уровня набор текстур, в котором
    /// левое измерение соответствует размерам фигур, а правое номерам доступных текстур  
    /// </summary>
    public Texture2D[,] textures;
    private Texture2D[,] reserveTextures, tempTextures;

    /// <summary>
    /// Пуст ли еще следующий набор текстур
    /// </summary>
    public bool NextSetEmpty { get { return _nextSetEmpty; } }
    private bool _nextSetEmpty = true;

    private int _countWork = 0;
    private int _countWorkToFrame = int.MaxValue;

    /// <summary>
    /// Инициализатор, вместо конструктора
    /// </summary>
    /// <param name="countSizes">Число текстур каждого типа</param>
    /// <param name="countEachTexture">Число текстур каждого типа</param>
    /// <param name="minSizeTexture">Минимальный размер текстур</param>
    public void Init(int countSizes, int countEachTexture, int minSizeTexture) {
        _countEachTexture = countEachTexture;
        _countSizes = countSizes;
        _minSizeTexture = minSizeTexture;

        // Вычисляю допустимое количество работы на кадр
        reserveTextures = new Texture2D[1,1];
        int score = TestSpeed();
        tempTextures = reserveTextures;

        reserveTextures = new Texture2D[_countSizes, _countEachTexture];
        
        // Делаю _countWorkToFrame максимален, и в блокирующей манере создаю набор текстур для первого уровня
        StartCoroutine(UpdateTetures(_countSizes, _countEachTexture, _minSizeTexture));

        _countWorkToFrame = score * LIMIT_LOAD_MS; //и только теперь задаю пяти миллисекундное количество работы на кадр

        ChangeSets(); // Быстро созданный массив делаю основным
    }

    /// <summary>
    /// Заменяет имеющиеся наборы текстур в массиве Textures на свежесгенерированные
    /// </summary>
    public void ChangeSets() {
        // Освободить старые ресурсы
        if (_nextSetEmpty) {
            Debug.Log("Заказано обновление текстур когда они еще не готовы! Стоит проверять \"NextSetReady\"");
            return;
        }
        //Stopwatch stopwatch = Stopwatch.StartNew();
        // Освобождаю ресурсы от еще в прошлый раз устаревших текстур
        if (tempTextures != null) {
            int nSets = tempTextures.GetLength(1);
            for (int i = 0; i < tempTextures.GetLength(0); i++) {
                for (int j = 0; j < nSets; j++) {
                    DestroyImmediate(tempTextures[i, j], true);
                }
            }
        }
        //Resources.UnloadUnusedAssets();
        // сохраняю устаревшие текстуры, чтобы объекты на сцена не испортить.  
        tempTextures = textures;
        // копирую новые
        _nextSetEmpty = true;
        textures = reserveTextures;
        reserveTextures = new Texture2D[_countSizes, _countEachTexture];
        //DebugF.Log("Старый набор текстур был удален и заменен резервным за {0}мс", stopwatch.ElapsedMilliseconds);

        // Запускаю расчет резервного набора текстур
        StartCoroutine(UpdateTetures(_countSizes, _countEachTexture, _minSizeTexture));
    }

    /// <summary>
    /// Создает новый набор текстур и освобождает ресурсы занятые прежним набором
    /// </summary>
    /// <param name="countSizes">Число текстур каждого типа</param>
    /// <param name="countEachTexture">Число текстур каждого типа</param>
    /// <param name="minSizeTexture">Минимальный размер текстур</param>
    /// <param name="benchmarkMode">запустить в диагностическом режиме</param>
    /// <returns>для размазывания нагрузки по кадрам вызывать через StartCoroutine(...)</returns>
    private IEnumerator UpdateTetures(int countSizes, int countEachTexture, int minSizeTexture, bool benchmarkMode = false) {
        //Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < countSizes; i++) {
            int size = (int)(minSizeTexture * Math.Pow(2, i));
            float minDivider = size / 16.0f; // Эти коэффициенты определяют разброс
            float maxDivider = size / 8.0f;  // частоты повтора рисунка на текстуре
            for (int j = 0; j < countEachTexture; j++) {
                reserveTextures[i, j] = new Texture2D(size, size);

                // Вычисление первого градиента
                float divider = Random.Range(minDivider, maxDivider);
                float[,] gradients = new float[size, 2];
                for (int n = 0; n < size; n++) {
                    //gradients[i, 0] = Mathf.Abs(Mathf.Sin(((float)i) / divider1)) * 0.8f;
                    gradients[n, 1] = Mathf.Abs(Mathf.Sin(((float)n) / divider)) * 0.8f;
                }
                _countWork += size / 2; //Учет нагрузки вычисления градиента

                // Расчет цветов пикселей совмещенный с расчетом второго градиента
                divider = Random.Range(minDivider, maxDivider); // 
                Color color1 = _colors[Random.Range(0, _colors.Length - 1)];
                Color color2 = _colors[Random.Range(0, _colors.Length - 1)];
                for (int x = 0; x < reserveTextures[i, j].width; x++) {
                    gradients[x, 0] = Mathf.Abs(Mathf.Sin(((float)x) / divider)) * 0.8f;
                    for (int n = 0; n < reserveTextures[i, j].height / minSizeTexture; n++) {
                        if (_countWork > _countWorkToFrame) { //Проверка не пора ли передохнуть до следующего кадра
                            _countWork = 0;
                            yield return null; // остановка выполнения функции до следующего кадра
                        }
                        _countWork += minSizeTexture; // Продолжение полезной работы:
                        for (int y = n * minSizeTexture; y < (n + 1) * minSizeTexture; y++) {
                            reserveTextures[i, j].SetPixel(x, y, color1 * gradients[x, 0] + color2 * gradients[y, 1]);
                        }
                    }
                }
                reserveTextures[i, j].Apply();
            }
        }
        _nextSetEmpty = benchmarkMode; // При benchmarkMode портится полезный "reserveTextures"
        //DebugF.Log("Новый набор текстур создан за : {0}мс", stopwatch.ElapsedMilliseconds);
    }

    private int TestSpeed() {
        _countWorkToFrame = int.MaxValue;
        const float size = 256.0f;
        Stopwatch stopwatch = Stopwatch.StartNew();
        StartCoroutine(UpdateTetures(1, 1, (int)size, true));
        float res = stopwatch.ElapsedMilliseconds; //Замеряю время создания одной текстуры 1024x1024
        float pixelToOneMs = size * size / res;
        //DebugF.Log("Получил x{0}. Скорость миллисекунд на пиксель : {1} для загрузки 1мс на кадр _countWorkToFrame = {2}",
        //    reserveTextures[0, 0].width, 1 / pixelToOneMs, pixelToOneMs);
        return (int)pixelToOneMs;
    }

    private readonly Color[] _colors = new[]
        {
            /*Color.black,*/ Color.blue, /*Color.clear,*/ Color.cyan, Color.gray, Color.green, 
            Color.grey, Color.magenta, Color.red, Color.white, Color.yellow
        };

}
