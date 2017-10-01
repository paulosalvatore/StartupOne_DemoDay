using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCinematic : MonoBehaviour
{
	public bool ativar;

	public GameObject carro;

	public float distanciaMaxima;
	public Vector3 posicaoBaseCamera;

	public int multiplicadorRight;
	public int multiplicadorForward;

	private int modificadorRight = 1;

	private bool reiniciarCamType = false;

	private void Update()
	{
		if (reiniciarCamType)
		{
			//CameraController.instancia.SetCamType(0);

			reiniciarCamType = false;
		}

		if (!ativar)
			return;

		reiniciarCamType = true;

		//CameraController.instancia.SetCamType(6);

		float distancia = Vector3.Distance(Camera.main.transform.position, carro.transform.position);

		if (distancia > distanciaMaxima)
		{
			modificadorRight *= -1;

			Camera.main.transform.position =
				carro.transform.position +
				posicaoBaseCamera +
				(carro.transform.right * modificadorRight * multiplicadorRight) +
				(carro.transform.forward * multiplicadorForward);
		}
	}
}
