using UnityEngine;
using System.Collections;
using NewtonVR;

public class HandPointer : MonoBehaviour
{
	public Color LineColor;
	public float LineWidth = 0.02f;
	public bool ForceLineVisible = true;

	public bool OnlyVisibleOnTrigger = true;

	private LineRenderer Line;

	private NVRHand Hand;

	internal bool lineEnabled = true;

	private void Awake()
	{
		Line = this.GetComponent<LineRenderer>();
		Hand = this.GetComponent<NVRHand>();

		if (Line == null)
		{
			Line = this.gameObject.AddComponent<LineRenderer>();
		}

		if (Line.sharedMaterial == null)
		{
			Line.material = new Material(Shader.Find("Unlit/Color"));
			Line.material.SetColor("_Color", LineColor);
			NVRHelpers.LineRendererSetColor(Line, LineColor, LineColor);
		}

		Line.useWorldSpace = true;
	}

	private void LateUpdate()
	{
		Line.enabled = lineEnabled && (ForceLineVisible || (OnlyVisibleOnTrigger && Hand != null && Hand.Inputs[NVRButtons.Trigger].IsPressed));

		if (Line.enabled)
		{
			Line.material.SetColor("_Color", LineColor);
			NVRHelpers.LineRendererSetColor(Line, LineColor, LineColor);
			NVRHelpers.LineRendererSetWidth(Line, LineWidth, LineWidth);

			RaycastHit hitInfo;
			bool hit = Physics.Raycast(this.transform.position, this.transform.forward, out hitInfo, 1000);
			Vector3 endPoint;

			if (hit)
			{
				endPoint = hitInfo.point;

				if (hitInfo.transform.CompareTag("Driver"))
				{
					Jogo.instancia.PrepararProximaFase();
				}
			}
			else
			{
				endPoint = this.transform.position + (this.transform.forward * 1000f);
			}

			Line.SetPositions(new Vector3[] { this.transform.position, endPoint });
		}
	}
}