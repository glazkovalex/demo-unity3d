using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Класс позволяет создать средствами GUI двухуровневую панель меню. 
/// Двухуровневое меню имеет подраздел "Настройки".
/// </summary>
public class MenuAndSettings : MonoBehaviour
{
    private GUISkin _skin; // Скин для кнопок основного меню 
    private GUIStyle _welcomLabel; //Стиль для ненужной надписи вверху экрана 

    private Rect playGameRect = new Rect(Screen.width / 2 - 50, Screen.height / 2 - 65, 100, 30);
    private Rect optionsRect = new Rect(Screen.width / 2 - 50, Screen.height / 2 - 15, 100, 30);
    private Rect quitRect = new Rect(Screen.width / 2 - 50, Screen.height / 2 + 35, 100, 30);

    private bool gameMode = false; // Признак активности основного (коренного) меню  
    private bool optionsMode = false; // Признак активности вложенного меню "Настройки"
    private bool gameWasStartedMode = false; // Признак того, что игра уже была запущена 
    private bool moveMode = false;    // Признак активности режима управления объектом 
    //    private bool IsMusicOn = true; // Признак включенности музыки

    public float step = 1; //Шаг с которым перемещается управляемый объект
    public float tic  = 1; //Время, за которое происходит перемещение объекта

    const string NAME_MAIN_SCENE = "GameSpace"; //Название основной сцены
    const KeyCode KEY_TO_MOVEMENT = KeyCode.F5; //Кнопка включающая управление объектом
    const KeyCode KEY_TO_MAIN_MENU = KeyCode.Escape; //Кнопка вызова основного меню

    // Use this for initialization
    void Start() {
    }

    void Update() {

    }

    void OnGUI() {
        if(!RemoteData.DataValid)
            return;
        if (Input.GetKey(KEY_TO_MAIN_MENU)) { //Если нажали кнопку вызова главного меню, то:
            gameMode = false; //активизация основного меню
            optionsMode = false;
            moveMode = false;
            Time.timeScale = 0; //Остановка игрового времени (Пауза)
        }

        if (gameMode) { // Если активен режим игры, то игровой комментарий
            if (moveMode) // Пора. Рисуем и обрабатываем кнопки...
            {
                GUI.Label(new Rect(Screen.width / 2, 0, 50, 20),
                    "Для выхода из дополнительного меню нажмите " + KEY_TO_MAIN_MENU, _welcomLabel);
            }
            else {
                if (Input.GetKey(KEY_TO_MOVEMENT)) moveMode = true;
                GUI.Label(new Rect(Screen.width / 2, 0, 50, 20),
                    "Для входа в дополнительное меню нажмите " + KEY_TO_MOVEMENT, _welcomLabel);
            }
        }
        else if (optionsMode) //Если активно меню "Настройки", то рисуем его
        {
            GUI.Label(new Rect(Screen.width / 2, 0, 50, 20), "Настройки", _welcomLabel);
            GUI.skin = _skin;
            GUI.Label(new Rect(270, 75, 50, 20), "Шаг смещения");
            step = GUI.HorizontalSlider(new Rect(50, 100, 500, 20), step, 0.1f, 10);
            GUI.Label(new Rect(560, 95, 50, 20), /*_shootDelay*/step.ToString());

            GUI.Label(new Rect(270, 125, 50, 20), "Время перемещения");
            tic = GUI.HorizontalSlider(new Rect(50, 150, 500, 20), tic, 0.1f, 5);
            GUI.Label(new Rect(560, 145, 50, 20), tic.ToString());

            if (GUI.Button(new Rect(20, 190, 100, 30), "<< Back"))
                optionsMode = false; //Завершили "настройку" 
        }
        else //Значит активно основное меню. Рисуем и обрабатываем кнопки.
        {
            GUI.Label(new Rect(Screen.width / 2, 0, 50, 20), "Здравствуйте", _welcomLabel);
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
                    //FreeMouseLook(true);
                    Time.timeScale = 1; //Нормализация игрового времени
                }
                if (GUI.Button(optionsRect, "Настройки"))
                    optionsMode = true; //активизация меню настроек
            }

            if (GUI.Button(quitRect, "Выход"))
                Application.Quit(); //Завершает приложение
        }
    }

    void Awake() {
        DontDestroyOnLoad(this); //Сохраняет целевой объект при загрузке новой сцены.
        RemoteData.AllComplete += RemoteDataOnAllComplete;
    }

    private void RemoteDataOnAllComplete(object sender, EventArgs eventArgs) {
        _skin = RemoteData.RequiredAssets["MenuSkin"].obj as GUISkin;
        _welcomLabel = _skin.GetStyle("Header");
        RemoteData.AllComplete -= RemoteDataOnAllComplete;
    }
}
