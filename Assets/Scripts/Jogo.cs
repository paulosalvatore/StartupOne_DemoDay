using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	// Fases

	internal enum Fases
	{
		ALARME,
		PINTURA,
		RODAS
	};

	internal Fases fase;
	internal Fases proximaFase;
	public List<GameObject> fasesPontos;
	public AudioClip proximaFaseClip;

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

	// Shader

	public Shader shieldShader;

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
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.H))
			AlterarFase(Fases.ALARME);
		else if (Input.GetKeyDown(KeyCode.J))
			AlterarFase(Fases.PINTURA);
		else if (Input.GetKeyDown(KeyCode.K))
			AlterarFase(Fases.RODAS);

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

		leftHand.GetComponent<HandPointer>().lineEnabled = true;
	}

	public void ProximaFase()
	{
		ReproduzirAudio(proximaFaseClip);

		leftHand.GetComponent<HandPointer>().lineEnabled = false;

		AlterarFase(proximaFase);
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

	private void LigarCarro()
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