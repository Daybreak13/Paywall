using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Paywall;

namespace Paywall.Editors {

	[CustomEditor(typeof(GameManagerIRE_PW), true)]
	[CanEditMultipleObjects]

	/// <summary>
	/// Game manager editor
	/// </summary>

	public class GameManagerIRE_PWEditor : Editor {

		private GameManagerIRE_PW _target {
			get { return (GameManagerIRE_PW)target; }
		}

		/// <summary>
		/// When inspecting a Corgi Controller, we add to the regular inspector some labels, useful for debugging
		/// </summary>
		public override void OnInspectorGUI() {
			if (_target != null) {
				EditorGUILayout.LabelField("Status", _target.Status.ToString());
			}
			DrawDefaultInspector();


		}

		void OnEnable() {
			_target.GameManagerInspectorNeedRedraw += this.Repaint;
		}

		void OnDisable() {
			_target.GameManagerInspectorNeedRedraw -= this.Repaint;
		}
	}
}