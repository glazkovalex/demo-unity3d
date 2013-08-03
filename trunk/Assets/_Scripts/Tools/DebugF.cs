using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// Debug c форматированными параметрами и групповым отключением.  
/// </summary>
public static class DebugF
{
    [Flags]
    public enum Types
    {
        None = 0,
        Message = 1,
        Warning = 2,
        Error = 4,
        NotMessage = 6,
        All = 7
    }

    public static Types EnabledTypesLogs {
        get { return _enabledTypesLogs; }
        set {
            if (value != _enabledTypesLogs) {
                if (value < Types.All && value > Types.None)
                    LogWarning("Отключены сообщения типа(ов) : {0}",
                        (Types.Message | Types.Warning | Types.Error) & ~value);
                    _enabledTypesLogs = value;
            }
        }
    }
    private static Types _enabledTypesLogs = Types.All;

    static DebugF() {
        EnabledTypesLogs = Types.All;//Types.Message | Types.Warning | Types.Error;
    }

    /// <summary>
    /// Выводит на консоль информационное сообщение
    /// </summary>
    /// <param name="message">Сообщение</param>
    public static void Log(string message) {
        if ((EnabledTypesLogs & Types.Message) == Types.Message)
            Debug.Log(string.Format(message));
    }

    /// <summary>
    /// Выводит на консоль информационное сообщение
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="pars">Массив параметров заявленных в сообщении</param>
    public static void Log(string message, params object[] pars) {
        if ((EnabledTypesLogs & Types.Message) == Types.Message)
            Debug.Log(string.Format(message, pars));
    }

    /// <summary>
    /// Выводит на консоль предупреждение /!\
    /// </summary>
    /// <param name="message">Сообщение</param>
    public static void LogWarning(string message) {
        if ((EnabledTypesLogs & Types.Warning) == Types.Warning)
            Debug.LogWarning(string.Format(message));
    }

    /// <summary>
    /// Выводит на консоль предупреждение /!\
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="pars">Массив параметров заявленных в сообщении</param>
    public static void LogWarning(string message, params object[] pars) {
        if ((EnabledTypesLogs & Types.Warning) == Types.Warning)
            Debug.LogWarning(string.Format(message, pars));
    }

    /// <summary>
    /// Выводит на консоль сообщение об ошибке (!)
    /// </summary>
    /// <param name="message">Сообщение</param>
    public static void LogError(string message) {
        if ((EnabledTypesLogs & Types.Error) == Types.Error)
            Debug.LogError(string.Format(message));
    }

    /// <summary>
    /// Выводит на консоль сообщение об ошибке (!)
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="pars">Массив параметров заявленных в сообщении</param>
    public static void LogError(string message, params object[] pars) {
        if ((EnabledTypesLogs & Types.Error) == Types.Error)
            Debug.LogError(string.Format(message, pars));
    }
}

