using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseSystemToggleGenomics : MonoBehaviour, IPointerClickHandler
{

	public void OnPointerClick(PointerEventData eventData)
	{
		string groupNumber = gameObject.name.Split(' ')[1];

		gameObject.GetComponentInParent<Genomics>().ToggleGenomicsGroup(int.Parse(groupNumber));
	}
}