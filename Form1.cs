using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace sudokukai;

//Undoができるように
//完成された盤面と完全に一致したらクリア
//入力しきった数字のラベルを押せなくする、イベントハンドラを削除？、文字の色を薄くする、ラベルごと削除？
//候補の数字を小さくメモれるように
//間違えられる回数に上限を

public partial class Form1 : Form
{
    Board board;
    Board solved_board;
    System.Windows.Forms.Timer timer;
    Point point;
    Label[] numbers = new Label[9];
    //列(column)がx座標、行(row)がy座標なことに注意
    int _cursor_row = 0;
    int _cursor_column = 0;
    int _cursor_number;
    bool[,] _match;
    public Form1()//コンストラクタ
    {
        board = new Board();//現在の盤面
        board.Generator(80);
        solved_board = new Board();//解決された盤面
        Array.Copy(board._cell, solved_board._cell, board._cell.Length);//盤面をコピー
        solved_board.UpdateBoard();
        solved_board.Solver();//問題を解く
        //board.DebugCell();
        //solved_board.DebugCell();
        _match = new bool[9, 9];

        InitializeComponent();

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 1;
        timer.Tick += new EventHandler(TimerTick);
        timer.Start();

        //数字ラベル
        var font = new Font("Arial", 32, FontStyle.Regular);
        for (int i = 0; i < 9; i++)
        {
            switch (i)
            {
                case 0:
                    point = new Point(320+0*64, 96+0*64);
                    break;
                case 1:
                    point = new Point(320+1*64, 96+0*64);
                    break;
                case 2:
                    point = new Point(320+2*64, 96+0*64);
                    break;
                case 3:
                    point = new Point(320+0*64, 96+1*64);
                    break;
                case 4:
                    point = new Point(320+1*64, 96+1*64);
                    break;
                case 5:
                    point = new Point(320+2*64, 96+1*64);
                    break;
                case 6:
                    point = new Point(320+0*64, 96+2*64);
                    break;
                case 7:
                    point = new Point(320+1*64, 96+2*64);
                    break;
                case 8:
                    point = new Point(320+2*64, 96+2*64);
                    break;
            }
            numbers[i] = new Label() {
                Text = $"{i + 1}",
                Location =  point,
                ForeColor = Color.FromArgb(0x34, 0x48, 0x61),
                BackColor = Color.FromArgb(0xE3, 0xE7, 0xF0),
                Font = font,
                AutoSize = true
            };
            numbers[i].Click += new EventHandler(ClickNumbers);
        }
        this.Controls.AddRange(numbers);

        //消去ラベル
        var erace = new Label(){
            Text = "X",
            Location = new Point (320+1*64, 96+3*64),
            ForeColor = Color.FromArgb(0x34, 0x48, 0x61),
            BackColor = Color.FromArgb(0xE3, 0xE7, 0xF0),
            Font = font,
            AutoSize = true
        };
        erace.Click += new EventHandler(ClickErace);
        this.Controls.Add(erace);

        this.DoubleBuffered = true;
    }

