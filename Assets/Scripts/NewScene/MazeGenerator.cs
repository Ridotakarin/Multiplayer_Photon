using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 15;
    public int height = 15;
    public GameObject wallPrefab;

    private int[,] maze;

    void Start()
    {
        GenerateMaze();
        DrawMaze();
    }

    void GenerateMaze()
    {
        maze = new int[width, height];

        // ban đầu toàn bộ là tường
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1;

        // bắt đầu đào từ (1,1)
        Carve(1, 1);
    }

    void Carve(int x, int y)
    {
        maze[x, y] = 0;
        int[] dirs = { 0, 1, 2, 3 };
        Shuffle(dirs);

        foreach (int dir in dirs)
        {
            int dx = 0, dy = 0;
            if (dir == 0) dx = 1;       // phải
            else if (dir == 1) dx = -1; // trái
            else if (dir == 2) dy = 1;  // lên
            else if (dir == 3) dy = -1; // xuống

            int nx = x + dx * 2;
            int ny = y + dy * 2;

            if (nx > 0 && ny > 0 && nx < width && ny < height) // KHÔNG ép viền
            {
                if (maze[nx, ny] == 1)
                {
                    maze[x + dx, y + dy] = 0;
                    Carve(nx, ny);
                }
            }
        }
    }

    void DrawMaze()
    {
        float step = 3f; // mỗi ô cách nhau 3 đơn vị

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 1)
                {
                    Vector3 pos = new Vector3(x * step, 0, y * step);
                    Quaternion rot = Quaternion.identity;

                    // nếu tường dạng dọc thì xoay
                    if (IsVerticalWall(x, y))
                        rot = Quaternion.Euler(0, 90, 0);

                    Instantiate(wallPrefab, pos, rot, transform);
                }
            }
        }
    }

    bool IsVerticalWall(int x, int y)
    {
        // kiểm tra có đường đi ở trái/phải thì xoay dọc
        bool left = (x > 0 && maze[x - 1, y] == 0);
        bool right = (x < width - 1 && maze[x + 1, y] == 0);

        if (left || right) return true;
        return false;
    }

    void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int tmp = array[i];
            int r = Random.Range(i, array.Length);
            array[i] = array[r];
            array[r] = tmp;
        }
    }
}
