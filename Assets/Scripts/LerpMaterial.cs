using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RangeFloat
{
	public float min;
	public float max;

	public RangeFloat(float _min, float _max)
	{
		min = _min;
		max = _max;
	}
}

[System.Serializable]
public class RangeVector4
{
	public Vector4 min;
	public Vector4 max;

	public RangeVector4(Vector4 _min, Vector4 _max)
	{
		min = _min;
		max = _max;
	}
}

public class LerpMaterial : MonoBehaviour
{
	private string variavel;
	private bool pegarValorInicio = false;
	private string tipo;

	// Float
	private float inicioFloat = 0f;

	private float valorFloat = 1f;
	private List<float> materialsInicioFloat = new List<float>();
	private bool floatAtivado = false;

	// Vector4
	private Vector4 inicioVector4 = Vector4.zero;

	private Vector4 valorVector4 = Vector4.one;
	private List<Vector4> materialsInicioVector4 = new List<Vector4>();
	private bool vector4Ativado = false;

	[Header("Duração do Lerp")]
	public float duracao = 1f;

	private float duracaoMinima = 0.0001f;

	private bool executar = false;
	private bool destruir = true;

	private GameObject objetoFade;
	private List<Material> materials;

	private float tempoInicioLerp = 0;
	private int indice = 0;

	private void Start()
	{
		objetoFade = gameObject;

		PegarRenderers();
	}

	private void FixedUpdate()
	{
		AplicarLerp();
	}

	// Float

	public static void Iniciar(GameObject gameObject, string tipo, float valor, float duracao = 1f, bool destruir = true)
	{
		Iniciar(gameObject, tipo, "", valor, duracao, destruir);
	}

	public static void Iniciar(GameObject gameObject, string tipo, string variavel, float valor, float duracao = 1f, bool destruir = true)
	{
		LerpMaterial instancia = Aplicar(gameObject, tipo, duracao, destruir, variavel);

		instancia.floatAtivado = true;
		instancia.pegarValorInicio = true;
		instancia.valorFloat = valor;

		instancia.Iniciar();
	}

	public static void Iniciar(GameObject gameObject, string tipo, RangeFloat intervalo, float duracao = 1f, bool destruir = true)
	{
		Iniciar(gameObject, tipo, "", intervalo, duracao, destruir);
	}

	public static void Iniciar(GameObject gameObject, string tipo, string variavel, RangeFloat intervalo, float duracao = 1f, bool destruir = true)
	{
		LerpMaterial instancia = Aplicar(gameObject, tipo, duracao, destruir, variavel);

		instancia.floatAtivado = true;
		instancia.inicioFloat = intervalo.min;
		instancia.valorFloat = intervalo.max;

		instancia.Iniciar();
	}

	// Vector4

	public static void Iniciar(GameObject gameObject, string tipo, Vector4 valor, float duracao = 1f, bool destruir = true)
	{
		Iniciar(gameObject, tipo, "", valor, duracao, destruir);
	}

	public static void Iniciar(GameObject gameObject, string tipo, string variavel, Vector4 valor, float duracao = 1f, bool destruir = true)
	{
		LerpMaterial instancia = Aplicar(gameObject, tipo, duracao, destruir, variavel);

		instancia.vector4Ativado = true;
		instancia.pegarValorInicio = true;
		instancia.valorVector4 = valor;

		instancia.Iniciar();
	}

	public static void Iniciar(GameObject gameObject, string tipo, RangeVector4 intervalo, float duracao = 1f, bool destruir = true)
	{
		Iniciar(gameObject, tipo, "", intervalo, duracao, destruir);
	}

	public static void Iniciar(GameObject gameObject, string tipo, string variavel, RangeVector4 intervalo, float duracao = 1f, bool destruir = true)
	{
		LerpMaterial instancia = Aplicar(gameObject, tipo, duracao, destruir, variavel);

		instancia.vector4Ativado = true;
		instancia.inicioVector4 = intervalo.min;
		instancia.valorVector4 = intervalo.max;

		instancia.Iniciar();
	}

	private static LerpMaterial Aplicar(GameObject gameObject, string tipo, float duracao, bool destruir, string variavel)
	{
		if (!gameObject)
			throw new NullReferenceException("GameObject do LerpMaterial não está definido.");

		LerpMaterial instancia = gameObject.AddComponent<LerpMaterial>();

		instancia.variavel = variavel;
		instancia.tipo = tipo;
		instancia.duracao = Mathf.Max(instancia.duracaoMinima, duracao);
		instancia.destruir = destruir;

		return instancia;
	}

	private void Iniciar()
	{
		tempoInicioLerp = Time.time;

		executar = true;
	}

	private void Finalizar()
	{
		if (destruir)
			Destroy(this);
	}

