using System;
using UnityEngine;

namespace CoreSystem.Attributes
{
    /// <summary>
    /// Attribute to show a dropdown for [SerializeReference] fields in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SubclassSelectorAttribute : PropertyAttribute
    {
    }
}
