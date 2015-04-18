using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Linq;

public static class Utilities
{
	public static void SortChildren(this GameObject o)
    {
        var children = o.GetComponentsInChildren<Transform>(true).ToList();
        children.Remove(o.transform);
        children.Sort(CompareChildren);
        for (int i = 0; i < children.Count; i++)
            children[i].SetSiblingIndex(i);
    }
    private static int CompareChildren(Transform lhs, Transform rhs)
    {
        if (lhs == rhs) return 0;
        var test = rhs.gameObject.activeInHierarchy.CompareTo(lhs.gameObject.activeInHierarchy);
        if (test != 0) return test;
        if (lhs.localPosition.z < rhs.localPosition.z) return -1;
        if (lhs.localPosition.z > rhs.localPosition.z) return 1;
        return 0;
    }
	public static string GetStackTrace(int skipFrames = 1)
	{
		StringBuilder sb = new StringBuilder();
		System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);

		// maybe count-skipframes;
		string stackIndent = "|";
		string filename = "";
        for(int i = skipFrames; i< st.FrameCount; i++ )
        {
        	stackIndent += " ";
            System.Diagnostics.StackFrame sf = st.GetFrame(i);
            sb.Append(stackIndent);
            filename = sf.GetFileName();
            if (filename != null)
            {
	            int lastIndex = filename.LastIndexOf("\\");
	            if (lastIndex > 0)
	            {
	            	sb.Append(filename.Substring(lastIndex));	
	            }
	            else
	            {
	            	sb.Append(filename);	
	            }
	        }
            
            sb.Append(sf.GetFileLineNumber());
        }
        return sb.ToString();
	}

	public static string FormattedRealTime()
	{
		float seconds = Mathf.Round(Time.realtimeSinceStartup);
		float minutes = Mathf.Floor(seconds / 60f);
		seconds -= minutes*60f;
		return String.Format("@{0:00}:{1:00}", minutes,seconds);
	}


	public static string FormattedTime(float ticks)
	{
		float seconds = Mathf.Round(ticks);
		float minutes = Mathf.Floor(seconds / 60f);
		seconds -= minutes*60f;
		float hours = Mathf.Floor(minutes / 60f);
		minutes -= hours*60f;
		return String.Format("{0}:{1:00}:{2:00}",hours, minutes,seconds);
	}

	public static string FormattedTimeLetters(float ticks)
	{
		float seconds = Mathf.Round(ticks);
		float minutes = Mathf.Floor(seconds / 60f);
		seconds -= minutes*60f;
		float hours = Mathf.Floor(minutes / 60f);
		minutes -= hours*60f;
		return String.Format("{0}h {1:00}m",hours, minutes);
	}

	public static string FormattedMinutesLetters(float minutes)
	{
		float hours = Mathf.Floor(minutes / 60f);
		minutes -= hours*60f;
		return String.Format("{0}h {1:00}m",hours, minutes);
	}
	
	public static string ColorToHex(Color32 color)
	{
		return (color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2"));
		
	}
	
	public static Color HexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
	}
	
	public static Color AdjustRGBAColor(Color32 color, float r = 1f, float g = 1f, float b = 1f, float a = 1f)
	{
		UnityEngine.Color c = color;
		
		c.r *= r;
		c.g *= g;
		c.b *= b;
		c.a *= a;
		
		return c;
	}
	
	public static Color AdjustHSBColor(Color32 color, float h = 1f, float s = 1f, float b = 1f)
	{
		HSBColor hsbColor = HSBColor.FromColor(color);
		
		hsbColor.h *= h;
		hsbColor.s *= s;
		hsbColor.b *= b;
		
		return hsbColor.ToColor();
	}
	
	public static string UppercaseFirst(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		return char.ToUpper(text[0]) + text.Substring(1);
	}
	
	
	static public void SetLayerInChildren(Transform t, int layer)
	{
		if (t.gameObject.layer != LayerHelper.Interactable && t.gameObject.layer != LayerHelper.IgnoreRaycast && 
            (layer != LayerHelper.PlayerDefault || t.gameObject.layer != LayerHelper.VFX )
            )
			t.gameObject.layer = layer;
		
		for( int i = 0, n = t.childCount; i < n; i++ )
		{
			SetLayerInChildren(t.GetChild(i), layer);
		}
	}
	public static string CommasInNumbers(double number)
	{
		return string.Format("{0:#,###0}", number);
	}
		
	public static int CarouselIndex(int start, int offset, int min, int max)
	{
		int end = start;
		while (Mathf.Abs(offset) > 0)
		{
			if (offset > 0)
			{
				++end;
				if (end > max)
				{
					end = min;
				}
				--offset;
			}
			else if (offset < 0)
			{
				--end;
				if (end < min)
				{
					end = max;
				}
				++offset;
			}
		}
		
		return end;
	}

    public static Quaternion GetRotationFromMatix(Matrix4x4 m)
    {
        float qw = Mathf.Sqrt(1f + m.m00 + m.m11 + m.m22) / 2.0f;
        float w = 4.0f * qw;
        float qx = (m.m21 - m.m12) / w;
        float qy = (m.m02 - m.m20) / w;
        float qz = (m.m10 - m.m01) / w;
        return new Quaternion(qx, qy, qz, qw);
    }
	

    public static float HueToRGB(float p, float q, float t)
    {
    	if(t < 0f) 
    		t += 1f;
        if(t > 1f) 
        	t -= 1f;
        if(t < 1f/6f) 
        	return p + (q - p) * 6f * t;
        if(t < 1f/2f) 
        	return q;
        if(t < 2f/3f) 
        	return p + (q - p) * (2f/3f - t) * 6f;
        return p;
    }
    // adapted from http://en.wikipedia.org/wiki/HSL_color_space

    public static Color HSLtoRGB(float h, float s, float l)
    {
    	Color output = Color.black;
    	if (s < 0.01f)
    	{
    		return output;
    	}

        float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
        float p = 2f * l - q;
        output.r = HueToRGB(p, q, h + 1f/3f);
        output.g = HueToRGB(p, q, h);
        output.b = HueToRGB(p, q, h - 1f/3f);
        return output;
    }
}