    bool CompareBoard()//盤面の比較、すべてのマスが一致したらtrueを返す
    {
        int cnt = 0;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (board._cell[i, j] == solved_board._cell[i, j])
                {
                    _match[i, j] = true;
                    cnt++;
                }
                else
                {
                    _match[i, j] = false;
                }
            }
        }
        if (cnt < 81)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    void TimerTick(object sender, EventArgs e)//ティックイベント
    {
        //this.Text = $"[{_cursor_number}]({_cursor_row},{_cursor_column})";
        _cursor_number = board._cell[_cursor_row, _cursor_column];
        if (CompareBoard() == true)//解き終わった場合
        {
            // MessageBox.Show("Congratulations!");
        }
        //再描画
        this.Invalidate();
    }
    protected override void OnMouseClick(MouseEventArgs e)//クリックイベント
    {
        base.OnMouseClick(e);
        if (e.X < 32 * 9 && e.Y < 32 * 9)
        {
            _cursor_row = e.Y/32;
            _cursor_column = e.X/32;
        }
    }
    void ClickNumbers(object sender, EventArgs e)//数字をクリックしたときのイベント
    {
        Label numbers = (Label) sender;
        int number = int.Parse(numbers.Text);
        board.InputCell(_cursor_row, _cursor_column, number);
    }
    void ClickErace(object sender, EventArgs e)//消去をクリックしたときのイベント
    {
        board.InputCell(_cursor_row, _cursor_column, 0);
    }
    protected override void OnPaint(PaintEventArgs e)//描画処理
    {
        base.OnPaint(e);

        var brush1 = new SolidBrush(Color.FromArgb(0xE2, 0xEB, 0xF3));
        var brush2 = new SolidBrush(Color.FromArgb(0xC3, 0xD7, 0xEA));
        var brush3 = new SolidBrush(Color.FromArgb(0xBB, 0xDE, 0xFB));
        int row = 0;
        int column = 0;
        //ブロック
        //所属しているブロックを特定
        int block = board.IdentifyBlock(_cursor_row, _cursor_column);
        //開始位置を指定
        switch (block)
        {
            case 0:
                row = 0;
                column = 0;
                break;
            case 1:
                row = 0;
                column = 3;
                break;
            case 2:
                row = 0;
                column = 6;
                break;
            case 3:
                row = 3;
                column = 0;
                break;
            case 4:
                row = 3;
                column = 3;
                break;
            case 5:
                row = 3;
                column = 6;
                break;
            case 6:
                row = 6;
                column = 0;
                break;
            case 7:
                row = 6;
                column = 3;
                break;
            case 8:
                row = 6;
                column = 6;
                break;
        }
        for (int i = 0; i < 3; i++)//行＋
        {
            for (int j =0; j < 3; j++)//列＋
            {
                e.Graphics.FillRectangle(brush1, 32*(column+j)+1, 32*(row+i)+1, 32, 32);
            }
        }
        //行
        row = _cursor_row;
        for (column = 0; column < 9; column++)
        {
            e.Graphics.FillRectangle(brush1, 32*column+1, 32*row+1, 32, 32);
        }
        //列
        column = _cursor_column;
        for (row = 0; row < 9; row++)
        {
            e.Graphics.FillRectangle(brush1, 32*column+1, 32*row+1, 32, 32);
        }
        //カーソルと同じ数字
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (_cursor_number > 0 && board._cell[i, j] == _cursor_number)
                {
                    e.Graphics.FillRectangle(brush2, 32*j+1, 32*i+1, 32, 32);
                }
            }
        }
        //カーソル
        e.Graphics.FillRectangle(brush3, 32*_cursor_column+1, 32*_cursor_row+1, 32, 32);
        brush1.Dispose();
        brush2.Dispose();
        brush3.Dispose();

        var pen1 = new Pen(Color.FromArgb(0xBF, 0xC6, 0xD4), 1);
        var pen2 = new Pen(Color.FromArgb(0x34, 0x48, 0x61), 2);
        //グリッド（細）
        for (int i = 0; i < 10; i++)
        {
            e.Graphics.DrawLine(pen1, 32*i+1, 1, 32*i+1, 32*9+1);
        }
        for (int i = 0; i < 10; i++)
        {
            e.Graphics.DrawLine(pen1, 1, 32*i+1, 32*9+1, 32*i+1);
        }
        //グリッド（太）
        for (int i = 0; i < 4; i++)
        {
            e.Graphics.DrawLine(pen2, 32*3*i+1, 1, 32*3*i+1, 32*9+1);
        }
        for (int i = 0; i < 4; i++)
        {
            e.Graphics.DrawLine(pen2, 1, 32*3*i+1, 32*9+1, 32*3*i+1);
        }
        pen1.Dispose();
        pen2.Dispose();

        var font = new Font("Arial", 20, FontStyle.Regular);
        var brush4 = new SolidBrush(Color.FromArgb(0x34, 0x48, 0x61));//元からある数字
        var brush5 = new SolidBrush(Color.FromArgb(0x32, 0x5A, 0xAF));//入力した数字
        var brush6 = new SolidBrush(Color.FromArgb(0xE5, 0x5C, 0x6C));//間違っている数字
        //数字
        for (int i = 0; i < 9; i++)//行
        {
            for (int j = 0; j < 9; j++)//列
            {
                if (board._cell[i, j] > 0)
                {
                    if (board._changeable[i, j] == false)//元からある数字
                    {
                        e.Graphics.DrawString($"{board._cell[i, j]}", font, brush4, 32*j+6, 32*i+2);
                    }
                    if (board._changeable[i, j] == true)//入力した数字
                    {
                        e.Graphics.DrawString($"{board._cell[i, j]}", font, brush5, 32*j+6, 32*i+2);
                    }
                    if (_match[i, j] == false)//間違っている数字
                    {
                        e.Graphics.DrawString($"{board._cell[i, j]}", font, brush6, 32*j+6, 32*i+2);
                    }
                }
            }
        }
        font.Dispose();
        brush4.Dispose();
        brush5.Dispose();
        brush6.Dispose();

        //数字ラベル


    }
}
