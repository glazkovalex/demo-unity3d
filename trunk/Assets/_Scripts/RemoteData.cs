using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

/// <summary>
/// Загружает внешние данные и добавляет их в Assets проекта.
/// Загружаемые ресурсы должны иметь уникальные имена в части основной части имени файла, той что не расширение!
/// В расширении должно быть указано название типа сохраненных в файле данных.
/// Компоненты нуждающиеся в загружаемых ресурсах могут использовать их только после завершения их загрузки.
/// Для инициализации правильными данными нужно на Awake проверить DataValid
/// (мог быть взведен в предыдущей сцене), если не false, то подписываться на AllComplete
/// и обработчике инициализировать свои поля заведомо правильными данными из этого класса.
/// </summary>
public class RemoteData : MonoBehaviour
{
    public const string IMAGES_LOCAL_PATH = @"\AssetBundles\";
    //private const string EXTENSION = ".unity3d";

    //public Texture2D Texture {
    //    get {
    //        if (!_cashTextures.ContainsKey(_MD5)) {
    //            if (_loadByDefault) { //Если грузить по умолчанию, то defId для этой картинки уже не обрабатывается!
    //                if (Resources.Load("DefImages/" + _aseetName, typeof(Texture2D)) != null) {
    //                    //Debug.Log("Загружаемая по умолчанию картинка с названием assets'a:" + _aseetName + " присутствует!");
    //                    _cashTextures.Add(_MD5, Resources.Load("DefImages/" + _aseetName, typeof(Texture2D)) as Texture2D);
    //                }
    //                else {
    //                    Debug.LogWarning("Загружаемой по умолчанию картинки с названием:" + _aseetName + ", id:" + _id +
    //                        ", MD5:" + _MD5 + " нет среди локальных Assets! Ей назначена пустая текстура 1х1 пиксель");
    //                    _cashTextures.Add(_MD5, new Texture2D(1, 1, TextureFormat.RGB24, false));
    //                }
    //            }
    //            else { //Если не нужно грузить по умолчанию
    //                if (images.ContainsKey(_defId) && images[_defId].Texture != null)
    //                    _cashTextures.Add(_MD5, images[_defId].Texture);
    //                else
    //                    _cashTextures.Add(_MD5, new Texture2D(1, 1, TextureFormat.RGB24, false));
    //                //Debug.Log("Зазружаем из сети, т.к. в справочнике нет текстуры с _MD5:" + _MD5 + ", для картинки c id:" + _id + ", и именем:" + _name);
    //                HLWWWPool.Download(_id, _URL, OnDownloadComplete);
    //                //Debug.Log("Запросили текстуру с id:" + _id.ToHexString() + " и именем:" + _name + ", которая лежит по адресу:" + _URL);                        
    //            }
    //        }
    //        //else Debug.Log("В кеши нашлась нужная текстура с MD5:" + _MD5 + ", для картинки c id:" + _id + ", и именем:" + _name);
    //        return _cashTextures[_MD5];
    //    }
    //}

    private Dictionary<string, Type> _toLoad = new Dictionary<string, Type>();

    /// <summary>
    /// Класс содержащий данные внешнего asset  и тип
    /// </summary>
    public class AssetObject
    {
        public Object obj;
        public Type type; // Для начальной инициализации
    }

    /// <summary>
    /// Загрузка всех ресурсов завершена
    /// </summary>
    public static bool DataValid {
        get { return _dataValid; }
        set { _dataValid = value; }
    }
    private static bool _dataValid = false;

    public static Dictionary<string, AssetObject> RequiredAssets {
        get { return _requiredAssets; }
        set { _requiredAssets = value; }
    }
    private static Dictionary<string, AssetObject> _requiredAssets;

    public static event EventHandler AllComplete;

