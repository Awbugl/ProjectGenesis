using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGenesis.Patches.UI.QTools
{
    internal class ObjectPool<T> where T : Component
    {
        private readonly Func<T> _ctor;

        private readonly List<T> _pointer = new List<T>();

        private int _curReuseIndex;

        internal ObjectPool(Func<T> ctor) => _ctor = ctor;

        public T Alloc()
        {
            T component;

            if (_curReuseIndex < _pointer.Count) { component = _pointer[_curReuseIndex]; }
            else
            {
                component = _ctor();
                _pointer.Add(component);
            }

            _curReuseIndex++;

            component.gameObject.SetActive(true);

            return component;
        }

        public void RecycleAll()
        {
            _pointer.ForEach(i => i.gameObject.SetActive(false));
            _curReuseIndex = 0;
        }
    }
}
