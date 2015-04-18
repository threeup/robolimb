using UnityEngine;
using System.Collections.Generic;


public enum Facing
{
	posZ,		// Facing 0 - shared edge of octants 6 & 5, EAST
	posZ_posX,	// Facing 1 - shared edge of octants 5 & 4, SOUTHEAST
	posX,		// Facing 2 - shared edge of octants 4 & 3, SOUTH
	negZ_posX,	// Facing 3 - shared edge of octants 3 & 2, SOUTHWEST
	negZ,		// Facing 4 - shared edge of octants 2 & 1, WEST
	negZ_negX,	// Facing 5 - shared edge of octants 1 & 8, NORTHWEST
	negX,		// Facing 6 - shared edge of octants 8 & 7, NORTH
	negX_posZ,	// Facing 7 - shared edge of octants 7 & 6, NORTHEAST
	Error,
	Auto
}

public enum OrdinalDir
{
	NW, N, NE, E, SE, S, SW, W,
} 

public class IsoMath
{
	static public byte BIGBYTE = (byte)(byte.MaxValue - 16);
	static public byte MAXBYTE = byte.MaxValue;
	static public int BIGINT = int.MaxValue - 1024;
	static public int HUGEINT = int.MaxValue - 256;
	static public int MAXINT = int.MaxValue;
	static public float BIGFLOAT = float.MaxValue - 4092;
	static public float HUGEFLOAT = float.MaxValue - 1024; 
	static public float MAXFLOAT = float.MaxValue;
	static public object Unused(object a)
	{
		return a;
	}

#region BASIC MATH

	static public int IntMin(int a, int b)
	{
		return a > b ? b : a;
	}

	static public int IntMax(int a, int b)
	{
		return a > b ? a : b;
	}

	static public byte ByteMin(byte a, byte b)
	{
		return a > b ? b : a;
	}

	static public byte ByteMax(byte a, byte b)
	{
		return a > b ? a : b;
	}

	/// <summary>
	/// Increments with a max which causes a wrap around
	/// </summary>
	/// <returns>
	/// true if it wraps around from hitting the maximum
	/// </returns>
	static public bool Increment(ref int value, int maximum)
	{
		value++;
        if (value >= maximum)
        {
            value = 0;
            return true;
        }
        return false;
	}
#endregion

#region FACING FUNCTIONS

	/// <summary>
	/// Determine if vectors are close to each other on x and z
	/// </summary>
	public static bool AlmostSame(Vector3 posA, Vector3 posB)
	{
		float deltaX = posA.x-posB.x;
		float deltaZ = posA.z-posB.z;
		return (deltaX*deltaX + deltaZ*deltaZ) < 0.01f*0.01f;
		//return Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.z - posB.z) < 0.01f;
	}
	
