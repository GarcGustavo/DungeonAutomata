using System;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts.Data;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	public class GridRoom : IComparable<GridRoom> {
		public List<Vector3Int> tiles;
		public List<Vector3Int> edgeTiles;
		public List<GridRoom> connectedRooms;
		public int roomSize;
		public bool isAccessibleFromMainRoom;
		public bool isMainRoom;

		public GridRoom() {
		}

		public GridRoom(List<Vector3Int> roomTiles, CellData[,] map) {
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<GridRoom>();

			edgeTiles = new List<Vector3Int>();
			foreach (Vector3Int tile in tiles) 
			{
				for (int x = tile.x-1; x <= tile.x+1; x++) 
				{
					for (int y = tile.y-1; y <= tile.y+1; y++) 
					{
						if (x == tile.x || y == tile.y) 
						{
							//Debug.Log("x: " + x + " y: " + y);
							if (x > 0 && x < map.GetLength(0) && y > 0 && y < map.GetLength(1)) 
							{
								if (map[x,y].cellType == CellTypes.Wall) 
								{
									edgeTiles.Add(tile);
								}
							}
						}
					}
				}
			}
		}

		public void SetAccessibleFromMainRoom() 
		{
			if (!isAccessibleFromMainRoom) 
			{
				isAccessibleFromMainRoom = true;
				foreach (GridRoom connectedRoom in connectedRooms) 
				{
					connectedRoom.SetAccessibleFromMainRoom();
				}
			}
		}

		public static void ConnectRooms(GridRoom roomA, GridRoom roomB) {
			if (roomA.isAccessibleFromMainRoom) {
				roomB.SetAccessibleFromMainRoom ();
			} else if (roomB.isAccessibleFromMainRoom) {
				roomA.SetAccessibleFromMainRoom();
			}
			roomA.connectedRooms.Add (roomB);
			roomB.connectedRooms.Add (roomA);
		}

		public bool IsConnected(GridRoom otherRoom) {
			return connectedRooms.Contains(otherRoom);
		}

		public int CompareTo(GridRoom otherRoom) {
			return otherRoom.roomSize.CompareTo (roomSize);
		}
	}
}