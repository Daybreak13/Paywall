using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEditor;

namespace Paywall.Editors {

	[CustomEditor(typeof(LevelManagerIRE_PW), true)]
	[InitializeOnLoad]
	public class LevelManagerIRE_PWEditor : Editor {
		//protected SceneViewIcon _sceneViewIcon;

		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		static void DrawGameObjectName(LevelManagerIRE_PW levelManager, GizmoType gizmoType) {
			GUIStyle style = new GUIStyle();
			Vector3 v3FrontTopLeft;

			if (levelManager.RecycleBounds.size != Vector3.zero) {
				style.normal.textColor = Color.yellow;
				v3FrontTopLeft = new Vector3(levelManager.RecycleBounds.center.x - levelManager.RecycleBounds.extents.x, levelManager.RecycleBounds.center.y + levelManager.RecycleBounds.extents.y + 1, levelManager.RecycleBounds.center.z - levelManager.RecycleBounds.extents.z);  // Front top left corner
				Handles.Label(v3FrontTopLeft, "Level Manager Recycle Bounds", style);
				MMDebug.DrawHandlesBounds(levelManager.RecycleBounds, Color.yellow);
			}

			if (levelManager.DeathBounds.size != Vector3.zero) {
				style.normal.textColor = Color.red;
				v3FrontTopLeft = new Vector3(levelManager.DeathBounds.center.x - levelManager.DeathBounds.extents.x, levelManager.DeathBounds.center.y + levelManager.DeathBounds.extents.y + 1, levelManager.DeathBounds.center.z - levelManager.DeathBounds.extents.z);  // Front top left corner
				Handles.Label(v3FrontTopLeft, "Level Manager Death Bounds", style);
				MMDebug.DrawHandlesBounds(levelManager.DeathBounds, Color.red);
			}
		}
	}
}
