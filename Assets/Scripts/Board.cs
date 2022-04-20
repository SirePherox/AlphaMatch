using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using System;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;

    public Tile[,] Tiles { get; private set; }

    public int width => Tiles.GetLength(0) ;
    public int heigth => Tiles.GetLength(1);

    private const float TweenDuration = 0.25f;

    private readonly List<Tile> _selection = new List<Tile>();

    public void Awake() => Instance = this;

    private void Start()
    {
        Tiles = new Tile[rows.Max(row => row.tiles.Length ),rows.Length];

        for (var y = 0; y < heigth; y++)
        {
            for(var x = 0; x < width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                //Random is an ambigous reference between System.Random and UnityEngine.Random
                tile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0,ItemDatabase.Items.Length)];

                Tiles[x, y] = tile;
            }
        }
    }
    public async void Select(Tile tile)
    {
        if(!_selection.Contains(tile))
        {
            _selection.Add(tile);
        }

        if (_selection.Count < 2) return;

        Debug.Log($"Message is {_selection[0].x} , {_selection[0].y} and {_selection[1].x} , {_selection[1].y} ");

        await Swap(_selection[0], _selection[1]);

        if (CanPop())
        {
            Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }

        _selection.Clear();
    }

    public async Task Swap (Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
                .Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Item;
        tile1.Item = tile2.Item;
        tile2.Item = tile1Item;
    }

    private bool CanPop()
    {
        for(var y = 0; y < heigth; y++)
        {
            for(var x = 0; x < width; x++)
            {
                if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2) return true;
            }
        }
        return false;
    }

    private async void Pop()
    {
        for(var y = 0; y < heigth; y++)
        {
            for(var x = 0; x < width; x++)
            {
                var tiles = Tiles[x, y];
                var connectedTiles = tiles.GetConnectedTiles();
                if (connectedTiles.Skip(1).Count() < 2) continue;

                var deflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles) {
                    deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));
                }

                await deflateSequence.Play()
                    .AsyncWaitForCompletion();

                var inflateSequence = DOTween.Sequence();
                foreach ( var connectedTile in connectedTiles)
                {
                    //random is an ambigous reference between system.random and unityengine.random
                    connectedTile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

                    
                    inflateSequence.Join( connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
                }

                await inflateSequence.Play().AsyncWaitForCompletion();

                
            }
        }
    }


}
