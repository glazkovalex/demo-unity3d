using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Меню.
/// </summary>
public class MenuAndSettings : MonoBehaviour
{
    private GUISkin _skin; // Скин для кнопок основного меню 
    private GUIStyle _welcomLabel; //Стиль для ненужной надписи вверху экрана 

    private Rect playGameRect = new Rect(Screen.width / 2 - 50, Screen.height / 2 - 65, 100, 30);
    private Rect quitRect = new Rect(Screen.width / 2 - 50, Screen.height / 2 + 35, 100, 30);

    private bool gameMode = false; // Признак активности основного (коренного) меню  
    private bool gameWasStartedMode = false; // Признак того, что игра уже была запущена 

    public float step = 1; //Шаг с которым перемещается управляемый объект
    public float tic  = 1; //Время, за которое происходит перемещение объекта

    const string NAME_MAIN_SCENE = "GameSpace"; //Название основной сцены
    const KeyCode KEY_TO_MAIN_MENU = KeyCode.Escape; //Кнопка вызова основного меню

    void Start() {
        InitFromRemoteData();
    }

    void InitFromRemoteData() {
        if (!RemoteData.DataValid) { // Подождать загрузки данных
            RemoteData.AllComplete += InitFromRemoteData;
            return;
        }
        //Иначе данные уже загрузить, можно отписаться и проинициализароваться 
        RemoteData.AllComplete -= InitFromRemoteData;
        // Инициализация объектов данными из удаленного источника и отписка:
        _skin = RemoteData.RequiredAssets["MenuSkin"].obj as GUISkin;
        _welcomLabel = _skin.GetStyle("Header");
    }

    void OnGUI() {
        if(!RemoteData.DataValid)
            return;
        if (Input.GetKey(KEY_TO_MAIN_MENU)) { //Если нажали кнопку вызова главного меню, то:
            gameMode = false; //активизация основного меню
            Time.timeScale = 0; //Остановка игрового времени (Пауза)
        }

        if (gameMode) { // Если активен режим игры, то игровой комментарий
        }
        else //Значит активно основное меню. Рисуем и обрабатываем кнопки.
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, 30, 200, 20), "Основное меню", _welcomLabel);
            GUI.skin = _skin;

            if (!gameWasStartedMode) //Если еще НЕ была запущена, то запускаем
            {
                if (GUI.Button(playGameRect, "Начать игру")) {
                    gameMode = true;
                    gameWasStartedMode = true;
                    Application.LoadLevel(NAME_MAIN_SCENE); //Загружает и запускает сцену
                }
            }
            else //Игра уже запускалась, продолжаем ее
            {
                if (GUI.Button(playGameRect, "Продолжить")) {
                    gameMode = true;
                    Time.timeScale = 1; //Нормализация игрового времени
                }
            }

            if (GUI.Button(quitRect, "Выход"))
                Application.Quit(); //Завершает приложение
        }
    }

    void Awake() {
        DontDestroyOnLoad(this); //Сохраняет целевой объект при загрузке новой сцены.
    }
}
