using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jogo : MonoBehaviour
{
	public static Jogo instancia;

	public RCC_CarControllerV3 controladorCarro;

	public Touch leftTouch;
	public Touch rightTouch;

	public NVRHand leftHand;
	public NVRHand rightHand;

	public OvrAvatar ovrAvatar;

	public NVRPlayer jogador;

	private enum Fases
	{
		ALARME,
		PINTURA,
		PORTA_MALAS,
		RODAS
	};

	private Fases fase;

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

	public Shader shieldShader;

	private void Awake()
	{
		instancia = this;

		Invoke("ChecarTouchVR", 0.1f);
	}

	private void Start()
	{
		CarregarLataria();

		//AlterarFase(Fases.ALARME);
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
			if (rightTouch.buttonOnePress)
			{
				LigarCarro();
			}
			else if (rightTouch.buttonTwoPress)
			{
				AbrirCarro();
			}
			else if (rightTouch.buttonStickPress)
			{
				FecharCarro();
			}
		}
		if (fase == Fases.PINTURA)
		{
			if (pinturaSelecionada &&
				(leftTouch.trigger > 0f ||
				rightTouch.trigger > 0f))
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

	private void AlterarFase(Fases _fase)
	{
		fase = _fase;

		if (fase == Fases.ALARME)
		{
			controleCarro.SetActive(true);

			ovrAvatar.ShowControllers(true);
		}
		else if (fase == Fases.PINTURA)
		{
			controleCarro.SetActive(false);

			ovrAvatar.ShowControllers(false);
		}
		else if (fase == Fases.RODAS)
		{
			GameObject[] rodas = GameObject.FindGameObjectsWithTag("Roda");

			foreach (GameObject roda in rodas)
			{
				roda.GetComponent<NVRInteractableItem>().enabled = true;
			}
		}
	}

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

	private void AlterarPinturaCarro()
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

	static public void ReproduzirAudio(AudioClip clip = null)
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