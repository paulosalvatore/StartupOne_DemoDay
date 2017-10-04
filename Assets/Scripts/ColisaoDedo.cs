using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColisaoDedo : MonoBehaviour
{
	private bool interacaoLiberada = true;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void LiberarInteracao()
	{
		interacaoLiberada = true;
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!interacaoLiberada)
			return;

		if (collider.CompareTag("Botão"))
		{
			interacaoLiberada = false;

			if (collider.name.Contains("LigarRadio"))
			{
				Jogo.instancia.AlterarEstadoRadio();
			}
			else if (collider.name.Contains("AlterarRadio"))
			{
				Jogo.instancia.AlterarRadio();
			}
			else if (collider.name.Contains("AlterarGPS"))
			{
				Jogo.instancia.AlterarGPS();
			}
			else if (collider.name.Contains("Carro"))
			{
				Jogo.instancia.LigarCarro();
			}
			else if (collider.name.Contains("Garagem"))
			{
				Jogo.instancia.AbrirPortaGaragem();
			}
			else
			{
				interacaoLiberada = true;
			}

			if (!interacaoLiberada)
				Invoke("LiberarInteracao", 1f);
		}
	}
}