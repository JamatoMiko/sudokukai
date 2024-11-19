using System.Diagnostics;

namespace sudokukai;

//与えられた問題を解く
//→消去法
//マスごとに候補の数字を調べる
//消去法だと限界がある
//候補が複数あっても、同じブロック・行・列内でそのマスにしか入らない数字があれば確定できる
//問題を作る
//→まず完成されたボードを作る、次に解決可能になるようにマスを開けていく

//難しい問題が解けない
//難しい問題を作れない
//今のままだと、常に候補が一つしかないから、最初の盤面からすべてのマスを埋めることができる問題しか作れない、順序を保存しないといけない
//候補の数が一つになるように空けていくんじゃなくて、Solverを呼び出して解決可能ならいいようにする、Solverが解決可能かどうかの値を返すようにする
//段階的に解けるようにする
//そのマスに入る数字の候補が複数あっても、その数字がそのマスにしか入れないんだったら確定できる
//Solverで解けない判定をどうするか
//ループ一周したときにcntが増えていなければ解けないということになる

//UpdatePossibilityを作ったので、CheckCellじゃなくていい

//難しすぎると解けない
//仮に入れてみて矛盾がないかを確かめるしかない？
//総当たりに成っちゃうけど
//矛盾がないか調べるメソッドを作る、_block、_row、_columnごとに数字の重複がないか調べる、Solverの始めに実行し矛盾があった場合エラーになる
//マスごとの消去法、数字ごとの消去法の二つで解けない場合、総当たりに移行

