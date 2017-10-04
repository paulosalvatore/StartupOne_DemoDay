using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Jogo : MonoBehaviour
{
	// Básicos

	public static Jogo instancia;

	public RCC_CarControllerV3 controladorCarro;
	private Vector3 posicaoInicialCarro = Vector3.zero;
	private Quaternion rotacaoInicialCarro;

	// Mãos e Jogador

	public NVRHand leftHand;

	public NVRHand rightHand;

	public OvrAvatar ovrAvatar;

	public NVRPlayer jogador;

	public Transform colliderDedo;
	private bool instanciarColliderDedoLeft = true;
	private bool instanciarColliderDedoRight = true;

	// Fases

	internal enum Fases
	{
		AGUARDANDO,
		ALARME,
		PINTURA,
		RODAS,
		SUSPENSAO,
		INTERNO,
		PILOTAR
	};

	internal Fases fase;
	internal Fases proximaFase;
	public List<GameObject> fasesPontos;
	public AudioClip proximaFaseClip;
	private bool prepararProximaFase = false;

	// Alarme

	public GameObject controleCarro;

	private bool carroAberto = false;
	private bool ligarCarroLiberado = true;

	public AudioClip abrirCarroClip;
	public AudioClip fecharCarroClip;
	public AudioClip ligarCarroClip;

	// Pintura

	public float duracaoMudancaCorLataria;
	private List<GameObject> lataria = new List<GameObject>();
	private List<Material> materiaisLataria = new List<Material>();
	private GameObject pinturaSelecionada;

	// Rodas

	public GameObject[] rodas;
	private List<Material> materialsRodas = new List<Material>();
	private List<Shader> shadersRodas = new List<Shader>();
	private List<Color> coresRodas = new List<Color>();

	public RangeFloat rangeRodas;
	private bool exibindoShaderRodas = false;
	private bool inverterRangeRodas = false;
	public Color corShieldRodas;
	public float duracaoPulsoRodas;
	internal bool colidindoRodaCarro;
	internal GameObject novaRodaCarro;

	// Suspensão

	public SuspensionSportcar suspensao;

	// Interno

	private bool aguardarBotaoFase = false;

	// Shader

	public Shader shieldShader;

	// Radio - Carro

	private bool radioLigado = false;
	private bool gpsLigado = false;
	public AudioSource radioAudioSource;
	public MeshRenderer telaCarro;
	public Material telaPretaMaterial;
	public Material radioMaterial;
	public Material gpsMaterial;

	// Garagem

	public Transform portaGaragem;
	private bool garagemAberta = false;
	public AudioClip portaGaragemClip;

	// Métodos Nativos

	private void Awake()
	{
		instancia = this;

		Invoke("ChecarTouchVR", 0.1f);
	}

	private void Start()
	{
		CarregarLataria();

		Invoke("IniciarJogo", 5f);

		MoverJogador(fasesPontos[0]);

		DesativarPontosFases();

		Invoke("GravarPosicaoCarro", 1f);

		StartCoroutine(PosicionarColliderDedos());
	}

	private void Update()
	{
		if (prepararProximaFase)
		{
			if (leftHand.Inputs[NVRButtons.Trigger].SingleAxis == 0f)
			{
				prepararProximaFase = false;

				ProximaFase();
			}
		}

		if (fase == Fases.ALARME)
		{
			if (rightHand.Inputs[NVRButtons.A].PressDown)
			{
				LigarCarro();
			}
			else if (rightHand.Inputs[NVRButtons.B].PressDown)
			{
				AbrirCarro();
			}
			else if (rightHand.Inputs[NVRButtons.Touchpad].PressDown)
			{
				FecharCarro();
			}
		}
		else if (fase == Fases.PINTURA)
		{
			controladorCarro.brakeInput = 1f;

			if (pinturaSelecionada &&
				(leftHand.Inputs[NVRButtons.Trigger].SingleAxis > 0f ||
				rightHand.Inputs[NVRButtons.Trigger].SingleAxis > 0f))
			{
				AlterarPinturaCarro();
			}
		}
		else if (fase == Fases.RODAS)
		{
			if (!exibindoShaderRodas &&
				((leftHand.CurrentlyInteracting && leftHand.CurrentlyInteracting.CompareTag("Roda")) &&
				(rightHand.CurrentlyInteracting && rightHand.CurrentlyInteracting.CompareTag("Roda"))))
			{
				foreach (GameObject roda in rodas)
				{
					MeshRenderer[] meshRenderers = roda.GetComponentsInChildren<MeshRenderer>();

					foreach (MeshRenderer meshRenderer in meshRenderers)
					{
						foreach (Material material in meshRenderer.materials)
						{
							materialsRodas.Add(material);
							shadersRodas.Add(material.shader);
							coresRodas.Add(material.color);

							material.shader = shieldShader;
							material.color = corShieldRodas;
						}
					}
				}

				exibindoShaderRodas = true;
			}
			else if (exibindoShaderRodas &&
				(!leftHand.CurrentlyInteracting &&
				!rightHand.CurrentlyInteracting))
			{
				int chave = 0;

				foreach (Material material in materialsRodas)
				{
					Shader shader = shadersRodas[chave];
					Color cor = coresRodas[chave];

					material.shader = shader;
					material.color = cor;

					chave++;
				}

				exibindoShaderRodas = false;
			}
			else if (exibindoShaderRodas)
			{
				int chave = 0;

				foreach (GameObject roda in rodas)
				{
					if (!roda.GetComponent<LerpMaterial>())
					{
						RangeFloat range =
							inverterRangeRodas
								?
							new RangeFloat(rangeRodas.max, rangeRodas.min)
								:
							rangeRodas;

						if (chave == 0)
							inverterRangeRodas = !inverterRangeRodas;

						LerpMaterial.Iniciar(roda, "variavel", "_Strength", range, duracaoPulsoRodas);
					}

					chave++;
				}
			}
		}
		else if (fase == Fases.SUSPENSAO)
		{
			float v = Input.GetAxis("Vertical");

			suspensao.FrontSpringsOffset = v * -1f;
			suspensao.RearSpringsOffset = v * -1f;

			if (leftHand.Inputs[NVRButtons.X].PressDown)
				rodas[0].SetActive(false);
			else if (leftHand.Inputs[NVRButtons.X].PressUp)
				rodas[0].SetActive(true);

			if (leftHand.Inputs[NVRButtons.Y].PressDown)
				rodas[1].SetActive(false);
			else if (leftHand.Inputs[NVRButtons.Y].PressUp)
				rodas[1].SetActive(true);
		}

		/*if (aguardarBotaoFase &&
			(proximaFase == Fases.SUSPENSAO ||
			proximaFase == Fases.INTERNO ||
			proximaFase == Fases.PILOTAR))
		{
			if (leftHand.Inputs[NVRButtons.Touchpad].PressDown)
			{
				AlterarFase(proximaFase);

				aguardarBotaoFase = false;
			}
		}*/
	}

	private void LateUpdate()
	{
		if (posicaoInicialCarro != Vector3.zero &&
			(controladorCarro.transform.position != posicaoInicialCarro ||
			controladorCarro.transform.rotation != rotacaoInicialCarro))
		{
			controladorCarro.transform.position = posicaoInicialCarro;
			controladorCarro.transform.rotation = rotacaoInicialCarro;
		}
	}

	// Início do Jogo/Fases

	private void IniciarJogo()
	{
		AlterarFase(Fases.ALARME);
	}

	private void MoverJogador(GameObject ponto)
	{
		jogador.transform.position = ponto.transform.position;
		jogador.transform.rotation = ponto.transform.rotation;
	}

	private void AlterarFase(Fases _fase)
	{
		fase = _fase;

		DesativarPontosFases();

		if (fase == Fases.ALARME)
		{
			controleCarro.SetActive(true);

			ovrAvatar.ShowRightController(true);

			proximaFase = Fases.PINTURA;

			Invoke("PrepararFase", 10f);
		}
		else if (fase == Fases.PINTURA)
		{
			MoverJogador(fasesPontos[1]);

			if (controladorCarro.engineRunning)
				controladorCarro.KillOrStartEngine();

			controleCarro.SetActive(false);

			ovrAvatar.ShowControllers(false);

			proximaFase = Fases.RODAS;

			Invoke("PrepararFase", 20f);
		}
		else if (fase == Fases.RODAS)
		{
			MoverJogador(fasesPontos[2]);

			GameObject[] rodas = GameObject.FindGameObjectsWithTag("Roda");

			foreach (GameObject roda in rodas)
			{
				roda.GetComponent<NVRInteractableItem>().enabled = true;
			}

			proximaFase = Fases.SUSPENSAO;

			Invoke("PrepararFase", 15f);
		}
		else if (fase == Fases.SUSPENSAO)
		{
			MoverJogador(fasesPontos[3]);

			controladorCarro.chassis.transform.Find("Corpo").gameObject.SetActive(false);
			controladorCarro.transform.Find("Lights").gameObject.SetActive(false);

			proximaFase = Fases.INTERNO;

			Invoke("PrepararFase", 20f);
		}
		else if (fase == Fases.INTERNO)
		{
			controladorCarro.chassis.transform.Find("Corpo").gameObject.SetActive(true);
			controladorCarro.transform.Find("Lights").gameObject.SetActive(true);

			MoverJogador(fasesPontos[4]);

			proximaFase = Fases.PILOTAR;
		}
		else if (fase == Fases.PILOTAR)
		{
			if (!controladorCarro.engineRunning)
				AlterarEstadoMotor();

			jogador.transform.SetParent(controladorCarro.transform);
			leftHand.gameObject.SetActive(false);
			rightHand.gameObject.SetActive(false);
			ovrAvatar.ShowFirstPerson = false;
			ovrAvatar.transform.Find("hand_left").gameObject.SetActive(false);
			ovrAvatar.transform.Find("hand_right").gameObject.SetActive(false);

			posicaoInicialCarro = Vector3.zero;
		}
	}

	private void PrepararFase()
	{
		if (proximaFase == Fases.PINTURA)
		{
			fasesPontos[1].SetActive(true);
		}
		else if (proximaFase == Fases.RODAS)
		{
			fasesPontos[2].SetActive(true);
		}
		else if (proximaFase == Fases.SUSPENSAO)
		{
			fasesPontos[3].SetActive(true);
		}
		else if (proximaFase == Fases.INTERNO)
		{
			fasesPontos[4].SetActive(true);
		}
		/*else if (proximaFase == Fases.SUSPENSAO ||
				proximaFase == Fases.INTERNO ||
				proximaFase == Fases.PILOTAR)
		{
			aguardarBotaoFase = true;
		}*/

		leftHand.GetComponent<HandPointer>().lineEnabled = true;
	}

	public void ProximaFase()
	{
		ReproduzirAudio(proximaFaseClip);

		leftHand.GetComponent<HandPointer>().lineEnabled = false;

		AlterarFase(proximaFase);
	}

	public void PrepararProximaFase()
	{
		prepararProximaFase = true;
	}

	private void DesativarPontosFases()
	{
		leftHand.GetComponent<HandPointer>().lineEnabled = false;

		foreach (GameObject objeto in fasesPontos)
		{
			if (objeto.activeSelf)
				objeto.SetActive(false);
		}
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

					colliderDedoLeft.transform.localPosition = Vector3.zero;

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

					colliderDedoRight.transform.localPosition = Vector3.zero;

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

	// Controles

	private void ChecarTouchVR()
	{
		foreach (Transform child in jogador.transform)
		{
			if (child.name.Contains("Hand") && child.childCount > 0)
			{
				DesativarFilhos(child.GetChild(0));

				if (child.name.Contains("[Physical]"))
				{
					Collider[] colliders = child.GetChild(1).GetComponentsInChildren<Collider>();

					foreach (Collider collider in colliders)
						collider.isTrigger = true;
				}
			}
		}
	}

	// Carro

	private void GravarPosicaoCarro()
	{
		posicaoInicialCarro = controladorCarro.transform.position;

		rotacaoInicialCarro = controladorCarro.transform.rotation;
	}

	private void AbrirCarro()
	{
		if (carroAberto)
			return;

		carroAberto = true;

		ReproduzirAudio(abrirCarroClip);

		LigarIndicadoresCarro();
	}

	private void LigarIndicadoresCarro()
	{
		controladorCarro.indicatorsOn = RCC_CarControllerV3.IndicatorsOn.Off;
		controladorCarro.indicatorsOn = RCC_CarControllerV3.IndicatorsOn.All;

		Invoke("DesligarIndicadoresCarro", 0.5f);
	}

	private void DesligarIndicadoresCarro()
	{
		controladorCarro.indicatorsOn = RCC_CarControllerV3.IndicatorsOn.Off;
	}

	private void FecharCarro()
	{
		if (!carroAberto)
			return;

		carroAberto = false;

		ReproduzirAudio(fecharCarroClip);

		LigarIndicadoresCarro();
	}

	public void LigarCarro()
	{
		if (!ligarCarroLiberado)
			return;

		ligarCarroLiberado = false;

		AlterarEstadoMotor();

		ReproduzirAudio(ligarCarroClip);

		Invoke("LiberarLigarCarro", 1f);
	}

	private void LiberarLigarCarro()
	{
		ligarCarroLiberado = true;
	}

	private void AlterarEstadoMotor()
	{
		controladorCarro.KillOrStartEngine();
	}

	private void CarregarLataria()
	{
		GameObject[] objetos = FindObjectsOfType<GameObject>();

		foreach (GameObject objeto in objetos)
		{
			if (objeto.name.Contains("Lataria"))
			{
				lataria.Add(objeto);

				Renderer[] renderers = objeto.GetComponentsInChildren<Renderer>();

				foreach (Renderer renderer in renderers)
					materiaisLataria.Add(renderer.material);
			}
		}
	}

	public void AlterarPinturaCarro(GameObject objeto)
	{
		pinturaSelecionada = objeto;

		AlterarPinturaCarro();
	}

	public void AlterarPinturaCarro()
	{
		Material material = pinturaSelecionada.GetComponent<MeshRenderer>().material;

		Vector4 corSelecionada =
			new Vector4(
				material.color.r,
				material.color.g,
				material.color.b,
				material.color.a
			);

		Vector4 specSelecionado =
			new Vector4(
				material.GetColor("_SpecColor").r,
				material.GetColor("_SpecColor").g,
				material.GetColor("_SpecColor").b,
				material.GetColor("_SpecColor").a
			);

		Vector4 emissionSelecionado =
			new Vector4(
				material.GetColor("_EmissionColor").r,
				material.GetColor("_EmissionColor").g,
				material.GetColor("_EmissionColor").b,
				material.GetColor("_EmissionColor").a
			);

		foreach (GameObject objeto in lataria)
		{
			LerpMaterial.Iniciar(objeto, "cor", corSelecionada, duracaoMudancaCorLataria);
			LerpMaterial.Iniciar(objeto, "corSpecular", specSelecionado, duracaoMudancaCorLataria);
			LerpMaterial.Iniciar(objeto, "corEmission", emissionSelecionado, duracaoMudancaCorLataria);
		}
	}

	public void AlterarEstadoRadio()
	{
		radioLigado = !radioLigado;

		if (radioLigado)
		{
			AlterarRadio();

			radioAudioSource.Play();
		}
		else
		{
			telaCarro.material = telaPretaMaterial;

			radioAudioSource.Pause();
		}
	}

	public void AlterarRadio()
	{
		if (!radioLigado)
			return;

		gpsLigado = false;

		telaCarro.material = radioMaterial;
	}

	public void AlterarGPS()
	{
		if (!radioLigado)
			return;

		gpsLigado = true;

		telaCarro.material = gpsMaterial;
	}

	// Garagem

	public void AbrirPortaGaragem()
	{
		if (garagemAberta)
			return;

		ReproduzirAudio(portaGaragemClip);

		iTween.RotateAdd(
			portaGaragem.gameObject,
			iTween.Hash(
						"x", 90f,
						"time", 4f,
						"easeType", iTween.EaseType.easeInQuad
					)
				);

		garagemAberta = true;

		ProximaFase();

		PrepararFase();
	}

	// Processamento de Eventos Básicos e Colisões

	public void FinalizarInteracao()
	{
		if (colidindoRodaCarro)
		{
			foreach (GameObject roda in rodas)
			{
				roda.GetComponent<MeshFilter>().mesh = novaRodaCarro.GetComponent<MeshFilter>().mesh;
				roda.GetComponent<MeshRenderer>().materials = novaRodaCarro.GetComponent<MeshRenderer>().materials;
				roda.transform.localScale = novaRodaCarro.transform.localScale;

				Vector3 rotacao = roda.transform.localEulerAngles;

				if (roda.name.Contains("FL"))
					rotacao.y = 0;
				else if (roda.name.Contains("FR"))
					rotacao.y = 180f;
				else if (roda.name.Contains("RL"))
					rotacao.y = 0;
				else if (roda.name.Contains("RR"))
					rotacao.y = 180f;

				roda.transform.localEulerAngles = rotacao;
			}

			Destroy(novaRodaCarro);
		}
	}

	public void ProcessarTriggerEnter(NVRHand hand, Collider collider)
	{
	}

	public void ProcessarTriggerStay(NVRHand hand, Collider collider)
	{
		if (collider.CompareTag("PinturaCarro") &&
			pinturaSelecionada != collider.gameObject)
		{
			pinturaSelecionada = collider.gameObject;
		}
	}

	public void ProcessarTriggerExit(NVRHand hand, Collider collider)
	{
		if (collider.CompareTag("PinturaCarro") &&
			pinturaSelecionada == collider.gameObject)
		{
			pinturaSelecionada = null;
		}
	}

	private void DesativarFilhos(Transform destino)
	{
		foreach (Transform child in destino)
			child.gameObject.SetActive(false);
	}

	private void AtivarFilhos(Transform destino)
	{
		foreach (Transform child in destino)
			child.gameObject.SetActive(true);
	}

	// Métodos Estáticos

	public static void ReproduzirAudio(AudioClip clip = null)
	{
		if (clip == null)
			return;

		GameObject objeto = new GameObject();

		objeto.name = clip.name;

		AudioSource audioSource = objeto.AddComponent<AudioSource>();
		audioSource.clip = clip;
		audioSource.Play();

		Destroy(objeto, clip.length);
	}
}