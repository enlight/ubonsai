#region License

/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2014 Vadim Macagon
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#endregion License

using System.Collections.Generic;
using UnityEngine;

namespace UBonsai.Editor
{
    public class GUISkinManager
    {
        private static GUISkinManager Instance
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new GUISkinManager();
                }
                return _singleton;
            }
        }

        public static GUISkin Skin
        {
            get
            {
                if (Instance._skin == null)
                {
                    Instance._skin = Resources.LoadAssetAtPath<GUISkin>(
                        "Assets/ubonsai/Editor/Skin/Monochrome.guiskin"
                    );
                }
                return Instance._skin;
            }
        }

        public static GUIStyle NodeLabelStyle
        {
            get
            {
                if (Instance._nodeLabelStyle == null)
                {
                    Instance._nodeLabelStyle = Skin.GetStyle("nodeLabel");
                }
                return Instance._nodeLabelStyle;
            }
        }

        public static GUIStyle NodeWindowNormalStyle
        {
            get
            {
                if (Instance._nodeWindowNormalStyle == null)
                {
                    Instance._nodeWindowNormalStyle = Skin.GetStyle("nodeWindowNormal");
                }
                return Instance._nodeWindowNormalStyle;
            }
        }

        public static GUIStyle NodeWindowSelectedStyle
        {
            get
            {
                if (Instance._nodeWindowSelectedStyle == null)
                {
                    Instance._nodeWindowSelectedStyle = Skin.GetStyle("nodeWindowSelected");
                }
                return Instance._nodeWindowSelectedStyle;
            }
        }

        public static Texture2D GetNodeIcon(System.Type nodeType)
        {
            Texture2D nodeIcon = null;
            if (!Instance._nodeIcons.TryGetValue(nodeType, out nodeIcon))
            {
                var iconPath = _nodeIconPath + nodeType.Name + ".png";
                nodeIcon = Resources.LoadAssetAtPath<Texture2D>(iconPath);
                Instance._nodeIcons[nodeType] = nodeIcon;
            }
            return nodeIcon;
        }

        private static GUISkinManager _singleton;
        private const string _nodeIconPath = "Assets/ubonsai/Editor/Skin/Icons/64x64/";

        private GUISkin _skin;
        private GUIStyle _nodeLabelStyle;
        private GUIStyle _nodeWindowNormalStyle;
        private GUIStyle _nodeWindowSelectedStyle;
        private Dictionary<System.Type, Texture2D> _nodeIcons;

        private GUISkinManager()
        {
            _nodeIcons = new Dictionary<System.Type, Texture2D>();
        }
    }
}