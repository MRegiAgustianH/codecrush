using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public int type;

    private Board board;

    public void Init(int x, int y, int type, Sprite sprite, Board board)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        this.board = board;

        GetComponent<SpriteRenderer>().sprite = sprite;
        transform.localScale = Vector3.one * 0.8f;
    }

    private void OnMouseDown()
    {
        board.SelectTile(this);
    }
}