	// FacingVectors is for converting to vector3
	// mapped to public enum Facing
	public static Vector3[] FacingVectors = new Vector3[]
	{
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(1f, 0f,-1f),
		new Vector3(0f, 0f, -1f),
		new Vector3(-1f, 0f, -1f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(-1f, 0f, 1f)
	};


	public static Vector3 getDirectionFromFacing(Facing face)
	{
		if (face == Facing.Error || face == Facing.Auto)
		{
			return Vector3.zero;
		}
		return FacingVectors[(int)face];
	}
	public static Facing getFacingFromDirection(Vector3 facing)
	{
		float one = 0.98f;

		Facing f = Facing.Error;
		if (Mathf.Abs(facing.x) >= one)
		{
			f = (facing.x > 0f) ? Facing.posX : Facing.negX;
		}
		else if (Mathf.Abs(facing.z) >= one)
		{
			f = (facing.z > 0f) ? Facing.posZ : Facing.negZ;
		}
		else
		{
			if (facing.x > 0f && facing.z > 0f)
				f = Facing.posZ_posX;
			else if (facing.x > 0f && facing.z < 0f)
				f = Facing.negZ_posX;
			else if (facing.x < 0f && facing.z < 0f)
				f = Facing.negZ_negX;
			else if (facing.x < 0f && facing.z > 0f)
				f = Facing.negX_posZ;
		}
		return f;
	}
#endregion

#region ORDINAL FUNCTIONS
	// For NW, the first choice is NW=0, then N=1, then W=7
	// For N, the order is N NE NW E W ....
	public static int[][] BestOrdinal = new int[][]{
			new int[] {0, 1, 7, 2, 6, 3, 5, 4},  
			new int[] {1, 2, 0, 3, 7, 4, 6, 5},
			new int[] {2, 3, 1, 4, 0, 5, 7, 6},
			new int[] {3, 4, 2, 5, 1, 6, 0, 7},
			new int[] {4, 3, 5, 2, 6, 1, 7, 0},
			new int[] {5, 6, 4, 7, 3, 0, 2, 1},
			new int[] {6, 7, 5, 0, 4, 1, 3, 2},
			new int[] {7, 0, 6, 1, 5, 2, 4, 3}};


	public static int[] OrdinalStepX = new int[] { 1, 0, -1, -1, -1, 0, 1, 1 };
	public static int[] OrdinalStepY = new int[] { -1, -1, -1, 0, 1, 1, 1, 0 };
		
	// This cost is to reduce diagonal zigzaging
	public static float[] OrdinalStepCost = new float[] { 1f, 0f, 1f, 0f, 1f, 0f, 1f, 0f };


		// NE is pi/8 22.5 to 3pi/8 67.5, a range of 45degrees
	private static float ORDINALTHRESHOLD = Mathf.Cos(Mathf.PI/8f) - Mathf.Sin(Mathf.PI/8f);
	
	public static OrdinalDir ToOrdinal(Facing facing)
	{
		switch(facing)
		{
			case Facing.posZ: return OrdinalDir.S;
			case Facing.posZ_posX: return OrdinalDir.SW;
			case Facing.posX: return OrdinalDir.W;
			case Facing.negZ_posX: return OrdinalDir.NW;
			case Facing.negZ: return OrdinalDir.N;
			case Facing.negZ_negX: return OrdinalDir.NE;
			case Facing.negX: return OrdinalDir.E;
			case Facing.negX_posZ: return OrdinalDir.SE;
		}
		return OrdinalDir.N;
	}

	public static OrdinalDir ToOrdinal(Vector2 dir)
	{
		if (Mathf.Abs(Mathf.Abs(dir.x) - Mathf.Abs(dir.y)) < ORDINALTHRESHOLD)
		{
			if (dir.x > 0)
			{
				return (dir.y > 0) ? OrdinalDir.SW : OrdinalDir.NW;
			}
			else
			{
				return (dir.y > 0) ? OrdinalDir.SE : OrdinalDir.NE;	
			}
		}
		else
		{
			if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
			{
				return (dir.x > 0) ? OrdinalDir.W : OrdinalDir.E;
			}
			else
			{
				return (dir.y > 0) ? OrdinalDir.S : OrdinalDir.N;	
			}	
		}
	}

	public static Vector2 ToVector(OrdinalDir dir)
	{
		int dirInt = (int)dir;
		if (dirInt >= 0 && dirInt <= 7)
		{
			return new Vector2(OrdinalStepX[dirInt], OrdinalStepY[dirInt]);
		}
		return Vector2.zero;
	}

	public static OrdinalDir OrdinalAdd(OrdinalDir first, OrdinalDir second)
	{
		int result = (int)first + (int)second;
		if (result >= 8)
		{
			result -= 8;
		}
		return (OrdinalDir)result;
	}

	public static int OrdinalSubtract(OrdinalDir first, OrdinalDir second)
	{
		
		// 1 - 8 = -7, -7 +8 ==1

		int result = ((int)first - (int)second);
		if (result < 0)
		{
			result += 8;
		}
		return result;
	}

	public static int OrdinalSubtractAbs(OrdinalDir first, OrdinalDir second)
	{
		// 8 - 1 = 7,  
		// 1 - 8 = -7, -7 +8 ==1

		int diffLeft = ((int)first - (int)second);
		int diffRight = ((int)second - (int)first);
		if (diffLeft < 0)
		{
			diffLeft += 8;
		}
		if (diffRight < 0)
		{
			diffRight += 8;
		}
		return Mathf.Min(diffLeft, diffRight);
	}

	public static OrdinalDir ToStraightOrdinal(Vector2 dir)
	{
		if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
		{
			return (dir.x > 0) ? OrdinalDir.W : OrdinalDir.E;
		}
		else
		{
			return (dir.y > 0) ? OrdinalDir.S : OrdinalDir.N;	
		}	
	}

	public static OrdinalDir ToStraightSecondOrdinal(Vector2 dir)
	{
		if (Mathf.Abs(dir.x) <= Mathf.Abs(dir.y))
		{
			return (dir.x > 0) ? OrdinalDir.W : OrdinalDir.E;
		}
		else
		{
			return (dir.y > 0) ? OrdinalDir.S : OrdinalDir.N;	
		}	
	}

	public static OrdinalDir ToSecondOrdinal(Vector2 dir)
	{
		OrdinalDir first = ToOrdinal(dir);
		bool aboveDiagonal = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);
		switch(first)
		{
			default:
			case OrdinalDir.S: return (dir.x > 0) ? OrdinalDir.SW : OrdinalDir.SE;
			case OrdinalDir.N: return (dir.x > 0) ? OrdinalDir.NW : OrdinalDir.NE;
			case OrdinalDir.W: return (dir.y > 0) ? OrdinalDir.SW : OrdinalDir.NW;
			case OrdinalDir.E: return (dir.y > 0) ? OrdinalDir.SE : OrdinalDir.NE;

			case OrdinalDir.NE: return (aboveDiagonal) ? OrdinalDir.N : OrdinalDir.E;
			case OrdinalDir.NW: return (aboveDiagonal) ? OrdinalDir.N : OrdinalDir.W;
			case OrdinalDir.SE: return (aboveDiagonal) ? OrdinalDir.S : OrdinalDir.E;
			case OrdinalDir.SW: return (aboveDiagonal) ? OrdinalDir.S : OrdinalDir.W;
		}

	}

