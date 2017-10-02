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

	public NVRPlayer jogador;

	private enum Fases
	{
		ALARME,
		ROTACIONAR_VEICULO,
		PINTURA,
		PORTA_MALAS,
		RODAS
	};

	private Fases fase = Fases.ALARME;

	public Shader shieldShader;

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

	private void Awake()
	{
		instancia = this;

		fase = Fases.RODAS;
	}

	private void Start()
	{
		foreach (Transform child in jogador.transform)
		{
			if (child.name == "LeftHand" ||
				child.name == "RightHand" ||
				child.name == "LeftHand [Physical]" ||
				child.name == "RightHand [Physical]")
				DesativarFilhos(child.GetChild(0));
		}
	}

	private void Update()
	{
		if (fase == Fases.RODAS)
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

	public void AlterarEstadoMotor()
	{
		controladorCarro.KillOrStartEngine();
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

	public void FinalizarInteracao()
	{
		if (colidindoRodaCarro)
		{
			foreach (GameObject roda in rodas)
			{
				roda.GetComponent<MeshFilter>().mesh = novaRodaCarro.GetComponent<Roda>().meshFilter.mesh;
			}

			novaRodaCarro.SetActive(false);
		}
	}

	private void DesativarFilhos(Transform destino)
	{
		foreach (Transform child in destino)
			child.gameObject.SetActive(false);
	}
}