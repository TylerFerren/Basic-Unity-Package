using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Codesign.Collections
{
    public class Category : ObjectAttributes
    {
        private Category parent;
        public Category Partent { get { return parent; } }

    }
}