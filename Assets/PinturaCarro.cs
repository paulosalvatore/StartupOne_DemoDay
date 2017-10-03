using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinturaCarro : MonoBehaviour
{
	private Vector3 posicaoInicial;

	void Awake()
	{
		posicaoInicial = transform.position;
	}

	void Update()
	{
	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.layer == LayerMask.NameToLayer("RCC"))
		{
			Destroy(gameObject);

			Jogo.instancia.AlterarPinturaCarro(gameObject);

			GameObject instancia = Instantiate(gameObject, posicaoInicial, transform.rotation);

			instancia.transform.SetParent(transform.parent);

			instancia.GetComponent<PinturaCarro>().enabled = true;
			instancia.GetComponent<NVRInteractableItem>().enabled = true;
		}
	}
}