	public static OrdinalDir OrdinalReverse(OrdinalDir dir)
	{
		switch(dir)
		{
			default:
			case OrdinalDir.S: return OrdinalDir.N;
			case OrdinalDir.N: return OrdinalDir.S;
			case OrdinalDir.W: return OrdinalDir.E;
			case OrdinalDir.E: return OrdinalDir.W;

			case OrdinalDir.NE: return OrdinalDir.SW;
			case OrdinalDir.SW: return OrdinalDir.NE;
			case OrdinalDir.NW: return OrdinalDir.SE;
			case OrdinalDir.SE: return OrdinalDir.NW;
		}

	}

	public static void SplitIntoOrdinals(Vector2 dir, float overlap, out OrdinalDir best, out OrdinalDir next)
	{
		// best
		// 0 to 22 = N
		// 22 to 45 = NE 
		// 45 to 67 = NE
		// 67 to 90 = E

		// next
		// if overlap 12
		// 0 to 10 = N
		// 10 to 22 = NE
		// 22 to 35 = N
		// 35 to 45 = NE
		// 45 to 55 = NE
		// 55 to 67 = E
		// 67 to 80 = NE
		// 80 to 90 = E
		
		// North is top right
		//OrdinalDir primaryY = dir.y > 0 ? OrdinalDir.W : OrdinalDir.E;
		//OrdinalDir primaryX = dir.x > 0 ? OrdinalDir.N : OrdinalDir.S;
		//OrdinalDir diagonal = dir.y > 0 ? (dir.x > 0 ? OrdinalDir.NW : OrdinalDir.SW) : (dir.x > 0 ? OrdinalDir.NE : OrdinalDir.SE);

		// North is top left
		OrdinalDir primaryY = dir.y > 0 ? OrdinalDir.N : OrdinalDir.S;
		OrdinalDir primaryX = dir.x > 0 ? OrdinalDir.E : OrdinalDir.W;
		OrdinalDir diagonal = dir.y > 0 ? (dir.x > 0 ? OrdinalDir.NE : OrdinalDir.NW) : (dir.x > 0 ? OrdinalDir.SE : OrdinalDir.SW);

		float twentytwo = 22.5f;
		float lowLine = 22.5f-overlap;
		float highLine = 22.5f+overlap;
		float absY = Mathf.Abs(dir.y);
		float absX = Mathf.Abs(dir.x);
		float degY = Mathf.Asin (absY)*180f/Mathf.PI;
		float degX = Mathf.Asin (absX)*180f/Mathf.PI;
		bool doDebug = false;
		if (doDebug)
		{
			Debug.Log (dir.y+" degY"+degY);
			Debug.Log (dir.x+" degX"+degX);
		}
		if (degY < twentytwo)
		{
			best = primaryX;
			next = (degY < lowLine) ? primaryX : diagonal;
			if (doDebug) { Debug.Log("1="+best+" "+next); }
		}
		else if (degX < twentytwo) // 67 to 90
		{
			best = primaryY;
			next = (degX < lowLine) ? primaryY : diagonal;
			if (doDebug) { Debug.Log("4="+best+" "+next); }
		}
		else if (degY < degX) // 22 to 45
		{
			best = diagonal;
			next = (degY < highLine) ? primaryX : diagonal;
			if (doDebug) { Debug.Log("2="+best+" "+next); }
		}
		else //45 to 67
		{
			best = diagonal;
			next = (degX < highLine) ? primaryY : diagonal;
			if (doDebug) { Debug.Log("3="+best+" "+next); }
		}
		
	}
#endregion

