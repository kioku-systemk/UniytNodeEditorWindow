using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class NodeEditorWindow : EditorWindow {

	// Colors
	private static Color s_lineColor       = new Color(0.2f,0.2f,0.2f,1.0f);
	private static Color s_nodeBaseColor   = new Color(0.4f,0.4f,0.4f,1.0f);
	private static Color s_outputPlugColor = new Color(0.2f,0.2f,0.2f,1.0f);
	private static Color s_inputPlugColor  = new Color(0.2f,0.2f,0.2f,1.0f);
	private static Color s_activeLineColor = new Color(1.0f,0.0f,0.0f,1.0f);
	private static Color s_nodeTextColor   = new Color(1.0f,1.0f,1.0f,1.0f);
	private static Color s_outputTextColor = new Color(1.0f,1.0f,1.0f,1.0f);
	private static Color s_inputTextColor  = new Color(1.0f,1.0f,1.0f,1.0f);

	private static float s_lineWidth = 2.0f;

	private static int s_nodeWidth = 100;

	private static GUIStyle s_boxNodeStyle = null;
	private static GUIStyle s_boxOutputPlugStyle = null;
	private static GUIStyle s_boxInputPlugStyle = null;
	private static GUIStyle s_outputTextStyle = null;
	private static GUIStyle s_inputTextStyle = null;

	private List<Node> m_nodes = new List<Node>();
	private OutputPlug m_activePlug = null;
	
	public NodeEditorWindow() {
	}
	~NodeEditorWindow() {
	}


	private Texture2D MakeTex(int width, int height, Color col)
	{
		int w = width;
		int h = height;
		Color[] pix = new Color[w * h];
		for (int i = 0; i < pix.Length; ++i)
			pix[i] = col;
		Texture2D result = new Texture2D(w, h);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}
	
	public void AddNode(Node node) {
		m_nodes.Add(node);
	}

	public Node AddNode(string name) {
		Node node = new Node(this, 0, 0, name);
		AddNode(node);
		return node;
	}

	public void RemoveNode(Node node) {
		m_nodes.Remove(node);
	}

	public void ClearNodes() {
		m_nodes.Clear();
	}

	public OutputPlug GetActiveOutputPlug() {
		return m_activePlug;
	}
	public void SetActiveOutputPlug(OutputPlug plug) {
		m_activePlug = plug;
	}
	
	public class InputPlug {
		private Rect m_rect;	
		private Node m_parent;
		private bool m_mdown = false;
		private string m_name;
		private OutputPlug m_srcplug = null;
		
		public InputPlug(Node parent, float x, float y, string name) {
			m_parent = parent;
			m_rect = new Rect(x, y, 10, 10);
			m_name = name;
		}
		public void Layout() {
			GUI.Box(m_rect, "", s_boxInputPlugStyle);
			GUI.Label(new Rect(m_rect.x + 10, m_rect.y - 3, s_nodeWidth, 20), m_name, s_inputTextStyle);
		}
		
		public bool Test(Vector2 p) {
			return m_rect.Contains(p);
		}
		public Node GetParent() {
			return m_parent;
		}
		public string GetName() {
			return m_name;
		}
		public Vector2 GetPos() {
			return new Vector2(m_rect.x + m_parent.m_rect.x + m_rect.width / 2,
				m_rect.y + m_parent.m_rect.y + m_rect.height / 2);
		}
		public void SetSrcPlug(OutputPlug plug) {
			m_srcplug = plug;
		}
		public bool OnMouseDown(Vector2 p) {
			if (Test(p)) {
				m_mdown = true;
				if (m_srcplug != null) {
					m_parent.GetEditorWindow().SetActiveOutputPlug(m_srcplug); // Re active
					m_srcplug.RemoveTarget(this); // disconnect
				}
				return true;
			}
			return false;
		}
		public bool OnMouseUp(Vector2 p) {
			if (m_mdown) {
				m_mdown = false;
			}
			if (Test(p)) {
				OutputPlug oplug = m_parent.GetEditorWindow().GetActiveOutputPlug();
				if (oplug != null) {
					if (m_srcplug != null) {
						m_srcplug.RemoveTarget(this); // disconnect
					}
					oplug.AddTarget(this); // Connect
				}

				return true;
			} else {
				return false;
			}
		}

		public void OnMouseDrag(Vector2 p) {
		}
	}

	// ---- Draw line function -----
	public static Texture2D lineTex;    
	static void drawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
	    if (!lineTex) { lineTex = new Texture2D(1, 1); }
	    Matrix4x4 savedmatrix = GUI.matrix;
    	GUI.matrix = Matrix4x4.identity;
	    Color savedColor = GUI.color;
        GUI.color = color;
        float angle = Vector3.Angle(pointB - pointA, Vector2.right);
	   	if (pointA.y > pointB.y) { angle = -angle; }
	   	float len = (pointB - pointA).magnitude;
	   	if (len > 0.0) {
		    GUIUtility.ScaleAroundPivot(new Vector2(len, width), new Vector2(pointA.x, pointA.y + 0.5f));
		    GUIUtility.RotateAroundPivot(angle, pointA);
		    GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTex);
		}
	    GUI.matrix = savedmatrix;
        GUI.color = savedColor;
    }
	//------------------------------

	public class OutputPlug {
		private Rect m_rect;
		private Node m_parent;
		private bool m_mdown = false;
		private string m_name;
		private List<InputPlug> m_targetPlugs = new List<InputPlug>();
		
		public OutputPlug(Node parent, float x, float y, string name) {
			m_parent = parent;
			m_rect = new Rect(x, y, 10, 10);
			m_name = name;
		}
		public void Layout() {
			GUI.Box(m_rect, "", s_boxOutputPlugStyle);
			int tsize  = m_name.Length * 12;
			GUI.Label(new Rect(m_rect.x  - tsize, m_rect.y - 3, tsize, 20), m_name, s_outputTextStyle);
		}
		public void DrawLines() {
			Vector2 startp = GetPos();
			for (int i = 0; i < m_targetPlugs.Count; ++i) {
				Vector2 endp = m_targetPlugs[i].GetPos();
				drawLine(startp, endp, s_lineColor, s_lineWidth);
			}
		}
		public bool AddTarget(InputPlug plug) {
			if (plug.GetParent() != m_parent) { // not same node
				m_targetPlugs.Add(plug);
				plug.SetSrcPlug(this);
				return true;
			}
			return false;
		}
		public void RemoveTarget(InputPlug plug) {
			m_targetPlugs.Remove(plug);
			plug.SetSrcPlug(null);
		}
		
		public Node GetParent() {
			return m_parent;
		}
		public string GetName() {
			return m_name;
		}
		public Vector2 GetPos() {
			return new Vector2(m_rect.x + m_parent.m_rect.x + m_rect.width / 2,
				m_rect.y + m_parent.m_rect.y + m_rect.height / 2);
		}
		
		public bool Test(Vector2 p) {
			return m_rect.Contains(p);
		}
		public bool OnMouseDown(Vector2 p) {
			if (Test(p)) {
				m_parent.GetEditorWindow().SetActiveOutputPlug(this);

				m_mdown = true;
				return true;
			}
			return false;
		}
		public bool OnMouseUp(Vector2 p) {
			if (m_mdown) {
				m_mdown = false;
			}
			if (Test(p))
				return true;
			else
				return false;
		}

		public void OnMouseDrag(Vector2 p) {
		}
	}

	public class Node {
		public Rect m_rect;
		private bool m_mdown = false;
		private string m_name;
		public NodeEditorWindow m_win;
		
		private List<InputPlug> m_inplugs = new List<InputPlug>();
		private List<OutputPlug> m_outplugs = new List<OutputPlug>();
		
		public NodeEditorWindow GetEditorWindow() {
			return m_win;
		}
		public Node(NodeEditorWindow win, float x, float y, string name) {
			m_win = win;
			m_name = name;
			m_rect = new Rect(x, y, s_nodeWidth, 20);
		}
		
		public InputPlug AddInput(string name)
		{
			InputPlug iplug = new InputPlug(this, 0, PlugCount() * 15 + 20, name);
			m_inplugs.Add(iplug);
			m_rect.height = PlugCount() * 15 + 20;
			return iplug;
		}
		public OutputPlug AddOutput(string name)
		{
			OutputPlug oplug = new OutputPlug(this, 90, PlugCount() * 15 + 20, name);
			m_outplugs.Add(oplug);
			m_rect.height = PlugCount() * 15 + 20;
			return oplug;
		}

		public int PlugCount() {
			return m_inplugs.Count + m_outplugs.Count;
		}

		public void SetPos(int x, int y) {
			m_rect.x = x;
			m_rect.y = y;
		}

		public void DrawLines() {
			for (int i = 0; i < m_outplugs.Count; ++i) {
				m_outplugs[i].DrawLines();
			}
		}
		
		public void Layout() {
			GUIStyle style = new GUIStyle();
			GUI.BeginGroup(m_rect, style);
	
			GUI.Box(new Rect(0,0, m_rect.width, m_rect.height), m_name, s_boxNodeStyle);
			
			for (int i = 0; i < m_inplugs.Count; ++i) {
				m_inplugs[i].Layout();
			}
			
			for (int i = 0; i < m_outplugs.Count; ++i) {
				m_outplugs[i].Layout();
			}
			
			GUI.EndGroup();
			
			
			/*GUI.Label (new Rect (95, 0, 100, 30), "Hello World!");
			if (GUI.Button(new Rect(20, 70, 80, 20), "Button")){
				Debug.Log("CCAAA");
			}
			m_textStr = GUI.TextField(new Rect(20, 100, 80, 20), m_textStr, 8);
			*/
			
		}
		public bool Test(Vector2 p) {
			return m_rect.Contains(p);
		}
		public void Move(Vector2 mv){
			m_rect.x += mv.x;
			m_rect.y += mv.y;
		}
		public bool OnMouseDown(Vector2 p) {
			// Plugs
			Vector2 ppos = new Vector2(m_rect.x, m_rect.y);
			for (int i = 0; i < m_inplugs.Count; ++i) {
				if (m_inplugs[i].OnMouseDown(p - ppos))
					return true;
			}
			for (int i = 0; i < m_outplugs.Count; ++i) {
				if (m_outplugs[i].OnMouseDown(p - ppos))
					return true;
			}
			
			if (Test(p)) {
				m_mdown = true;
				return true;
			}
			return false;
		}
		public bool OnMouseUp(Vector2 p) {
			// Plugs
			Vector2 ppos = new Vector2(m_rect.x, m_rect.y);
			for (int i = 0; i < m_inplugs.Count; ++i) {
				if (m_inplugs[i].OnMouseUp(p - ppos))
					return true;
			}
			for (int i = 0; i < m_outplugs.Count; ++i) {
				if (m_outplugs[i].OnMouseUp(p - ppos))
					return true;
			}
			
			if (m_mdown)
				m_mdown = false;

			if (Test(p))
				return true;
			else
				return false;
		}

		public void OnMouseDrag(Vector2 p, Vector2 diff) {
			// Plugs
			for (int i = 0; i < m_inplugs.Count; ++i) {
				m_inplugs[i].OnMouseDrag(p);
			}
			
			for (int i = 0; i < m_outplugs.Count; ++i) {
				m_outplugs[i].OnMouseDrag(p);
			}
			
			if (m_mdown)
				Move(diff);
		}
	}
	
	public void Update()
	{
		wantsMouseMove = true;
		Repaint();
	}

	
	Vector2 m_oldMousePos;
	void OnGUI () {
		OnNodeGUI();
	}
	public void OnNodeGUI () {
		//Debug.Log("OnGUI Type=" + Event.current.type);
		
		if (s_boxNodeStyle == null) {
			s_boxNodeStyle = new GUIStyle( GUI.skin.box );
			s_boxNodeStyle.normal.background = MakeTex( 2, 2, s_nodeBaseColor);
			s_boxNodeStyle.normal.textColor = s_nodeTextColor;
		}
		if (s_boxOutputPlugStyle == null) {
			s_boxOutputPlugStyle = new GUIStyle( GUI.skin.box );
			s_boxOutputPlugStyle.normal.background = MakeTex( 2, 2, s_outputPlugColor );
		}
		if (s_boxInputPlugStyle == null) {
			s_boxInputPlugStyle = new GUIStyle( GUI.skin.box );
			s_boxInputPlugStyle.normal.background = MakeTex( 2, 2, s_inputPlugColor );
		}
		if (s_outputTextStyle == null) {
			s_outputTextStyle = new GUIStyle( GUI.skin.label );
			s_outputTextStyle.normal.textColor = s_outputTextColor;
			s_outputTextStyle.alignment = TextAnchor.UpperRight; // Right Align
		}
		if (s_inputTextStyle == null) {
			s_inputTextStyle = new GUIStyle( GUI.skin.label );
			s_inputTextStyle.normal.textColor = s_inputTextColor;
		}

		// Draw Lines
		for (int i = 0; i < m_nodes.Count; ++i) {
			m_nodes[i].DrawLines();
		}
		
		// Draw Nodes
		for (int i = 0; i < m_nodes.Count; ++i) {
			m_nodes[i].Layout();
		}
		
		// Node Events
		Vector2 mousePos = Event.current.mousePosition;
		if (Event.current.type == EventType.MouseDown) {
			m_oldMousePos = mousePos;
		}
		for (int i = m_nodes.Count - 1; i >= 0; --i) {
			Node node = m_nodes[i];
			
			bool r = false;
			if (Event.current.type == EventType.MouseDown)
				r = node.OnMouseDown(mousePos);
			else if (Event.current.type == EventType.MouseUp)
				r = node.OnMouseUp(mousePos);
			else if (Event.current.type == EventType.MouseDrag) 
				node.OnMouseDrag(mousePos, new Vector2(mousePos.x - m_oldMousePos.x, mousePos.y - m_oldMousePos.y));
			if (r) {
				m_nodes.Add(node);
				m_nodes.RemoveAt(i);
				break;
			}
		}
		if (Event.current.type == EventType.MouseDrag) {
			m_oldMousePos = mousePos;
		}

		if (Event.current.type == EventType.MouseUp) {	
			SetActiveOutputPlug(null);
		}
		
		OutputPlug activePlug = GetActiveOutputPlug();
		if (activePlug != null) {
			Vector2 sp = activePlug.GetPos();
			Vector2 ep = mousePos;
			drawLine(sp, ep, s_activeLineColor, 2);
		
		}
	}
}
