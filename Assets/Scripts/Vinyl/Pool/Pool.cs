using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T> : IEnumerable<T>
{
    private List<PoolObject<T>> _pool;

    private Func<T> _factoryMethod;
    private Action<T> _enableMethod, _disableMethod;

    public Pool(int defaultCapacity, Func<T> factoryMethod, Action<T> enableMethod, Action<T> disableMethod)
    {
        _factoryMethod = factoryMethod;
        _enableMethod = enableMethod;
        _disableMethod = disableMethod;

        _pool = new List<PoolObject<T>>(defaultCapacity);
        for (int i = 0; i < defaultCapacity; i++)
        {
            PoolObject<T> newObj = new PoolObject<T>(_factoryMethod);
            _disableMethod(newObj.Content);
            _pool.Add(newObj);
        };
    }

    public T Get()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].IsActive)
            {
                _enableMethod(_pool[i].Content);
                _pool[i].IsActive = true;
                return _pool[i].Content;
            }
        }
        PoolObject<T> newObj = new PoolObject<T>(_factoryMethod);
        _enableMethod(newObj.Content);
        newObj.IsActive = true;
        _pool.Add(newObj);
        return newObj.Content;
    }

    public T Get(Func<T, bool> condition)
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (condition(_pool[i].Content))
                return _pool[i].Content;
        }
        return default(T);
    }



    public void Return(T obj)
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (_pool[i].Content.Equals(obj))
            {
                _disableMethod(obj);
                _pool[i].IsActive = false;
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var source in _pool)
        {
            if (source.IsActive)
                yield return source.Content;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }
}
