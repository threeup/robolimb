
public struct BitMask
{
	public int maskValue;

	public BitMask(int v)
	{
		maskValue = v;
	}

	public bool MatchMask(int idx)
	{
		return (maskValue & idx) > 0;
	}
	public bool HasBit(int idx)
	{
		return (maskValue & (1 << idx)) > 0;
	}
	public void SetBit(int idx, bool on = true)
	{
		if (on)
		{
			maskValue |= 1 << idx;
		}
		else
		{
			maskValue &= ~(1 << idx);	
		}
	}

    public void FlipBit(int idx)
    {
        SetBit(idx, !HasBit(idx));
    }


	public void Clear()
	{
		maskValue = 0;
	}

    public int CountBits(int max)
    {
        int sum = 0;
        for(int i=0;i<max;++i)
        {
            sum += HasBit(i) ? 1 : 0;
        }
        return sum;
    }
}
public struct BitMaskByte
{
    public byte maskValue;
    public BitMaskByte(byte v)
    {
        maskValue = v;
    }
    public bool MatchMask(int idx)
    {
        return (maskValue & idx) > 0;
    }
    public bool HasBit(int idx)
    {
        return (maskValue & (1 << idx)) > 0;
    }
    public void SetBit(byte idx, bool on = true)
    {
        if (on)
        {
            maskValue |= (byte)(1 << idx);
        }
        else
        {
            maskValue &= (byte)~(1 << idx);
        }
    }
    public void FlipBit(byte idx)
    {
        SetBit(idx, !HasBit(idx));
    }

    public void Clear()
    {
        maskValue = 0;
    }
}