    void Awake() {
        if (_requiredAssets != null) return; // Чтобы запускался единожды, несмотря на несколько компонентов
        _requiredAssets = new Dictionary<string, AssetObject>();
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
                Type type = Types.GetType("UnityEngine." + extension, "UnityEngine"); // Тут возможен ArgumentException
                if (type == null)
                    DebugF.LogError("Загрузка ресурсов не возможна, т.к. ресурсы типа {0} не поддерживаются", extension);
                extensionAndType.Add(extension, type);
            }
            _toLoad.Add(fileName.Remove(lastDot), extensionAndType[extension]);
            //DebugF.Log("Имя : {0}, а тип : {1}", fileName.Remove(lastDot), extensionAndType[extension]);
        }
        //Type withoutDefaultConstructor = typeof(Texture2D);
        //Object newObj;
        //foreach (var asset in _toLoad) {
            //if (asset.Value == withoutDefaultConstructor) // Из-за отсутствия конструктора по умолчанию. 
            //    newObj = new Texture2D(1, 1);
            //else // Для остальных типов создание заглушек универсально
            //    newObj = Activator.CreateInstance(asset.Value) as Object;

            //_requiredAssets.Add(asset.Key, new AssetObject { obj = null/*newObj*/, type = asset.Value });

            //if (_requiredAssets[asset.Key].obj == null)
            //    DebugF.Log("Создать объект {0} не получилось :(", asset.Key);
        //}
    }

    // Use this for initialization
    IEnumerator Start() {
        if (_requiredAssets.Count == 0) { // Чтобы запускался единожды, несмотря на несколько компонентов
            string path = "file://" + (Environment.CurrentDirectory + IMAGES_LOCAL_PATH).Replace('\\', '/');
            //foreach (KeyValuePair<string, Object> item in _requiredAssets) {
            //string url =  + "Checker.Black-White.Texture2D";
            //Environment.CurrentDirectory.Replace('\\', '/') + "/AssetBundles/Checker.Black-White.png";
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
                        //Debug.Log("Получили текстуру с MD5:" + images[id].MD5 + ", c id:" + id.ToHexString() + " и именем:" + HLImages.images[id].Name + ", которая лежит по адресу:" + HLImages.images[id].URL);
                        //_bundle.Unload(false);                 
                        //_requiredAssets[item.Key] = www.assetBundle.LoadAsync(item.Key, typeof(Texture2D)).asset;
                        //Instantiate(www.assetBundle.mainAsset);
                        //if (asset.Value == withoutDefaultConstructor) {// Подменяю данные в текстуре  
                        //    Texture2D t = www.assetBundle.Load(asset.Key, asset.Value) as Texture2D;
                        //    Texture2D exist = _requiredAssets[asset.Key].obj as Texture2D;
                        //    DebugF.Log("Результат загрузки текстуры : " + exist.LoadImage(t.EncodeToPNG()));
                        //}
                        //else // Для остальных типов просто заменяю объект
                        _requiredAssets.Add(asset.Key, new AssetObject
                            {
                                obj = www.assetBundle.LoadAsync(asset.Key, asset.Value).asset, 
                                type = asset.Value
                            });
                        //DebugF.Log("Вроде бы получилось загрузить");
                        www.assetBundle.Unload(false);

                        //DebugF.Log("В загруженном бандле следующие ассеты :");
                        //foreach (var asset in www.assetBundle.LoadAll()) {
                        //    DebugF.Log(asset.name);
                        //}
                        //LoadAsync(item.Key, typeof(Texture2D)).asset)
                        //string assetName = www.assetBundle.mainAsset.name;//"Checker.Black-White";
                        //if (Resources.Load("LoadedResources/" + assetName, typeof(Texture2D)) != null) {
                        //    Debug.Log("Картинка с названием asset'a:" + assetName + " загружена!");
                        //    //_cashTextures.Add(_MD5, Resources.Load("DefImages/"+ assetName, typeof(Texture2D)) as Texture2D);
                        //    //DebugF.Log("Вроде бы получилось загрузить");
                        //    www.assetBundle.Unload(false);
                        //}
                        //DebugF.Log("Не удалось загрузить ресурс : " + assetName);
                        //renderer.material.mainTexture = www.texture;
                    }
                }
            }
            _dataValid = true;
            if (AllComplete != null)
                AllComplete(this, new EventArgs());
            //renderer.material.mainTexture = _requiredAssets["Checker.Black-White"].obj as Texture2D;
        }
    }
}
