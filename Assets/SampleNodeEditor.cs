using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class SampleNodeEditor : NodeEditorWindow {

	SampleNodeEditor() {
		preset();
	}
	~SampleNodeEditor() {
	}

	void preset() {
		Node node1 = AddNode("Node1");
		OutputPlug plug1 = node1.AddOutput("OutABC");
		node1.SetPos(10, 100);
		node1.AddInput("InABC");

		Node node2 = AddNode("Node2");
		node2.SetPos(250, 150);
		node2.AddOutput("OutDDD");
		InputPlug plug2 = node2.AddInput("InFFF");
		node2.AddInput("InGGG");

		plug1.AddTarget(plug2); // Link

		Node node3 = AddNode("Node3");
		node3.AddOutput("OutHHH");
		node3.SetPos(10, 300);

	}

	void OnGUI()
	{
		base.OnNodeGUI();

		// ノード作成ボタン
		if ( GUI.Button( new Rect(000, 350, 150, 30), "new node" ) )
		{
			Node node = AddNode("Node1");
			node.AddInput("Input");
			node.AddOutput("Output");
		}

	}

	// メニュー表示
	[MenuItem ("Window/Sample NodeEditor")]
	static void Init () {
		EditorWindow.GetWindow (typeof (SampleNodeEditor));
	}
}
