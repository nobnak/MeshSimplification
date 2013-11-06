using UnityEngine;
using System.Collections;


namespace nobnak.Rendering {
	
	public class WireframeCamera : MonoBehaviour {
		public bool wireframeMode = true;
		
		void OnPreRender() {
			GL.wireframe = wireframeMode;
		}
		void OnPostRender() {
			GL.wireframe = false;
		}
	}
}