	public static float[] ParabolizeRange(float[] rangeMods)
	{
		int mid = (int)Mathf.Round(rangeMods.Length*0.5f);
		float[] adjustedMods = new float[rangeMods.Length];
		for(int i=0; i<rangeMods.Length; ++i)
		{
			adjustedMods[i] = 5*rangeMods[i] + 10 - ((i-mid)*(i-mid));
		}
		return adjustedMods;
	}

	public static float[] NormalizeRange(float[] rangeMods)
	{
		float bestValue = 0f;
		int zeroIdx = rangeMods.Length;
		for(int i=0; i<rangeMods.Length; ++i)
		{
			if (rangeMods[i] > bestValue)
			{
				bestValue = rangeMods[i];
			}
			if (zeroIdx == rangeMods.Length && rangeMods[i] == 0)
			{
				zeroIdx = i;
			}
		}
				
		float[] adjustedMods = new float[rangeMods.Length];
		for(int i=0; i<rangeMods.Length; ++i)
		{
			if (bestValue > 0.1f)
			{
				adjustedMods[i] = rangeMods[i]/bestValue;	
			}
			else
			{
				adjustedMods[i] = rangeMods[i];
			}
		}
		return adjustedMods;
	}

	public static float[] SanitizeRange(float[] rangeMods)
	{
		float min = 99999;
		for(int i=0; i<rangeMods.Length; ++i)
		{
			if (rangeMods[i] < min)
			{
				min = rangeMods[i];
			}
		}
		float[] adjustedMods = new float[rangeMods.Length];
		for(int i=0; i<rangeMods.Length; ++i)
		{
			adjustedMods[i] = rangeMods[i]-min+1;
		}
		return adjustedMods;
	}
	
	public static int CalcShotgunBlastRadius(int distance, float spreadAngle)
	{
		return Mathf.FloorToInt((float)distance * Mathf.Tan(spreadAngle * Mathf.Deg2Rad));
	}
	public static int ArrIdx(int x, int y, int w)
	{
		return x + y*w;
	}
	public static string FloatFToString(float f)
	{
	    return f.ToString(); 
	}
	public static void Shuffle<T>(ref List<T> list)  
	{  
	    int n = list.Count;  
	    while (n > 1) {  
	        n--;  
	        int k = UnityEngine.Random.Range(0,n + 1);  
	        T value = list[k];  
	        list[k] = list[n];  
	        list[n] = value;  
	    }  
	}
	
	public static int LevenshteinDistance(string s, string t)
	{
	    // degenerate cases
	    if (s == t) 
			return 0;
	    if (s.Length == 0) 
			return t.Length;
	    if (t.Length == 0) 
			return s.Length;
	 
	    // create two work vectors of integer distances
	    int[] v0 = new int[t.Length + 1];
	    int[] v1 = new int[t.Length + 1];
	 
	    // initialize v0 (the previous row of distances)
	    // this row is A[0][i]: edit distance for an empty s
	    // the distance is just the number of characters to delete from t
	    for (int i = 0; i < v0.Length; i++)
	        v0[i] = i;
	 
	    for (int i = 0; i < s.Length; i++)
	    {
	        // calculate v1 (current row distances) from the previous row v0
	 
	        // first element of v1 is A[i+1][0]
	        //   edit distance is delete (i+1) chars from s to match empty t
	        v1[0] = i + 1;
	 
	        // use formula to fill in the rest of the row
	        for (int j = 0; j < t.Length; j++)
	        {
	            var cost = (s[i] == t[j]) ? 0 : 1;
	            v1[j + 1] = Minimum(v1[j] + 1, v0[j + 1] + 1, v0[j] + cost);
	        }
	 
	        // copy v1 (current row) to v0 (previous row) for next interation
	        for (int j = 0; j < v0.Length; j++)
	            v0[j] = v1[j];
	    }
	 
	    return v1[t.Length];
	}
	private static int Minimum(int a, int b, int c)
    {
        int m = a;
		if (b < m)
			m = b;
        if (c < m)
			m = c;
        return m;		
		//return a < b ? (a < c ? a : c) : (b < c ? b : c); 
    }
	//public static IEnumerable<T> Randomize<T>(IEnumerable<T> source)
	//{
	    //UnityEngine.Random rnd = new UnityEngine.Random();
	    //return source.OrderBy<T, int>((item) => rnd.Next());
	//}
}