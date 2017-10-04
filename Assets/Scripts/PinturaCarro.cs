using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinturaCarro : MonoBehaviour
{
	private Vector3 posicaoInicial;

	private bool utilizado = false;
	private float tempo;
	private float delay = 1f;

	private void LiberarUtilizado()
	{
		utilizado = false;
	}

	private void Update()
	{
		if (utilizado && Time.time > tempo + delay)
			utilizado = false;
	}

	private void OnTriggerStay(Collider collider)
	{
		if (utilizado)
			return;

		if (collider.gameObject.layer == LayerMask.NameToLayer("PinturaCarro"))
		{
			NVRInteractableItem item = gameObject.GetComponent<NVRInteractableItem>();

			if (item.AttachedHands.Count > 0)
			{
				NVRHand hand = item.AttachedHands[0];

				if ((hand == Jogo.instancia.leftHand &&
					Jogo.instancia.leftHand.Inputs[NVRButtons.Grip].SingleAxis > 0f) ||
					(hand == Jogo.instancia.rightHand &&
					Jogo.instancia.rightHand.Inputs[NVRButtons.Grip].SingleAxis > 0f))
					return;
			}

			tempo = Time.time;
			utilizado = true;

			Jogo.instancia.AlterarPinturaCarro(gameObject);

			GameObject instancia = Instantiate(gameObject, posicaoInicial, transform.rotation);

			instancia.transform.SetParent(transform.parent);

			PinturaCarro pinturaCarro = instancia.GetComponent<PinturaCarro>();

			pinturaCarro.enabled = true;

			instancia.GetComponent<NVRInteractableItem>().enabled = true;
		}
	}
}