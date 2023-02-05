using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mati36.VinylExtensions
{
    static public class VinylExtensions
    {
        //STRING
        static public string Colored(this string text, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";

        }

        static public string Sized(this string text, int fontSize)
        {
            return $"<size={fontSize}>{text}</size>";
        }

        static public string Bold(this string text)
        {
            return $"<b>{text}</b>";
        }
    }
}
