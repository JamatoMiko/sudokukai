using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace sudokukai;

//Undoができるように
//完成された盤面と完全に一致したらクリア
//入力しきった数字のラベルを押せなくする、イベントハンドラを削除？、文字の色を薄くする、ラベルごと削除？、Board._numbersを使用
//メモのON/OFFの状態をわかりやすく
//間違えられる回数に上限を
//クリックイベントを左クリック限定に、右クリックでマスをクリア

public partial class Form1 : Form
{
    Board board;
    Board solved_board;
    System.Windows.Forms.Timer timer;
    Point point;
    Label[] numbers = new Label[9];
    Label memo = new Label();
    //列(column)がx座標、行(row)がy座標なことに注意
    int _cursor_row = 0;
    int _cursor_column = 0;
    int _cursor_number;
    bool[,] _match;
    bool _memo;
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

        //メモラベル
        memo.Text = "OFF";
        memo.Location = new Point (320+2*64, 96+3*64);
        memo.ForeColor = Color.FromArgb(0x34, 0x48, 0x61);
        memo.BackColor = Color.FromArgb(0xE3, 0xE7, 0xF0);
        memo.Font = font;
        memo.AutoSize = true;
        memo.Click += new EventHandler(ClickMemo);
        this.Controls.Add(memo);

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
        if (_memo == false)
        {
            if (board._cell[_cursor_row, _cursor_column] != number)
            {
                board.InputCell(_cursor_row, _cursor_column, number);
                //メモから入力した数字を消去
                //ブロック
                int block = board.IdentifyBlock(_cursor_row, _cursor_column);
                int row = 0;
                int column = 0;
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
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        board._memo_num[row + i, column + j, number - 1] = false;
                    }
                }
                //行
                for (int i = 0; i < 9; i++)
                {
                    board._memo_num[_cursor_row, i, number - 1] = false;
                }
                //列
                for (int i = 0; i < 9; i++)
                {
                    board._memo_num[i, _cursor_column, number - 1] = false;
                }
            }
            else
            {
                board.InputCell(_cursor_row, _cursor_column, 0);
            }
            for (int i = 0; i < 9; i++)
            {
                board._memo_num[_cursor_row, _cursor_column, i] = false;
            }
        }
        else//メモ
        {
            if (board._cell[_cursor_row, _cursor_column] == 0)
            {
                if (board._memo_num[_cursor_row, _cursor_column, number - 1] == false)
                {
                    board._memo_num[_cursor_row, _cursor_column, number - 1] = true;
                }
                else
                {
                    board._memo_num[_cursor_row, _cursor_column, number - 1] = false;
                }
            }
        }
    }
    void ClickErace(object sender, EventArgs e)//消去をクリックしたときのイベント
    {
        if (_memo == false)
        {
            board.InputCell(_cursor_row, _cursor_column, 0);
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                board._memo_num[_cursor_row, _cursor_column, i] = false;
            }
        }
    }
    void ClickMemo(object sender, EventArgs e)//メモをクリックしたときのイベント
    {
        if (_memo == false)
        {
            _memo = true;
            memo.Text = "ON";
            Debug.WriteLine($"Memo {_memo}");
        }
        else
        {
            _memo = false;
            memo.Text = "OFF";
            Debug.WriteLine($"Memo {_memo}");
        }
    }
    protected override void OnPaint(PaintEventArgs e)//描画処理
    {
        base.OnPaint(e);

        var brush1 = new SolidBrush(Color.FromArgb(0xE2, 0xEB, 0xF3));
        var brush2 = new SolidBrush(Color.FromArgb(0xC3, 0xD7, 0xEA));
        var brush3 = new SolidBrush(Color.FromArgb(0xBB, 0xDE, 0xFB));
        int row = 0;
        int column = 0;
        //カーソルのブロックをハイライト
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
        //カーソルの行をハイライト
        row = _cursor_row;
        for (column = 0; column < 9; column++)
        {
            e.Graphics.FillRectangle(brush1, 32*column+1, 32*row+1, 32, 32);
        }
        //カーソルの列をハイライト
        column = _cursor_column;
        for (row = 0; row < 9; row++)
        {
            e.Graphics.FillRectangle(brush1, 32*column+1, 32*row+1, 32, 32);
        }
        //カーソルと同じ数字をハイライト
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
        var font2 = new Font("Arial", 6, FontStyle.Regular);
        var brush4 = new SolidBrush(Color.FromArgb(0x34, 0x48, 0x61));//元からある数字
        var brush5 = new SolidBrush(Color.FromArgb(0x32, 0x5A, 0xAF));//入力した数字
        var brush6 = new SolidBrush(Color.FromArgb(0xE5, 0x5C, 0x6C));//間違っている数字
        var brush7 = new SolidBrush(Color.FromArgb(0x6E, 0x7C, 0x8C));//メモ
        //数字、メモ
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
                else//空
                {
                    for (int number = 0; number < 9; number++)
                    {
                        switch (number)
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
                        if (board._memo_num[i, j, number] == true)
                        {
                            e.Graphics.DrawString($"{number + 1}", font2, brush7, 32*j+3*column+5, 32*i+3*row+3);
                        }
                    }
                }
            }
        }
        font.Dispose();
        font2.Dispose();
        brush4.Dispose();
        brush5.Dispose();
        brush6.Dispose();
        brush7.Dispose();
    }
}