	private float PegarValorFloat(Material material)
	{
		switch (tipo)
		{
			case "fade":
				return Mathf.Clamp(material.color.a, 0, 1);

			case "variavel":
				return material.GetFloat(variavel);

			default:
				return 0;
		}
	}

	private Vector4 PegarValorVector4(Material material)
	{
		switch (tipo)
		{
			case "cor":
				return material.color;

			case "corSpecular":
				Color corSpecular = material.GetColor("_SpecColor");
				return new Vector4(corSpecular.r, corSpecular.g, corSpecular.b, corSpecular.a);

			case "corEmission":
				Color corEmission = material.GetColor("_EmissionColor");
				return new Vector4(corEmission.r, corEmission.g, corEmission.b, corEmission.a);

			default:
				return Vector4.zero;
		}
	}

	private void DefinirValorFloat(Material material, float inicioMaterial, float porcentagemCompleta, bool interpolar = true, float forcarValor = 0)
	{
		if (interpolar)
			forcarValor = valorFloat;

		switch (tipo)
		{
			case "fade":
				material.color = new Vector4(
						material.color.r,
						material.color.g,
						material.color.b,
						Mathf.Lerp(
							inicioMaterial,
							forcarValor,
							porcentagemCompleta
						)
					);
				break;

			case "variavel":
				material.SetFloat(
						variavel,
						Mathf.Lerp(
							inicioMaterial,
							forcarValor,
							porcentagemCompleta
						)
					);

				break;

			default:
				break;
		}
	}

	private void DefinirValorVector4(Material material, Vector4 inicioMaterial, float porcentagemCompleta, bool interpolar = true, Vector4 forcarValor = default(Vector4))
	{
		if (interpolar)
			forcarValor = valorVector4;

		switch (tipo)
		{
			case "cor":
				material.color =
					Vector4.Lerp(
						inicioMaterial,
						forcarValor,
						porcentagemCompleta
					);

				break;

			case "corSpecular":
				Vector4 lerpSpec =
					Vector4.Lerp(
						inicioMaterial,
						forcarValor,
						porcentagemCompleta
					);

				material.SetColor(
					"_SpecColor",
					new Color(
						lerpSpec.x,
						lerpSpec.y,
						lerpSpec.z,
						lerpSpec.w
					)
				);

				break;

			case "corEmission":
				Vector4 lerpEmission =
					Vector4.Lerp(
						inicioMaterial,
						forcarValor,
						porcentagemCompleta
					);

				material.SetColor(
					"_EmissionColor",
					new Color(
						lerpEmission.x,
						lerpEmission.y,
						lerpEmission.z,
						lerpEmission.w
					)
				);

				break;

			default:
				break;
		}
	}

	private void AplicarLerp()
	{
		if (!executar || materials.Count == 0)
			return;

		float tempoInicio = Time.time - tempoInicioLerp;
		float porcentagemCompleta = Mathf.Clamp(tempoInicio / duracao, 0, 1);

		int indiceMaterial = 0;

		foreach (Material material in materials)
		{
			float inicioMaterialFloat = inicioFloat;
			Vector4 inicioMaterialVector4 = inicioVector4;

			if (pegarValorInicio)
			{
				if (floatAtivado)
				{
					if (indice == 0)
						materialsInicioFloat.Add(PegarValorFloat(material));

					if (materialsInicioFloat.Count > indiceMaterial)
						inicioMaterialFloat = materialsInicioFloat[indiceMaterial];
				}
				else if (vector4Ativado)
				{
					if (indice == 0)
						materialsInicioVector4.Add(PegarValorVector4(material));

					if (materialsInicioVector4.Count > indiceMaterial)
						inicioMaterialVector4 = materialsInicioVector4[indiceMaterial];
				}
			}

			if (floatAtivado)
			{
				if (indice == 0)
					DefinirValorFloat(material, inicioMaterialFloat, 1, false, inicioMaterialFloat);

				DefinirValorFloat(material, inicioMaterialFloat, porcentagemCompleta);
			}
			else if (vector4Ativado)
			{
				if (indice == 0)
					DefinirValorVector4(material, inicioMaterialVector4, 1, false, inicioMaterialVector4);

				DefinirValorVector4(material, inicioMaterialVector4, porcentagemCompleta);
			}

			if (porcentagemCompleta >= 1.0f)
				executar = false;

			indiceMaterial++;
		}

		indice++;

		if (!executar)
			Finalizar();
	}

	private void PegarRenderers()
	{
		materials = new List<Material>();

		Renderer[] renderers = objetoFade.GetComponentsInChildren<Renderer>();

		foreach (Renderer renderer in renderers)
			foreach (Material material in renderer.materials)
				materials.Add(material);
	}
}