public class Board
{
    //_cell[行, 列]
    public int[,] _cell = {
    {4, 0, 5,  0, 0, 2,  7, 3, 8},
    {7, 6, 3,  1, 0, 0,  0, 4, 0},
    {0, 0, 0,  0, 0, 0,  0, 5, 0},

    {0, 3, 0,  0, 4, 0,  0, 0, 0},
    {0, 7, 0,  5, 0, 1,  0, 0, 4},
    {0, 4, 0,  8, 0, 0,  1, 0, 0},

    {1, 0, 0,  0, 0, 6,  3, 0, 2},
    {3, 9, 0,  0, 0, 0,  0, 0, 0},
    {0, 0, 0,  7, 5, 0,  0, 1, 0},
    };
    /*_cellの表
    (0,0)(0,1)(0,2) (0,3)(0,4)(0,5) (0,6)(0,7)(0,8)
    (1,0)(1,1)(1,2) (1,3)(1,4)(1,5) (1,6)(1,7)(1,8)
    (2,0)(2,1)(2,2) (2,3)(2,4)(0,5) (2,6)(2,7)(2,8)

    (3,0)(3,1)(3,2) (3,3)(3,4)(3,5) (3,6)(3,7)(3,8)
    (4,0)(4,1)(4,2) (4,3)(4,4)(4,5) (4,6)(4,7)(4,8)
    (5,0)(5,1)(5,2) (5,3)(5,4)(5,5) (5,6)(5,7)(5,8)

    (6,0)(6,1)(6,2) (6,3)(6,4)(6,5) (6,6)(6,7)(6,8)
    (7,0)(7,1)(7,2) (7,3)(7,4)(7,5) (7,6)(7,7)(7,8)
    (8,0)(8,1)(8,2) (8,3)(8,4)(8,5) (8,6)(8,7)(8,8)
    */
    //ブロック内に数字があるかどうか
    bool[,] _block = new bool[9, 9];
    //行内に数字があるかどうか
    bool[,] _row = new bool[9, 9];
    //列内に数字があるかどうか
    bool[,] _column = new bool[9, 9];
    //マスに数字が入るかどうか
    bool[,,] _po_num = new bool[9, 9, 9];
    //乱数オブジェクト
    Random rnd = new Random();
    public Board()//コンストラクタ
    {
        //DebugCell();
        UpdateBoard();
    }
    //メソッド////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public bool Solver()//与えられた問題を解決し、解決出来たらtrueを返す、出来なかったらfalseを返す
    {
        int cnt = 0;
        int pre_count = cnt;
        while (cnt < 81)
        {
            pre_count = cnt;
            cnt = 0;
            for (int row = 0; row < 9; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    //マスごとの候補の数字を調べる
                    if (_cell[row, column] == 0)//空だったら
                    {
                        if (CheckCell(row, column) == 1)//候補が一つだったら
                        {
                            for (int number = 0; number < 9; number++)
                            {
                                if (_po_num[row, column, number] == true)
                                {
                                    _cell[row, column] = number + 1;
                                    //_cellに変化を加えたらその都度更新する
                                    UpdateBoard();
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        cnt += 1;
                    }
                }
            }
            //数字ごとの候補のマスを調べる
            //まず全部のマスの候補を調べる
            UpdatePossibility();
            int m = 0;//開始行
            int n = 0;//開始列
            int p = 0;//候補行
            int q = 0;//候補列
            int po = 0;//候補のマスの数
            for (int number = 0; number < 9; number++)//数字
            {
                for (int block = 0; block < 9; block++)//ブロック
                {
                    //候補のマスの数を初期化
                    po = 0;
                    switch (block)//初期位置の指定
                    {
                        case 0:
                            m = 0;
                            n = 0;
                            break;
                        case 1:
                            m = 0;
                            n = 3;
                            break;
                        case 2:
                            m = 0;
                            n = 6;
                            break;
                        case 3:
                            m = 3;
                            n = 0;
                            break;
                        case 4:
                            m = 3;
                            n = 3;
                            break;
                        case 5:
                            m = 3;
                            n = 6;
                            break;
                        case 6:
                            m = 6;
                            n = 0;
                            break;
                        case 7:
                            m = 6;
                            n = 3;
                            break;
                        case 8:
                            m = 6;
                            n = 6;
                            break;
                    }
                    for (int i = 0; i < 3; i++)//行
                    {
                        for (int j = 0; j < 3; j++)//列
                        {
                            if (_cell[m + i, n + j] == 0)//空の場合
                            {
                                if (_po_num[m + i, n + j, number] == true)//数字が候補にある場合
                                {
                                    //候補のマスを保存
                                    p = m + i;
                                    q = n + j;
                                    //候補のマスの数を増やす
                                    po++;
                                }
                            }
                        }
                    }
                    if (po == 1)//候補のマスが一つだけの場合
                    {
                        //保存されたマスに数字を入れる
                        _cell[p, q] = number + 1;
                        //盤面を更新
                        UpdateBoard();
                    }
                }
                for (int row = 0; row < 9; row++)//行
                {
                    po = 0;
                    for (int column = 0; column < 9; column++)//列
                    {
                        if (_cell[row, column] == 0)//空の場合
                        {
                            if (_po_num[row, column, number] == true)//数字が候補にある場合
                            {
                                p = row;
                                q = column;
                                po++;
                            }
                        }
                    }
                    if (po == 1)//候補のマスが一つだけの場合
                    {
                        _cell[p, q] = number + 1;
                        UpdateBoard();
                    }
                }
                for (int column = 0; column < 9; column++)//列
                {
                    po = 0;
                    for (int row = 0; row < 9; row++)//列
                    {
                        if (_cell[row, column] == 0)//空の場合
                        {
                            if (_po_num[row, column, number] == true)//数字が候補にある場合
                            {
                                p = row;
                                q = column;
                                po++;
                            }
                        }
                    }
                    if (po == 1)//候補のマスが一つだけの場合
                    {
                        _cell[p, q] = number + 1;
                        UpdateBoard();
                    }
                }
            }
            if (cnt == pre_count)//カウントが前回のカウントと同じ（埋められるマスがない）場合
            {
                return false;
            }
        }
        return true;
    }
    public void Generator(int blank = 0)//指定したマスの数が空いた問題を作成する
    {
        //完成された盤面を作る
        bool complete;
        while (true)
        {
            InitializeCell();
            complete = GenerateCompletedBoard();
            if (complete)
            {
                break;
            }
        }
        //DebugCell();
        //ひとつ前の盤面を保存
        //盤面に一つ穴を空ける
        //Solverを実行して、trueが帰ってきたらひとつ前の盤面に戻してさらに穴をあけた盤面にしてその盤面を保存これを繰り返す、falseが帰ってきたらひとつ前の盤面に戻して別の穴をあけてまたSolverを実行する
        int[,] pre_cell = new int[9, 9];
        bool[,] tried = new bool[9, 9];
        int untried = 0;
        int row = 0;
        int column = 0;
        for (int i = 0; i < blank; i++)
        {
            //triedを初期化
            for (int m = 0; m < 9; m++)
            {
                for (int n = 0; n < 9; n++)
                {
                    tried[m, n] = false;
                }
            }
            while(true)
            {
                //盤面を保存
                Array.Copy(_cell, pre_cell, _cell.Length);
                while (true)
                {
                    row = rnd.Next(0, 9);
                    column = rnd.Next(0, 9);
                    if (_cell[row, column] > 0)//空じゃない場合
                    {
                        if (tried[row, column] == false)//まだ試していない場合
                        {
                            break;
                        }
                    }
                }
                _cell[row, column] = 0;//穴をあける
                UpdateBoard();
                if (Solver() == true)//解決可能の場合
                {
                    //ひとつ前の盤面に戻して穴をあける
                    Array.Copy(pre_cell, _cell, pre_cell.Length);
                    _cell[row, column] = 0;
                    UpdateBoard();
                    break;
                }
                else//解決不能の場合
                {
                    //ひとつ前の盤面に戻す(穴をあけない)
                    Array.Copy(pre_cell, _cell, pre_cell.Length);
                    UpdateBoard();
                    //マスを試行済みにする
                    tried[row, column] = true;
                    //試しいないマスがあるか調べる
                    untried = 0;
                    for (int m = 0; m < 9; m++)
                    {
                        for (int n = 0; n < 9; n++)
                        {
                            if (_cell[m, n] > 0 && tried[m, n] == false)//空じゃなく、試していない場合
                            {
                                untried++;
                            }
                        }
                    }
                    if (untried == 0)//試していないマスが無くなったら
                    {
                        //DebugCell();
                        Debug.WriteLine($"{i}/{blank}");
                        return;
                    }
                }
            }
        }
        //DebugCell();
    }
    bool GenerateCompletedBoard()
    {
        int number;
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                if (CheckCell(row, column) > 0)//候補がある場合
                {
                    while (true)
                    {
                        //候補の中からランダムに選び入れる
                        number = rnd.Next(0, 9);
                        if (_po_num[row, column, number] == true)
                        {
                            _cell[row, column] = number + 1;
                            //_cellに変化を加えたらその都度更新する
                            UpdateBoard();
                            break;
                        }
                    }
                }
                else//候補がない場合
                {
                    return false;
                }
            }
        }
        return true;
    }
    int CheckCell(int row, int column)//指定したマスに入れることができる数字を調べ候補数を返す
    {
        int cnt = 9;//候補数
        int block = IdentifyBlock(row, column);
        for (int number = 0; number < 9; number++)//_po_numをtrueで初期化
        {
            _po_num[row, column, number] = true;
        }
        for (int number = 0; number < 9; number++)
        {
            //ブロック内に同じ数字があるか
            if (_block[block, number] == true)
            {
                _po_num[row, column, number] = false;
                cnt -= 1;
                continue;
            }
            //行内に同じ数字があるか
            if (_row[row, number] == true)
            {
                _po_num[row, column, number] = false;
                cnt -= 1;
                continue;
            }
            //列内に同じ数字があるか
            if (_column[column, number] == true)
            {
                _po_num[row, column, number] = false;
                cnt -= 1;
                continue;
            }
        }
        return cnt;//候補数を返す
    }
    public int IdentifyBlock(int row, int column)//指定したマスが所属しているブロックを調べる
    {
        int block = 0;
        if (row < 3)
        {
            if (column < 3)
            {
                block = 0;
            }
            else if (column < 6)
            {
                block = 1;
            }
            else
            {
                block = 2;
            }
        }
        else if (row < 6)
        {
            if (column < 3)
            {
                block = 3;
            }
            else if (column < 6)
            {
                block = 4;
            }
            else
            {
                block = 5;
            }
        }
        else
        {
            if (column < 3)
            {
                block = 6;
            }
            else if (column < 6)
            {
                block = 7;
            }
            else
            {
                block = 8;
            }
        }
        return block;
    }
    void InitializeCell()//_cellの初期化
    {
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                _cell[row, column] = 0;
            }
        }
        UpdateBoard();
    }
    public void UpdateBoard()//全般の情報の更新
    {
        UpdateBlock();
        UpdateRow();
        UpdateColumn();
        UpdatePossibility();
    }
    bool UpdateBlock()//ブロック情報_blockの更新、矛盾していたらtrueを返す
    {
        int row = 0;
        int column = 0;
        for (int block = 0; block < 9; block++)//_blockをfalseで初期化
        {
            for (int number = 0; number < 9; number++)
            {
                _block[block, number] = false;
            }
        }
        for (int block = 0; block < 9; block++)//ブロック内に存在する数字の_blockをtrueに
        {
            switch (block)//開始位置(row, column)の指定
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
                for (int j = 0; j < 3; j++)//列＋
                {
                    if (_cell[row + i, column + j] > 0)
                    {
                        if (_block[block, _cell[row + i, column + j] - 1] == false)
                        {
                            _block[block, _cell[row + i, column + j] - 1] = true;
                        }
                        else//数字が重複していた場合
                        {
                            return true;
                        }
                    }
                }
            }
        }
        //DebugBlock();
        return false;
    }
    bool UpdateRow()//行情報_rowの更新、矛盾していたらtrueを返す
    {
        for (int row = 0; row < 9; row++)//_rowをfalseで初期化
        {
            for (int number = 0; number < 9; number++)
            {
                _row[row, number] = false;
            }
        }
        for (int row = 0; row < 9; row++)//行内に存在する数字の_rowをtrueに
        {
            for (int column = 0; column < 9; column++)
            {
                if (_cell[row, column] > 0)
                {
                    if (_row[row, _cell[row, column] - 1] == false)
                    {
                        _row[row, _cell[row, column] - 1] = true;
                    }
                    else//数字が重複していた場合
                    {
                        return true;
                    }
                }
            }
        }
        //DebugRow();
        return false;
    }
    bool UpdateColumn()//列情報_columnの更新、矛盾していたらtrueを返す
    {
        for (int column = 0; column < 9; column++)//_columnをfalseで初期化
        {
            for (int number = 0; number < 9; number++)
            {
                _column[column, number] = false;
            }
        }
        for (int column = 0; column < 9; column++)//列内に存在する数字の_columnをtrueに
        {
            for (int row = 0; row < 9; row++)
            {
                if (_cell[row, column] > 0)
                {
                    if (_column[column, _cell[row, column] - 1] == false)
                    {
                        _column[column, _cell[row, column] - 1] = true;
                    }
                    else//数字が重複していた場合
                    {
                        return false;
                    }
                }
            }
        }
        //DebugColumn();
        return false;
    }
    void UpdatePossibility()//候補の数字の更新
    {
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                if (_cell[row, column] == 0)
                {
                    CheckCell(row, column);
                }
            }
        }
    }
    //デバッグ表示////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void DebugCell()//_cellの内容をデバッグ表示
    {
        Debug.WriteLine("ーーーーーーーーーーーー");
        for (int i = 0; i < 9;)
        {
            Debug.WriteLine($"{_cell[i, 0]} {_cell[i, 1]} {_cell[i, 2]} | {_cell[i, 3]} {_cell[i, 4]} {_cell[i, 5]} | {_cell[i, 6]} {_cell[i, 7]} {_cell[i++, 8]}");
            Debug.WriteLine($"{_cell[i, 0]} {_cell[i, 1]} {_cell[i, 2]} | {_cell[i, 3]} {_cell[i, 4]} {_cell[i, 5]} | {_cell[i, 6]} {_cell[i, 7]} {_cell[i++, 8]}");
            Debug.WriteLine($"{_cell[i, 0]} {_cell[i, 1]} {_cell[i, 2]} | {_cell[i, 3]} {_cell[i, 4]} {_cell[i, 5]} | {_cell[i, 6]} {_cell[i, 7]} {_cell[i++, 8]}");
            Debug.WriteLine("ーーーーーーーーーーーー");
        }
    }
    public void DebugBlock()//_blockの内容をデバッグ表示
    {
        for (int i = 0; i < 9; i++)
        {
            Debug.WriteLine($"B[{i}]{_block[i, 0]},{_block[i, 1]},{_block[i, 2]},{_block[i, 3]},{_block[i, 4]},{_block[i, 5]},{_block[i, 6]},{_block[i, 7]},{_block[i, 8]}");
        }
    }
    public void DebugRow()//_rowの内容をデバッグ表示
    {
        for (int i = 0; i < 9; i++)
        {
            Debug.WriteLine($"[R{i}]{_row[i, 0]},{_row[i, 1]},{_row[i, 2]},{_row[i, 3]},{_row[i, 4]},{_row[i, 5]},{_row[i, 6]},{_row[i, 7]},{_row[i, 8]}");
        }
    }
    public void DebugColumn()//_columnの内容をデバッグ表示
    {
        for (int i = 0; i < 9; i++)
        {
            Debug.WriteLine($"[C{i}]{_column[i, 0]},{_column[i, 1]},{_column[i, 2]},{_column[i, 3]},{_column[i, 4]},{_column[i, 5]},{_column[i, 6]},{_column[i, 7]},{_column[i, 8]}");
        }
    }
}