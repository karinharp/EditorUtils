using System;   
using UnityEngine;   

namespace am
{

public static class EnumToolbar
{

    public static Enum Draw(Enum selected)
    {
	string[] toolbar = System.Enum.GetNames(selected.GetType());
	Array values = System.Enum.GetValues(selected.GetType());

	for (int i = 0; i  < toolbar.Length; i++)
	{
	    string toolname = toolbar[i];
	    toolname = toolname.Replace("_", " ");
	    toolbar[i] = toolname;
	}

	int selected_index = 0;
	while (selected_index < values.Length)
	{
	    if (selected.ToString() == values.GetValue(selected_index).ToString())
	    {
		break;
	    }
	    selected_index++;
	}
	selected_index = GUILayout.Toolbar(selected_index, toolbar, GUILayout.ExpandWidth(true));
	return (Enum) values.GetValue(selected_index);
    }
    
}
}

/*
 * Local variables:
 * compile-command: "make"
 * End:
 */
