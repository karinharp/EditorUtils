using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace am
{

public class InspectorBase<T> : Editor where T : ScriptableObject
{
    
    protected Stack<Action> m_callbackQueue;
    protected bool          m_isTermPollCallbackQueue;

    protected virtual void OnEnable()
    {
	m_callbackQueue   = new Stack<Action>();
	m_isTermPollCallbackQueue = false;
    }

    protected virtual void DrawCustomInspector(){ DrawDefaultInspector(); }    
    public override void OnInspectorGUI(){ DrawCustomInspector(); }

    protected void PollCallbackQueue(){
	while(m_callbackQueue.Count > 0){ (m_callbackQueue.Pop())(); }
	if(! m_isTermPollCallbackQueue){ EditorApplication.delayCall += PollCallbackQueue; }
    }

    protected static T Search(){
	var guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).ToString());
	if (guids.Length == 0){ return null; }
	var path = AssetDatabase.GUIDToAssetPath(guids[0]);
	return AssetDatabase.LoadAssetAtPath<T>(path);	
    }
    
    /*=================================================================================================*/

    protected virtual void DrawListNode<NodeT>(NodeT node){}
    
    protected virtual void DrawList<NodeT>(T so, List<NodeT> list, string listName = "") where NodeT : new(){
	
	for(int idx = 0; idx < list.Count; ++idx){
	    var node = list[idx];
	    
	    DrawListNode(node);
	    
	    EditorGUILayout.BeginHorizontal();
	    {
		DrawSimpleLabelField(listName + " Node", "", null);
		if(GUILayout.Button("↑", EditorStyles.miniButtonLeft)){ 
		    if(idx > 0){
			list.RemoveAt(idx); 
			list.Insert(idx-1, node);
			EditorUtility.SetDirty(so);
		    }
		}
		if(GUILayout.Button("↓", EditorStyles.miniButtonMid)){ 
		    if(idx < (list.Count - 1)){
			list.RemoveAt(idx); 
			list.Insert(idx+1, node);
			EditorUtility.SetDirty(so);
		    }
		}
		if(GUILayout.Button("Remove", EditorStyles.miniButtonRight)){ 
		    list.RemoveAt(idx); 
		    --idx;
		    EditorUtility.SetDirty(so);
		}		    
	    }
	    EditorGUILayout.EndHorizontal();
	    
	    GUILayout.Space(5);	

	    EditorGUILayout.BeginHorizontal();
	    GUILayout.Space(EditorGUI.indentLevel * 15);
	    GUILayout.Box(GUIContent.none, HrStyle.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
	    EditorGUILayout.EndHorizontal();
	    GUILayout.Space(5);	    
	}

	EditorGUILayout.BeginHorizontal();
	{
	    if(listName != ""){
                DrawSimpleLabelField(listName, "", null);
	    }
	    if(GUILayout.Button("Add", EditorStyles.miniButton)){		
		list.Add(new NodeT());
		EditorUtility.SetDirty(so);
	    }
	    if(GUILayout.Button("Save", EditorStyles.miniButton)){
		m_callbackQueue.Push(() => {		
			EditorUtility.SetDirty(so);
			AssetDatabase.SaveAssets();
			EditorUtility.DisplayDialog("Inspector :: Save", "Save Success.", "OK"); 
		    });
		m_isTermPollCallbackQueue = false;
                EditorApplication.delayCall += PollCallbackQueue;
	    }
	}
	EditorGUILayout.EndHorizontal();		
    }
    
    /*=================================================================================================*/

    /*
     * @sample
     * if(flag = Foldout(flag, "label")){}
     */
    protected virtual bool Foldout(bool display, string title)
    {
	
        var style           = new GUIStyle("ShurikenModuleTitle");
        style.font          = new GUIStyle(EditorStyles.label).font;
        style.border        = new RectOffset(15, 7, 4, 4);
        style.fixedHeight   = 22;
        style.contentOffset = new Vector2(20f, -2f);

        var rect = GUILayoutUtility.GetRect(16f, 22f, style);
        GUI.Box(rect, title, style);

        var e = Event.current;

        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint){
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition)){	    
            display = !display;
            e.Use();
        }

        return display;
    }

    /*=================================================================================================*/

    protected virtual void DrawInvokeBt(Func<Task> func){

	DrawSimpleLabelField("Invoke Menu");
	EditorGUILayout.BeginHorizontal();
	{
	    if(GUILayout.Button("Invoke", EditorStyles.miniButton)){
		m_callbackQueue.Push(async () => {
			m_isTermPollCallbackQueue = true;
			await func();
			EditorUtility.DisplayDialog(func.ToString() + "::Invoke", "Complete", "OK");
		    });		
		m_isTermPollCallbackQueue = false;
		EditorApplication.delayCall += PollCallbackQueue;
	    }
	}       
	EditorGUILayout.EndHorizontal();
	GUILayout.Space(5);	
    }
    
    protected void DrawSimpleLabelField(string label, string value = "",
					GUIStyle style = null, float defaultLabelWidth = 80f)
    {
	EditorGUILayout.BeginHorizontal();
	{
	    if(style == null){ style = EditorStyles.label; }
	    EditorGUILayout.LabelField(label, style, GUILayout.Width(defaultLabelWidth));
	    if(value != ""){
		EditorGUILayout.LabelField(value);
	    }
	}
	EditorGUILayout.EndHorizontal();	
    }
    
    protected void DrawSimpleTextField(T so, string label, ref string value,
				       float defaultLabelWidth = 80f)
    {
	EditorGUILayout.BeginHorizontal();
	{
	    EditorGUILayout.LabelField(label, GUILayout.Width(defaultLabelWidth));
	    var input = EditorGUILayout.TextField(value);
	    if(input != value){
		Undo.RegisterCompleteObjectUndo(so, label + " Change");
		value = input;
		EditorUtility.SetDirty(so);
	    }
	}
	EditorGUILayout.EndHorizontal();	
    }
    
    protected void DrawSimpleIntField(T so, string label, ref int value,
				       float defaultLabelWidth = 80f)
    {
	EditorGUILayout.BeginHorizontal();
	{
	    EditorGUILayout.LabelField(label, GUILayout.Width(defaultLabelWidth));
	    var input = EditorGUILayout.IntField(value);
	    if(input != value){
		Undo.RegisterCompleteObjectUndo(so, label + " Change");
		value = input;
		EditorUtility.SetDirty(so);
	    }
	}
	EditorGUILayout.EndHorizontal();	
    }

    protected void DrawSimpleIntSlider(T so, string label, ref int value, int min, int max, 
				       float defaultLabelWidth = 80f)
    {
	EditorGUILayout.BeginHorizontal();
	{
	    EditorGUILayout.LabelField(label, GUILayout.Width(defaultLabelWidth));
	    var input = EditorGUILayout.IntSlider(value, min, max);
	    if(input != value){
		Undo.RegisterCompleteObjectUndo(so, label + " Change");
		value = input;
		EditorUtility.SetDirty(so);
	    }
	}
	EditorGUILayout.EndHorizontal();	
    }
    
    protected void DrawSimpleBoolField(T so, string label, ref bool value,
				       float defaultLabelWidth = 80f)
    {
	EditorGUILayout.BeginHorizontal();
	{
	    EditorGUILayout.LabelField(label, GUILayout.Width(defaultLabelWidth));
	    var input = EditorGUILayout.Toggle(value);
	    if(input != value){
		Undo.RegisterCompleteObjectUndo(so, label + " Change");
		value = input;
		EditorUtility.SetDirty(so);
	    }
	}
	EditorGUILayout.EndHorizontal();	
    }

    // 参考：https://twitter.com/stereoarts/status/745252926025699328
    protected void DrawSimpleEnumField<EnumT>(T so, string label, ref EnumT value,
					      float defaultLabelWidth = 80f) where EnumT : struct
    {
	EditorGUILayout.BeginHorizontal();
	{
	    EditorGUILayout.LabelField(label, GUILayout.Width(defaultLabelWidth));

	    // var input = EditorGUILayout.EnumPopup(value) as EnumT; // これはNG
	    var enumPopup = typeof(EditorGUILayout).GetMethod("EnumPopup", BindingFlags.Static | BindingFlags.Public, null, new System.Type[]{ typeof(System.Enum), typeof(GUILayoutOption[])}, null);
	    var input = (EnumT)enumPopup.Invoke(null, new object[]{ value, null });
	    if(!input.Equals(value)){
		Undo.RegisterCompleteObjectUndo(so, label + " Change");
		value = input;
		EditorUtility.SetDirty(so);
	    }
	}
	EditorGUILayout.EndHorizontal();	
    }
    
    protected void DrawSimpleObjectField<ObjT>(T so, string label, ref ObjT value,
					       float defaultLabelWidth = 80f) where ObjT : UnityEngine.Object
    {
	EditorGUILayout.BeginHorizontal();
	{
	    EditorGUILayout.LabelField(label, GUILayout.Width(defaultLabelWidth));
	    var input = EditorGUILayout.ObjectField(value, typeof(ObjT), true) as ObjT;
	    if(input != value){
		Undo.RegisterCompleteObjectUndo(so, label + " Change");
		value = input;
		EditorUtility.SetDirty(so);
	    }
	}
	EditorGUILayout.EndHorizontal();	
    }
    
}
}    

/*
 * Local variables:
 * compile-command: ""
 * End:
 */
