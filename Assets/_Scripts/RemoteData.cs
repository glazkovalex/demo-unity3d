using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

/// <summary>
/// Загружает внешние Assets, в виде AssetBundle, и добавляет их в статический справочник.
/// Загружаемые ресурсы должны иметь уникальные имена в части основной части имени файла, той что не расширение!
/// В расширении должно быть указано название типа сохраненных в файле данных.
/// Компоненты нуждающиеся в загружаемых ресурсах должны пытаться их использовать только
/// после завершения их загрузки, что можно установить по свойству "DataValid" и по событию AllComplete.
/// Для инициализации правильными данными в MonoBehavior добавить следующий код:
/// </summary>
/// <example><code>
/// void Start() {\n\r
///    InitFromRemoteData();
///    ...
/// }
///
/// void InitFromRemoteData() {
///    if (!RemoteData.DataValid) { // Подождать загрузки данных
///        RemoteData.AllComplete += InitFromRemoteData;
///        return;
///    }
///    //Иначе данные уже загрузились, можно отписаться и проинициализароваться 
///    RemoteData.AllComplete -= InitFromRemoteData;
///    // Инициализация объектов данными из удаленного источника и отписка:
///    this....
/// }
/// </code></example>
public class RemoteData : MonoBehaviour
{
    public const string IMAGES_LOCAL_PATH = @"\AssetBundles\";

    private Dictionary<string, Type> _toLoad = new Dictionary<string, Type>();

    /// <summary>
    /// Класс содержащий данные внешнего asset и их тип
    /// </summary>
    public class AssetObject
    {
        public Object obj;
        public Type type; // Для начальной инициализации
    }

    /// <summary>
    /// Завершена ли загрузка всех ресурсов
    /// </summary>
    public static bool DataValid {
        get { return _dataValid; }
    }
    private static bool _dataValid = false;

    /// <summary>
    /// Справочник загруженных Assets
    /// </summary>
    public static Dictionary<string, AssetObject> RequiredAssets {
        get { return _requiredAssets; }
        set { _requiredAssets = value; }
    }
    private static Dictionary<string, AssetObject> _requiredAssets = new Dictionary<string, AssetObject>();

    /// <summary>
    /// Все данные загружены, можно использовать.  
    /// </summary>
    public static event Action AllComplete;

    private bool _awaked = false;
    void Awake() {
        if (_awaked) return; // Чтобы запускался единожды, несмотря на несколько копий компонентов
        _awaked = true;
        // Выясняю состав и тип ресурсов, которые необходимо подгрузить  
        string localPath = Environment.CurrentDirectory + IMAGES_LOCAL_PATH;
        Dictionary<string, Type> extensionAndType = new Dictionary<string, Type>();
        string fileName, extension;
        // Парсю имена файлов
        foreach (string file in Directory.GetFiles(localPath)) {
            fileName = file.Substring(localPath.Length);
            int lastDot = fileName.LastIndexOf('.');
            extension = fileName.Substring(lastDot + 1);
            if (!extensionAndType.ContainsKey(extension)) {
                // Проверяю есть ли в пространстве имен UnityEngine тип одноименный расширению
                Type type = Types.GetType("UnityEngine." + extension, "UnityEngine");
                // Тут возможен ArgumentException
                if (type == null)
                    DebugF.LogError("Загрузка ресурсов не возможна, т.к. ресурсы типа {0} не поддерживаются",
                                    extension);
                extensionAndType.Add(extension, type);
            }
            _toLoad.Add(fileName.Remove(lastDot), extensionAndType[extension]);
            //DebugF.Log("Имя : {0}, а тип : {1}", fileName.Remove(lastDot), extensionAndType[extension]);
        }
    }

    private static bool _started = false;
    // Use this for initialization
    IEnumerator Start() {
        if (!_started) { // Чтобы запускался единожды, несмотря на несколько копий компонентов
            _started = true;
            string path = "file://" + (Environment.CurrentDirectory + IMAGES_LOCAL_PATH).Replace('\\', '/');
            foreach (KeyValuePair<string, Type> asset in _toLoad) {
                //DebugF.Log("Пытаюсь прочитать : " + path + asset.Key + '.' + asset.Value.Name);
                using (WWW www = new WWW(path + asset.Key + '.' + asset.Value.Name)) {
                    // Ожидание завершения загрузки
                    yield return www;
                    // Разбандленование
                    if (www.error != null) {
                        Debug.LogWarning("Ошибка обработки URL: " + www.error);
                        throw new Exception("WWW download had an error:" + www.error);
                    }
                    else {
                        _requiredAssets.Add(asset.Key, new AssetObject
                            {
                                obj = www.assetBundle.LoadAsync(asset.Key, asset.Value).asset,
                                type = asset.Value
                            });
                        www.assetBundle.Unload(false);
                    }
                }
            }
            _dataValid = true;
            if (AllComplete != null)
                AllComplete();
            //Debug.Log("Загрузка завершена");
        }
    }
}
