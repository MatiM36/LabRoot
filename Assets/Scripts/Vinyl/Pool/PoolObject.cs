using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject<T>
{
    public bool IsActive { get; set; }

    private T _obj;
    public T Content { get { return _obj; } }

    public PoolObject(Func<T> factoryMethod)
    {
        _obj = factoryMethod();
        IsActive = false;
    }

}
