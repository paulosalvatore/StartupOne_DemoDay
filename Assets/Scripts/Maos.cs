using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maos : MonoBehaviour
{
	private void OnTriggerEnter(Collider collider)
	{
		Jogo.instancia.IniciarColisao(gameObject, collider);
	}

	private void OnTriggerStay(Collider collider)
	{
		Jogo.instancia.ProcessarColisao(gameObject, collider);
	}

	private void OnTriggerExit(Collider collider)
	{
		Jogo.instancia.EncerrarColisao(gameObject, collider);
	}
}
