﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid {

	public TextAsset source;
	public int sizeX, sizeY;
	public Cell[,] cells;
	public Dictionary<GameObject, Cell> worldObjects;
	public GameObject cellBase;
    public GameObject gridHolder;
    public GameLogic game;
    public List<Cell> selectedCells;

	public void initialisation (int x, int y) {
		cellBase = Resources.Load<GameObject> ("Prefabs/CellBase");
		worldObjects = new Dictionary<GameObject, Cell> ();
		this.sizeX = x;
		this.sizeY = y;
		initPosition ();
		initDistance ();
	}

    public void clearGrid() {
        GameObject.Destroy(gridHolder);
    }

	private void initPosition () {
        gridHolder = GameObject.Instantiate(Resources.Load<GameObject> ("Prefabs/GridHolder"), new Vector3(0, 0, 0), Quaternion.identity);
        gridHolder.name = "GridHolder";
        cells = new Cell[sizeX, sizeY];
		for (int y = 0; y < sizeY; y++) {
			for (int x = 0; x < sizeX; x++) {
				GameObject o = GameObject.Instantiate (cellBase, new Vector3 (x, 0, y), Quaternion.identity);
                o.transform.parent = gridHolder.transform;
				Cell c = new Cell (sizeX, sizeY, o);
                c.x = x;
				c.y = y;
				c.id = x + y * sizeX;
				cells [x, y] = c;
				c.inWorld = o;
                c.grid = this;
                c.game = game;
                o.name = "Cell #" + c.id;
                worldObjects.Add (o, c);
			}
		}

		for (int y = 0; y < sizeY; y++) {
			for (int x = 0; x < sizeX; x++) {
				Cell c = cells [x, y];
				c.id = x + sizeX * y;
				if (c.y + 1 < sizeY) {
					c.adjacent [(int)Direction.NORTH] = cells [x, y + 1];
				}
				if (c.y - 1 >= 0) {
					c.adjacent [(int)Direction.SOUTH] = cells [x, y - 1];
				}
				if (c.x + 1 < sizeX) {
					c.adjacent [(int)Direction.EAST] = cells [x + 1, y];
				}
				if (c.x - 1 >= 0) {
					c.adjacent [(int)Direction.WEST] = cells [x - 1, y];
				}
			}
		}
	}

	private void initDistance () {
		for (int y = 0; y < sizeY; y++)
			for (int x = 0; x < sizeX; x++)
				setDistanceForPosition (cells [x, y], x, y, 0);
	}

	private void setDistanceForPosition (Cell c, int x, int y, int distance) {
		if (c.distance [x, y] == -1 || distance < c.distance [x, y]) {
			c.distance [x, y] = distance;
			for (int a = 0; a < (int)Direction.COUNT; a++) {
				if (c.adjacent [a] != null) {
					setDistanceForPosition (c.adjacent [a], x, y, distance + 1);
				}
			}
		}
	}

	public void printGrid () {
		for (int y = 0; y < sizeY; y++) {
			for (int x = 0; x < sizeX; x++) {
				Debug.Log ("  " + x + ", " + y + "  ");
			}
			Debug.Log ("\n");
		}
	}

	public List<Cell> getInRange (Cell origin, int minRange, int maxRange) {

		List<Cell> output = new List<Cell> ();
		for (int y = 0; y < sizeY; y++) {
			for (int x = 0; x < sizeX; x++) {
				if (origin.distance [x, y] >= minRange && origin.distance [x, y] <= maxRange) {
					output.Add (cells [x, y]);
				}
			}
		}

		return output;
	}

	public List<Cell> getInLine (Cell origin, int minRange, int maxRange) {

		List<Cell> output = new List<Cell> ();
		for (int dir = 0; dir < (int)Direction.COUNT; dir++) {
			Cell current = origin;
			int i = 0;
			while (i <= maxRange && current != null) {
				if (i >= minRange)
					output.Add (current);
				current = current.adjacent [dir];
				i++;
			}
		}

		return output;
	}

	public void clickCell (GameObject cellObject) {
		ClearSelection ();
		Cell cell;
		if (worldObjects.TryGetValue (cellObject, out cell)) {
			if (cell != null) {
				selectedCells = getInLine (cell, 2, 4);
				if(NetworkMasterClient.singleton != null)
					NetworkMasterClient.singleton.MovementOrder(cell.id, GameLogicClient.localPlayer.entity.entityId);
			}

		}

	}

	public void ClearSelection () {
		if (selectedCells != null) {

		}
		selectedCells = null;
	}

	public Cell GetCell (int cellId) {
		return cells [cellId % sizeX, cellId / sizeX];
	}

	public Cell GetCell (int x, int y) {
		return cells [x, y];
	}

	public void SetCellColor (int cellId, Color color) {
		SetCellColor (GetCell (cellId), color);
	}

	public void SetCellColor (int x, int y, Color color) {
		SetCellColor (GetCell (x, y), color);
	}

	public void SetCellColor (Cell c, Color color) {
		c.inWorld.GetComponent<MeshRenderer> ().material.color = color;
	}

	public void SetCellColor (int [] cellId, Color color) {
		for(int i = 0; i < cellId.Length; i++)
			SetCellColor (GetCell (cellId[i]), color);
	}
}
