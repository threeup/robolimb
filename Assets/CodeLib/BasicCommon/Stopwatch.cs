using UnityEngine;
using System.Collections;

/// <summary>
/// A high-resolution stopwatch style timer utility class. This wraps an underlying high-resolution
/// timer and makes measuring elapsed intervals easier.
/// </summary>
/// 
public class Stopwatch
{
	System.Diagnostics.Stopwatch sw;
	
	public Stopwatch()
	{
		this.sw = new System.Diagnostics.Stopwatch();
	}

	// Use this for initialization
	public void Start()
	{
		sw.Start();
	}
	
	public void Stop()
	{
		sw.Stop();
	}
	
	public void Reset()
	{
		sw.Reset();
	}
	
	public void Restart()
	{
		sw.Reset();
		sw.Start();
	}
	
	public bool IsRunning
	{
		get { return sw.IsRunning; }
	}
	
	public System.TimeSpan Elapsed
	{
		get{ return this.sw.Elapsed; }
	}
	
	public float ElapsedSeconds
	{
		get { return (float)this.sw.Elapsed.TotalSeconds; }
	}

	public float ElapsedMilliSeconds
	{
		get { return (float)this.sw.ElapsedMilliseconds; }
	}
	

	
	public float LapSeconds()
	{
		float t = ElapsedSeconds;
		Restart();
		return t;
	}
	

	static public Stopwatch StartNew()
	{
		Stopwatch s = new Stopwatch();
		s.Start();
		return s;
	}
}

public class StopwatchUnity
{
	System.Text.StringBuilder sb;
	float sinceStartup;
	float sinceFrame;
    float sinceLastMark;
	
	public StopwatchUnity()
	{
		sb = new System.Text.StringBuilder();
		sinceStartup = 0;
		sinceFrame = 0;
	}

	// Use this for initialization
	public void Start()
	{
		sinceStartup = Time.realtimeSinceStartup;
		sinceFrame = Time.frameCount;
        sinceLastMark = sinceStartup;
	}

	public float ElapsedMilliSeconds
	{
		get { return (Time.realtimeSinceStartup - sinceStartup)*1000f; }
	}

	public float ElapsedFrameCount
	{
		get { return (Time.frameCount - sinceFrame); }
	}

	public void Mark(string prepend)
	{
		//sb.Append(prepend+ElapsedMilliSeconds+" |"+ElapsedFrameCount+"| ");
        sb.Append(prepend + " (" + (int)(ElapsedMilliSeconds) + "MS)  <" + ((Time.realtimeSinceStartup - sinceLastMark) * 1000f) + ">\n");
        sinceLastMark = Time.realtimeSinceStartup;
	}

	public override string ToString()
	{
		return sb.ToString();
	} 
}

