using UnityEngine;

public static class LayerHelper
{
	static int EVERYTHING = -1;
	static int WALL = -1;
	static int PROPS = -1;
	static int GROUND = -1;
	static int PLAYER_LAYER = -1;
	static int HIDDEN_BY_FOW_LAYER = -1;
	static int HIDDEN_LAYER = -1;
	static int INTERACTABLE_LAYER = -1;
	static int IGNORERAYCAST_LAYER = -1;
	static int UI_VISIBLE = -1;
	static int UI_HIDDEN = -1;
	static int UI_3D = -1;
    static int CHARACTER_DISC = -1;
    static int VFX_LAYER = -1;
    static int DEFAULT = 0;

    public static int Default
    {
        get { return DEFAULT; }
    }

	public static int Everything {
		get	{
			if (EVERYTHING == -1) SetupLayerInfo();				
			return EVERYTHING;
		}
	}
	
	public static int Wall {
		get	{
			if (WALL == -1) SetupLayerInfo();				
			return WALL;
		}
	}

	public static int Props {
		get	{
			if (PROPS == -1) SetupLayerInfo();				
			return PROPS;
		}
	}
	
	public static int Ground {
		get	{
			if (GROUND == -1) SetupLayerInfo();				
			return GROUND;
		}
	}
	
	public static int PlayerDefault {
		get	{
			if (PLAYER_LAYER == -1) SetupLayerInfo();				
			return PLAYER_LAYER;
		}
	}
	
	public static int PlayerHiddenByFOW {
		get	{
			if (HIDDEN_BY_FOW_LAYER == -1) SetupLayerInfo();				
			return HIDDEN_BY_FOW_LAYER;
		}
	}
	
	public static int PlayerHidden {
		get	{
			if (HIDDEN_LAYER == -1) SetupLayerInfo();				
			return HIDDEN_LAYER;
		}
	}
	
	public static int Interactable {
		get	{
			if (INTERACTABLE_LAYER == -1) SetupLayerInfo();				
			return INTERACTABLE_LAYER;
		}
	}
	
	public static int IgnoreRaycast {
		get	{
			if (IGNORERAYCAST_LAYER == -1) SetupLayerInfo();				
			return IGNORERAYCAST_LAYER;
		}
	}
	
	public static int UI {
		get	{
			if (UI_VISIBLE == -1) SetupLayerInfo();				
			return UI_VISIBLE;
		}
	}
	
	public static int UIHidden {
		get	{
			if (UI_HIDDEN == -1) SetupLayerInfo();				
			return UI_HIDDEN;
		}
	}
	
	public static int UI3D {
		get	{
			if (UI_3D == -1) SetupLayerInfo();				
			return UI_3D;
		}
	}

    public static int CharacterDisc {
        get	{
			if (CHARACTER_DISC == -1) SetupLayerInfo();
            return CHARACTER_DISC;
		}
    }

    public static int VFX {
        get	{
			if (VFX_LAYER == -1) SetupLayerInfo();
            return VFX_LAYER;
		}
    }
	
	static void SetupLayerInfo()
	{
		EVERYTHING = LayerMask.NameToLayer("Everything");
		
		WALL = LayerMask.NameToLayer("Wall"); 
		PROPS = LayerMask.NameToLayer("Props"); 
		GROUND = LayerMask.NameToLayer("GroundPlane");
		
		PLAYER_LAYER = LayerMask.NameToLayer("Player");
		HIDDEN_BY_FOW_LAYER = LayerMask.NameToLayer("Hidden by FOW");
		HIDDEN_LAYER = LayerMask.NameToLayer("Hidden");
		
		INTERACTABLE_LAYER = LayerMask.NameToLayer("Interactable");
		IGNORERAYCAST_LAYER = LayerMask.NameToLayer("Ignore Raycast");
		UI_VISIBLE = LayerMask.NameToLayer("UI");
		UI_HIDDEN = LayerMask.NameToLayer("UIHidden");
		UI_3D = LayerMask.NameToLayer("UI3D");
        CHARACTER_DISC = LayerMask.NameToLayer("CharacterDisc");
        VFX_LAYER = LayerMask.NameToLayer("VFX");
	}
}

