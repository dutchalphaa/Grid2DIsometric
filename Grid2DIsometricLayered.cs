using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid2DIsometricLayered<TGridObject>
{
  public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
  public class OnGridValueChangedEventArgs : EventArgs
  {
    public int x;
    public int y;
    public int z;
  }

  private int width;
  private int height;
  private int maxElevation;
  private float cellSize;
  private TGridObject[,,] gridArray;
  private Vector3 originPosition;
  public Grid2DIsometricLayered(int width, int height, int maxElevation, float cellSize, Vector3 originPosition, Func<Grid2DIsometricLayered<TGridObject>, int, int, int, TGridObject> createGridObject)
  {
    this.width = width;
    this.height = height;
    this.maxElevation = maxElevation;
    this.cellSize = cellSize;
    this.originPosition = originPosition;

    gridArray = new TGridObject[width, height, maxElevation];

    for (int x = 0; x < gridArray.GetLength(0); x++)
    {
      for (int y = 0; y < gridArray.GetLength(1); y++)
      {
        for (int z = 0; z < gridArray.GetLength(2); z++)
        {
          gridArray[x, y, z] = createGridObject(this, x, y, z);
        }
      }
    }

    //DebugGrid();
  }

  private void DebugGrid()
  {
    for (int x = 0; x < gridArray.GetLength(0); x++)
    {
      for (int y = 0; y < gridArray.GetLength(1); y++)
      {
        Debug.DrawLine(GetWorldPositionIsometric(x, y), GetWorldPositionIsometric(x, y + 1), Color.white, 100f);
        Debug.DrawLine(GetWorldPositionIsometric(x, y), GetWorldPositionIsometric(x + 1, y), Color.white, 100f);
      }
    }

    Debug.DrawLine(GetWorldPositionIsometric(0, height), GetWorldPositionIsometric(width, height), Color.white, 100f);
    Debug.DrawLine(GetWorldPositionIsometric(width, 0), GetWorldPositionIsometric(width, height), Color.white, 100f);
  }

  public void LogValues()
  {
    for (int x = 0; x < gridArray.GetLength(0); x++)
    {
      for (int y = 0; y < gridArray.GetLength(1); y++)
      {
        for (int z = 0; z < gridArray.GetLength(2); z++)
        {
          Debug.Log(x + ", " + y + " = " + gridArray[x, y, z].ToString());
        }
      }
    }
  }

  private Vector2 MakeIsometric(float x, float y)
  {
    return new Vector2(
      x - y,
      (x + y) / 2
    );
  }

  private Vector2 Make2D(float x, float y)
  {
    return new Vector2(
      (2 * y + x) / 2,
      (2 * y - x) / 2
    );
  }

  private Vector3 GetWorldPositionIsometric(float x, float y)
  {
    Vector2 XYIsometric = MakeIsometric(x, y);
    return GetWorldPosition(XYIsometric.x, XYIsometric.y);
  }

  private Vector3 GetWorldPosition(float x, float y)
  {
    return new Vector3(x, y) * cellSize + originPosition;
  }

  private Vector2Int GetXY(Vector3 worldPosition)
  {
    return new Vector2Int
    (
      Mathf.FloorToInt((worldPosition - originPosition).x / cellSize),
      Mathf.FloorToInt((worldPosition - originPosition).y / cellSize)
    );
  }

  private Vector2Int GetXYIsometric(Vector3 worldPosition)
  {
    Vector2 test = Make2D((worldPosition - originPosition).x, (worldPosition - originPosition).y);
    return new Vector2Int
    (
      Mathf.FloorToInt(test.x / cellSize),
      Mathf.FloorToInt(test.y / cellSize)
    );
  }

  public void SetGridObjectFromIsometricLayered(Vector3 worldPosition, TGridObject value)
  {
    for (int z = gridArray.GetLength(2) - 1; z >= 0; z--)
    {
      Vector3 newWorldPosition = new Vector3(worldPosition.x, worldPosition.y - cellSize / 2 * z, worldPosition.z);
      Vector2Int XY = GetXYIsometric(newWorldPosition);
      //Debug.Log(gridArray[XY.x, XY.y, z]);

      if (XY.x >= 0 && XY.y >= 0 && XY.x < width && XY.y < height && gridArray[XY.x, XY.y, z] != null)
      {
        gridArray[XY.x, XY.y, z] = value;
        break;
      }
    }
  }

  public void SetGridObjectFromIsometric(Vector3 worldPosition, TGridObject value)
  {
    Vector2Int cartesianGrid = GetXYIsometric(worldPosition);
    SetGridObject(cartesianGrid.x, cartesianGrid.y, value);
  }

  public void SetGridObject(int x, int y, int z, TGridObject value)
  {
    if (x >= 0 && y >= 0 && z >= 0 && x < width && y < height && z < maxElevation)
    {
      gridArray[x, y, z] = value;
      TriggerObjectChanged(x, y, z);
    }
  }

  public void SetGridObject(Vector3 worldPosition, TGridObject value)
  {
    Vector2Int XY = GetXY(worldPosition);
    SetGridObject(XY.x, XY.y, value);
  }

  public void SetGridObject(int x, int y, TGridObject value)
  {
    if (x >= 0 && y >= 0 && x < width && y < height)
    {
      gridArray[x, y, 0] = value;
      TriggerObjectChanged(x, y, 0);
    }
  }

  public void TriggerObjectChanged(int x, int y, int z)
  {
    if (OnGridValueChanged != null)
      OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y, z = z });
  }

  public TGridObject GetGridObjectFromIsometricLayered(Vector3 worldPosition)
  {
    for (int z = gridArray.GetLength(2) - 1; z >= 0; z--)
    {
      Vector3 newWorldPosition = new Vector3(worldPosition.x, worldPosition.y - cellSize / 2 * z, worldPosition.z);
      Vector2Int XY = GetXYIsometric(newWorldPosition);

      //check if the PlanetTile encountered is empty
      if (XY.x >= 0 && XY.y >= 0 && XY.x < width && XY.y < height && gridArray[XY.x, XY.y, z] != null)
      {
        return gridArray[XY.x, XY.y, z];
        break;
      }
    }
    return default(TGridObject);
  }

  public TGridObject GetGridObjectFromIsometric(Vector3 worldPosition)
  {
    Vector2Int cartesianGrid = GetXYIsometric(worldPosition);
    return GetGridObject(cartesianGrid.x, cartesianGrid.y);
  }

  public TGridObject GetGridObject(int x, int y, int z)
  {
    if (x >= 0 && y >= 0 && z >= 0 && x < width && y < height && z < maxElevation)
    {
      return gridArray[x, y, z];
    }
    else
    {
      return default(TGridObject);
    }
  }

  public TGridObject GetGridObject(Vector3 worldPosition)
  {
    Vector2Int XY = GetXY(worldPosition);
    return GetGridObject(XY.x, XY.y);
  }

  public TGridObject GetGridObject(int x, int y)
  {
    if (x >= 0 && y >= 0 && x < width && y < height)
    {
      return gridArray[x, y, 0];
    }
    else
    {
      return default(TGridObject);
    }
  }

  public int GetWidth()
  {
    return width;
  }

  public int GetHeight()
  {
    return height;
  }

  public int GetMaxElevation()
  {
    return maxElevation;
  }

  public float GetCellSize()
  {
    return cellSize;
  }
}
