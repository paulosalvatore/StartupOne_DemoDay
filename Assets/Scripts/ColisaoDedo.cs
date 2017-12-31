using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColisaoDedo : MonoBehaviour
{
	public Transform colliderDedo;
	private bool instanciarColliderDedoLeft = true;
	private bool instanciarColliderDedoRight = true;

	private void Start()
	{
		StartCoroutine(PosicionarColliderDedos());
	}

	private IEnumerator PosicionarColliderDedos()
	{
		while (true)
		{
			if (instanciarColliderDedoLeft)
			{
				GameObject pontaIndicador = GameObject.Find("hands:b_l_index_ignore");

				if (pontaIndicador != null)
				{
					GameObject colliderDedoLeft = Instantiate(colliderDedo.gameObject);

					colliderDedoLeft.transform.SetParent(pontaIndicador.transform);

					colliderDedoLeft.transform.localPosition = colliderDedo.transform.localPosition;
					colliderDedoLeft.transform.localRotation = colliderDedo.transform.localRotation;

					instanciarColliderDedoLeft = false;
				}
			}

			if (instanciarColliderDedoRight)
			{
				GameObject pontaIndicador = GameObject.Find("hands:b_r_index_ignore");

				if (pontaIndicador != null)
				{
					GameObject colliderDedoRight = Instantiate(colliderDedo.gameObject);

					colliderDedoRight.transform.SetParent(pontaIndicador.transform);

					colliderDedoRight.transform.localPosition = colliderDedo.transform.localPosition;
					colliderDedoRight.transform.localRotation = colliderDedo.transform.localRotation;

					instanciarColliderDedoRight = false;
				}
			}

			if (!instanciarColliderDedoLeft &&
				!instanciarColliderDedoRight)
			{
				Destroy(colliderDedo.gameObject);

				break;
			}

			yield return null;
		}
	}

	/*

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

	*/
}
