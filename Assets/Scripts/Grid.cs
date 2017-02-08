/**
 * This script creates a grid of tiles. Each tile can represent a brick or an
 * empty block witch can be filled with water. 
 * Note : The (0, 0) corresponds to the lower left tile.
 * Then, it computes, for each tile that doesn't represent a brick, how it should
 * or not transfer the eventual amount of water it contains to its neighbors.
 * 
 * Author : Martin Genet
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
	public GameObject _tilePrefab;

	// List of Tile representing a grid of size (_colSize*_rowSize)
	public static List<Tile> GRID;
	public static int _colSize = 20;		// Number of tiles per column
	public static int _rowSize = 48;		// Number of tiles per row
	private float _tileSize;

	// Script is updated once per n seconds
	public static float TICK_RATE = 0.0025f;
	private float _timer;

	// How much of the difference between two water levels should be transfered (actually, anything else than the half
	// is strange in the end)
	private float TRANSFER_FACTOR = 0.5f;

	// START & UPDATE
	// ------------------------------------------------------------------------------------------------------------

	// Use this for initialization
	void Start () {
		// Get the size of a tile
		_tileSize = _tilePrefab.transform.localScale.x;
		// Initialize GRID
		GRID = new List<Tile>();
		// Create a grid of tiles
		CreateTiles ();

		LaunchTimer ();
	}

	// Update is called once per frame
	void Update () {
		if (TimerFinished ()) {
			WaterFlow ();
			LaunchTimer ();
		}
	}

	// TIME & FRAME MANAGEMENT
	// ------------------------------------------------------------------------------------------------------------

	void LaunchTimer(){
		_timer = TICK_RATE;
	}

	bool TimerFinished(){
		_timer -= Time.deltaTime;
		return (_timer <= 0.0f);
	}

	// GRID CREATION & ACCESS FUNCTIONS
	// ------------------------------------------------------------------------------------------------------------

	/**
	 * If x and y are correct, return the Tile object attached to the block at
	 * the index representing the x-th element of the y-th line (the GRID is
	 * a 1D list). If not, return null.
	 **/
	public static Tile Get(int x, int y) {
		if (x < 0 || x >= _rowSize || y < 0 || y >= _colSize) {
			return null;
		}
		// return the GameObject at the index representing the x-th element
		// of the y-th line
		return GRID[y * _rowSize + x];
	}

	/**
	 * Create the (_colSize * _rowSize) tiles, by placing new instances
	 * of the Tile prefab, and add each reference to the GRID list.
	 **/
	void CreateTiles() {
		float xOffset = 0.0f;
		float yOffset = 0.0f - _tileSize;
		int x = -1, y = -1;

		int nbrOfTiles; // Number of tiles created
		GameObject clone;
		Tile newTile;

		for (nbrOfTiles = 0; nbrOfTiles < _colSize * _rowSize; nbrOfTiles += 1) {
			xOffset += _tileSize;
			x += 1;

			// When end of a row is reached, move up (will also happen on first loop)
			if (nbrOfTiles % _rowSize == 0) {
				yOffset += _tileSize;
				xOffset = 0.0f;
				y += 1;
				x = 0;
			}

			// Instantiate a prefab clone and add it to the GRID
			clone = (GameObject) Instantiate (_tilePrefab, 
				new Vector3 (transform.position.x + xOffset, transform.position.y + yOffset, transform.position.z), 
				transform.rotation);

			newTile = clone.GetComponent<Tile> ();
			newTile.x = x;
			newTile.y = y;
			GRID.Add (newTile);
		}
	}

	// WATER MANAGEMENT FUNCTIONS
	// ------------------------------------------------------------------------------------------------------------

	/**
	 * Compute the water movements for each tile.
	 **/
	void WaterFlow () {
		int x, y;
		for (x = 0; x < _rowSize; x++) {
			for (y = 0; y < _colSize; y++) {
				ComputeTile (x, y);
			}
		}
	}

	/**
	 * Compute what to do with the x-th tile of the y-th line.
	 * If x and y do not correspond to an existing tile, return 
	 * false. Else, return true after computing the water level.
	 **/
	bool ComputeTile (int x, int y){
		Tile tile = Get (x, y);
		bool res;
		// This shouldn't happen, but if (x, y) isn't correct, return false
		if (tile == null) {
			return false;
		}

		// If the tile is a brick or is totally empty, then ignore and return true
		if (tile.CurrentState == Tile.TileState.Brick || !(tile.CurrentWaterLevel > 0.0f)) {
			return true;
		}

		// First try to move some water down
		res = TryDown (tile);
		// If no water could be transfered down OR there is still some water in the tile
		if(!res || tile.CurrentWaterLevel > 0.0f){
			// then try to transfer water to the left and to the right
			TrySideways (tile);
		}

		return true;
	}

	/**
	 * Try to transfer as much water as possible to the tile under the current one, 
	 * which is refered by currentTile and is at least partially filled with water (see ComputeTile()).
	 * Return false if no water was transfered, or true if water could flow down.
	 **/
	bool TryDown (Tile currentTile){
		Tile down;
		float waterPortion;

		down = Get (currentTile.x, currentTile.y-1);
		// If we reached out of the grid, just empty the current tile and return true
		if (down == null) {
			currentTile.CurrentWaterLevel = 0.0f;
			return true;
		}

		// If the tile down is a brick or is full of water, return false
		if (down.CurrentState == Tile.TileState.Brick || !(down.CurrentWaterLevel < 1.0f)) {
			return false;
		}

		// What is needed to fill the tile down
		waterPortion = 1.0f - down.CurrentWaterLevel;
		// If the current tile's water level isn't important enough to fill it
		if (currentTile.CurrentWaterLevel < waterPortion) {
			// then the water portion to transfer is the current tile's own level
			waterPortion = currentTile.CurrentWaterLevel;
		}

		// Move it frome the current tile to the tile down and return true
		currentTile.CurrentWaterLevel -= waterPortion;
		down.CurrentWaterLevel += waterPortion;
		return true;
	}

	/**
	 * Try to transfer part of the current tile's water level to the left and to the right. 
	 **/
	void TrySideways(Tile currentTile){
		float diffLeft, diffRight;
		Tile left, right;
		bool noLeft, noRight;

		left = Get (currentTile.x - 1, currentTile.y);
		right = Get (currentTile.x + 1, currentTile.y);

		// Don't mess with the void
		noLeft = (left == null);
		noRight = (right == null);

		// Provided the tile left exists, 
		// if it is a brick or its water level is higher, balance with right only
		if (!noLeft && (left.CurrentState == Tile.TileState.Brick || left.CurrentWaterLevel > currentTile.CurrentWaterLevel)) {
			Balance (currentTile, right);
		}

		// Provided the tile right exists, 
		// if it is a brick or its water level is higher, balance with left only
		if (!noRight && (right.CurrentState == Tile.TileState.Brick || right.CurrentWaterLevel > currentTile.CurrentWaterLevel)) {
			Balance (currentTile, left);
		}

		// Here we need to balance the 3 tiles together, knowing that current tile has the highest water level

		// First, if current tile is near grid's border, 
		// drop water into the void before balancing level with the existing tile
		if (noLeft) {
			Balance (currentTile, left);
			Balance (currentTile, right);
		} else if (noRight) {
			Balance (currentTile, right);
			Balance (currentTile, left);
		} else {
			// Compute the water level difference between current tile and both left and right
			diffLeft = currentTile.CurrentWaterLevel - left.CurrentWaterLevel;
			diffRight = currentTile.CurrentWaterLevel - right.CurrentWaterLevel;

			// Balance with priority to the side which has the highest level difference (e.g. the lowest level)
			if (diffLeft > diffRight) {
				Balance3 (currentTile, left, right);
			} else {
				Balance3 (currentTile, right, left);
			}
		}
	}

	/**
	 * Balance water level between two tiles only : try to transfer water from source to target.
	 * Return false if :
	 * 	- target is a brick
	 * 	- target's water level is greater or equal to source's
	 * Return value isn't used for now, but may be useful I guess.
	 **/
	bool Balance(Tile source, Tile target){
		// If target doesn't exist, transfer water to the void and return true
		if (target == null) {
			if (source.CurrentWaterLevel <= 0.1f) {
				source.CurrentWaterLevel = 0.0f;
			} else {
				source.CurrentWaterLevel = source.CurrentWaterLevel / 2;
			}

			return true;
		}

		// If the target is a brick, return false
		if (target.CurrentState == Tile.TileState.Brick) {
			return false;
		}

		// If the target's water level is greater or equal to source's, return false
		if (target.CurrentWaterLevel >= source.CurrentWaterLevel) {
			return false;
		}

		// Now, transfer what is needed to balance levels and return true
		float diff = source.CurrentWaterLevel - target.CurrentWaterLevel;
		source.CurrentWaterLevel -= diff * TRANSFER_FACTOR;
		target.CurrentWaterLevel += diff * TRANSFER_FACTOR;

		return true;
	}

	/**
	 * Knowing that current tile has highest water level, balance the three tiles's level,
	 * giving priority to lowLevelTile, checking for highLevelTile afterwards.
	 **/
	void Balance3(Tile currentTile, Tile lowLevelTile, Tile highLevelTile){
		if (lowLevelTile.CurrentState == Tile.TileState.Brick || highLevelTile.CurrentState == Tile.TileState.Brick) {
			return;
		}
		float diff = currentTile.CurrentWaterLevel - lowLevelTile.CurrentWaterLevel;
		currentTile.CurrentWaterLevel -= diff * TRANSFER_FACTOR;
		lowLevelTile.CurrentWaterLevel += diff * TRANSFER_FACTOR;
		if (currentTile.CurrentWaterLevel > highLevelTile.CurrentWaterLevel) {
			diff = currentTile.CurrentWaterLevel - highLevelTile.CurrentWaterLevel;
			currentTile.CurrentWaterLevel -= diff * TRANSFER_FACTOR;
			highLevelTile.CurrentWaterLevel += diff * TRANSFER_FACTOR;
		}
	}
}
