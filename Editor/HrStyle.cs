using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

namespace am
{

// http://answers.unity3d.com/questions/216584/horizontal-line.html
public static class HrStyle
{

    private static GUIStyle m_line = null;

    static HrStyle(){
	m_line = new GUIStyle("box");
	m_line.border.top = m_line.border.bottom = 1;
	m_line.margin.top = m_line.margin.bottom = 1;
	m_line.padding.top = m_line.padding.bottom = 1;
    }
    
    public static GUIStyle EditorLine { get { return m_line; }}

}
}

/*
 * Local variables:
 * compile-command: "make -C../"
 * End:
 */

