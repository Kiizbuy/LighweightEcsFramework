using System;
using System.Collections.Generic;
using Components;
using EcsCore.Components;

namespace EcsCore
{
    public class MyCollection<TElement, TIndex> : List<TElement>
    {
        private Func<TElement, TIndex> _indexMapping { get; set; }

        public MyCollection(Func<TElement, TIndex> indexMapping)
        {
            _indexMapping = indexMapping;
        }

        public TElement this[TIndex index]
        {
            get
            {
                return Find(t => _indexMapping.Invoke(t).Equals(index));
            }
        }
    }

    public class Manda
    {
        public MyCollection<int, string> priv;

        void Pzdc()
        {
            var p = priv["manda"];
            priv.Add(0);
            priv.Add(1);
            priv.Add(2);
            priv.Add(3);
            priv.Add(4);
        }
    }
}