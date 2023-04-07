﻿using Interfaz.Models.Worlds;
using Interfaz.Utilities;
using System.Drawing;
using System.Text.Json;

namespace Interfaz.Models.Auxiliary
{
    [Serializable]
    public class Pares<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Pares(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        #region ForEach Compatibility
        /*public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();*/
        #endregion
    }
}
