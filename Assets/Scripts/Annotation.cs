using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Annotation
{
	public string created;
	public string modified;
	public string series;
	public string position;
	public string rotation;
	public string scale;
	public int? frame;
	public string notes;
	public string color;

	public Annotation(string series)
	{
		created = DateTime.Now.ToString();
		modified = created;
		this.series = series;
	}
}

[Serializable]
public class AnnotationCollection
{
	public List<Annotation> annotations;

	public AnnotationCollection()
	{
		annotations = new List<Annotation>();
	}

	public void Add(Annotation a)
	{
		annotations.Add(a);
	}
}