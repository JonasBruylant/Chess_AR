using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Sprite/SpriteDictionary")]
public class SpriteDictionary : ScriptableObject
{
    [Serializable]
    public class SpriteEntry
    {
        public TeamColor color;
        public PieceType pieceType;
        public Sprite spriteImage;
    }

    public List<SpriteEntry> sprites;

    public Sprite GetSprite(TeamColor color, PieceType pieceType)
    {
        for (int i = 0; i < sprites.Count; ++i)
        {
            if (sprites[i].color != color || sprites[i].pieceType != pieceType)
                continue;

            return sprites[i].spriteImage;
        }
        return null;
    }
}
