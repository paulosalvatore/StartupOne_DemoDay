using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jogo : MonoBehaviour
{
	public static Jogo instancia;

	public RCC_CarControllerV3 controladorCarro;

	public Touch leftTouch;
	public Touch rightTouch;

	public GameObject leftHand;
	public GameObject rightHand;

	public Transform objetosMaos;

	private enum Fases
	{
		ALARME,
		ROTACIONAR_VEICULO,
		PINTURA,
		PORTA_MALAS,
		RODAS
	};

	private Fases fase = Fases.ALARME;

	// Rodas
	public GameObject rodaFrenteEsquerda;
	public GameObject rodaFrenteDireita;
	public GameObject rodaTrasEsquerda;
	public GameObject rodaTrasDireita;

	private GameObject rodaMaoEsquerda;
	private GameObject rodaMaoDireita;
	private GameObject rodaSelecionada;
	private bool pegarRodaDisponivel = false;
	private GameObject slotRodaSelecionado;
	private bool rodaPega = false;

	private void Awake()
	{
		instancia = this;

		// Testes com a Roda
		fase = Fases.RODAS;
		pegarRodaDisponivel = true;
	}

	private void Update()
	{
		if (fase == Fases.RODAS)
		{
			if (pegarRodaDisponivel &&
				rodaSelecionada &&
				leftTouch.grip == 1f &&
				rightTouch.grip == 1f)
			{
				PegarRoda();
			}
			else if (rodaPega &&
				slotRodaSelecionado &&
				leftTouch.grip == 0f &&
				rightTouch.grip == 0f)
			{
				AplicarRoda();
			}
		}
	}

	public void AlterarEstadoMotor()
	{
		controladorCarro.KillOrStartEngine();
	}

	// Rodas

	private void SelecionarRoda()
	{
		Debug.Log("SelecionarRoda");

		rodaSelecionada = rodaMaoEsquerda;
	}

	private void DesselecionarRoda()
	{
		Debug.Log("DesselecionarRoda");

		rodaSelecionada = null;
	}

	private void PegarRoda()
	{
		Debug.Log("PegarRoda");

		pegarRodaDisponivel = false;
		rodaPega = true;

		rodaSelecionada.transform.SetParent(objetosMaos);
	}

	private void AplicarRoda()
	{
		Debug.Log("AplicarRoda");

		pegarRodaDisponivel = false;

		rodaSelecionada.transform.SetParent(objetosMaos);
	}

	private void AlterarRodas()
	{
		/*
		 * Fluxo da Experiência de Alteração de Rodas
		 * - Posicionar o jogador para a área específica.
		 * - Ele pode apontar para a área ou apertar um botão para teleportar.
		 * - Quando estiver na área, as rodas devem começar a emitir um efeito
		 * para o jogador saber que deve pegá-las.
		 * - O jogador deverá encostar as mãos na roda, apertar o botão de grip
		 * - A partir disso, a roda fica na mão dele.
		 * - Automaticamente as rodas atuais do veículo ficam com o efeito
		 * de shield.
		 * - O jogador deverá colocar a roda da sua mão em cima da roda do
		 * carro e soltar o grip (dedo do meio).
		 * - O grip dos dois controles deverá ser pressionado.
		 * - Quando soltar o grip, a roda irá ficar no lugar demarcado.
		 */
	}

	private void AlterarPintura()
	{
		/*
		 * Fluxo da Experiência de Alteração de Pintura
		 * - O jogador irá se teleportar para próximo do balcão de pintura,
		 * no local demarcado.
		 * - Quando ele estiver nesse local, terá diversos elementos com
		 * cores, onde ele deve chegar com o controle no trigger desses
		 * elementos e apertar o botão do indicador (trigger).
		 * - Ao apertar, o carro deve mudar a cor da pintura.
		 */
	}

	private Touch PegarTouch(GameObject objeto)
	{
		if (objeto == leftHand)
			return leftTouch;
		else
			return rightTouch;
	}

	public void IniciarColisao(GameObject objeto, Collider collider)
	{
		if (fase == Fases.RODAS)
		{
			Debug.Log("IniciarColisao - Rodas");

			if (pegarRodaDisponivel)
			{
				Touch touch = PegarTouch(objeto);

				if (touch == leftTouch)
					rodaMaoEsquerda = collider.gameObject;
				else if (touch == rightTouch)
					rodaMaoDireita = collider.gameObject;

				if (rodaMaoEsquerda && rodaMaoDireita)
					SelecionarRoda();
				else
					DesselecionarRoda();
			}
		}
	}

	public void ProcessarColisao(GameObject objeto, Collider collider)
	{
		if (fase == Fases.RODAS)
		{
			Debug.Log("ProcessarColisao - Rodas");

			if (rodaPega)
			{
				if (collider.gameObject == rodaFrenteEsquerda ||
					collider.gameObject == rodaFrenteDireita ||
					collider.gameObject == rodaTrasEsquerda ||
					collider.gameObject == rodaTrasDireita)
					slotRodaSelecionado = collider.gameObject;
			}
		}
	}

	public void EncerrarColisao(GameObject objeto, Collider collider)
	{
		if (fase == Fases.RODAS)
		{
			Debug.Log("EncerrarColisao - Rodas");

			if (pegarRodaDisponivel)
			{
				Touch touch = PegarTouch(objeto);

				if (touch == leftTouch)
					rodaMaoEsquerda = null;
				else if (touch == rightTouch)
					rodaMaoDireita = null;
			}
			else if (rodaPega)
			{
				if (collider.gameObject == rodaFrenteEsquerda ||
					collider.gameObject == rodaFrenteDireita ||
					collider.gameObject == rodaTrasEsquerda ||
					collider.gameObject == rodaTrasDireita)
					slotRodaSelecionado = null;
			}
		}
	}
}
