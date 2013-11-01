using UnityEngine;
using System.Collections;


namespace nobnak.Rendering {
	
	public class WireframeCamera : MonoBehaviour {
		public bool wireframeMode = true;
		
		void OnPreRender() {
			GL.wireframe = true;
		}
		void OnPostRender() {
			GL.wireframe = false;
		}
	}
}
