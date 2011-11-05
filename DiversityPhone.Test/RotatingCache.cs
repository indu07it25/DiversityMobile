﻿using System;
using System.Collections.Generic;

namespace DiversityPhone.Test
{
    

    public class RotatingCache<T> : IList<T> where T : IEquatable<T>
    {
          

        private ICacheSource<T> _source;
        private T[] _store;
        private int _lowerBoundIdx;
        private int _lowerBoundKey;        
        private int _upperBoundKey;

        public RotatingCache(int size, ICacheSource<T> source)
        {
            this._source = source;
            this._store = new T[size];
            this._lowerBoundIdx = 0;
            this._lowerBoundKey = 0;            
            this._upperBoundKey = 0;
        }

        private T getItem(int key)
        {
            if (!inRange(key))
                throw new IndexOutOfRangeException(String.Format("{0}/{1}",key,Count));
            if (!isCacheHit(key))
                fetchRangeAround(key);
            return _store[KeyToIndex(key)];
            
        }

        private bool inRange(int key)
        {
            return key >= 0 && key < Count;
        }

        private void fetchRangeAround(int idx)
        {
            int newlowerBoundKey = idx - (_store.Length / 2);
            newlowerBoundKey = (newlowerBoundKey < 0) ? 0 : newlowerBoundKey;
            int lowerKey = newlowerBoundKey;
            
            int itemCount = _store.Length;

            if (_lowerBoundKey > lowerKey && _lowerBoundKey - lowerKey < _store.Length)
                itemCount = _lowerBoundKey - lowerKey;
            else if (_upperBoundKey > lowerKey && _lowerBoundKey < lowerKey)
            {
                int overlap = _upperBoundKey - lowerKey;
                lowerKey = _upperBoundKey;
                itemCount -= overlap;
            }

            _upperBoundKey = lowerKey;
            int currentIdx = KeyToIndex(lowerKey);
            
            foreach (var item in _source.retrieveItems(itemCount, lowerKey))
            {
                _store[currentIdx] = item;
                currentIdx = ++currentIdx % _store.Length;
                _upperBoundKey++;
            }

            _lowerBoundIdx = KeyToIndex(newlowerBoundKey);
            _lowerBoundKey = newlowerBoundKey;           
        }

        private bool isCacheHit(int key)
        {
            int itemCount = _upperBoundKey - _lowerBoundKey;
            int Offset = key - _lowerBoundKey;
            return (Offset > 0 && Offset < itemCount);
        }

        private int IndexToKey(int idx)
        {

        }

        private int KeyToIndex(int key)
        {
            return ((key - _lowerBoundKey)+_lowerBoundIdx) % _store.Length;
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                return getItem(index);
            }
            set
            {
                
            }
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _source.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T item)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        public void Add(T item)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        public void Clear()
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        public void Insert(int index, T item)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        public void RemoveAt(int index)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }
    }
}
