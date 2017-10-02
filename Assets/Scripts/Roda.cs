using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roda : MonoBehaviour
{
	private void OnTriggerStay(Collider collider)
	{
		if (collider.name.Contains("Wheel"))
		{
			Jogo.instancia.novaRodaCarro = gameObject;
			Jogo.instancia.colidindoRodaCarro = true;
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (collider.name.Contains("Wheel"))
		{
			Jogo.instancia.colidindoRodaCarro = false;
		}
	}
}