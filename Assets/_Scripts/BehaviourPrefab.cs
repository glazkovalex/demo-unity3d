using System;
using UnityEngine;

/// <summary>
/// Базовый класс определяющий поведение префабов на сцене.
/// Перед экземпляров класса необходимо инициализировать статическое свойство HalfFlightLength!
/// </summary>
public abstract class BehaviourPrefab : MonoBehaviour
{
    /// <summary>
    /// Идентификатор. Автоматически переименовывает gameObject, дописывая номер. 
    /// </summary>
    public virtual int Id {
        get { return _id; }
        set {
            if (value >= 0) {
                name += "#" + value.ToString();
                _id = value;
            }
        }
    }
    private int _id;

    /// <summary>
    /// Текущая скорость префаба
    /// </summary>
    public virtual float Speed { get; set; }

    /// <summary>
    /// Длинна полета
    /// </summary>
    public static float HalfFlightLength { get; set; }

    /// <summary>
    /// Префаб был так или иначе "лопнут"
    /// </summary>
    public event EventHandler Burst;

    /// <summary>
    /// Оболочка для зажигания из потомков событие префаб "лопнули".
    /// </summary>
    /// <param name="e">Аргументы события</param>
    protected virtual void OnBurst(EventArgs e) {
        if (Burst != null)
            Burst(this, e);
    }
}
