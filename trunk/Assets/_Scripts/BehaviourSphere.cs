using System;
using UnityEngine;

public class BehaviourSphere : BehaviourPrefab
{
    //// Use this for initialization
    //void Start() {
    //    InitFromRemoteData();
    //}

    ///// <summary>
    ///// Инициализация локальных объектов данными из удаленного источника
    ///// </summary>
    //void InitFromRemoteData() {
    //    if (!RemoteData.DataValid) { // Подождать загрузки данных
    //        RemoteData.AllComplete += InitFromRemoteData;
    //        return;
    //    }
    //    //Иначе данные уже загрузить, можно отписаться и проинициализароваться 
    //    RemoteData.AllComplete -= InitFromRemoteData;
    //    // Инициализация объектов данными из удаленного источника и отписка:
    //    //renderer.material.mainTexture = RemoteData.RequiredAssets["Checker.Black-White"].obj as Texture2D;
    //}

    // Update is called once per frame
    private void Update() {
        // Перемещаю объект до дна игрового поля
        if (transform.localPosition.y > - HalfFlightLength) {
            transform.Translate(0, -Time.deltaTime * Speed, 0, Space.Self);
        }
        else {
            Speed = 0;
            OnBurst(new EventArgs()); // Оповещаю о лопании стоячего, без очков.
            gameObject.SetActive(false);
        }
    }

    void OnMouseDown() {
        OnBurst(new EventArgs()); // Оповещаю о лопании на лету, с очками.
        gameObject.SetActive(false);
    }
}