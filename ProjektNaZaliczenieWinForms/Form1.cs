using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProjektNaZaliczenieWinForms.Properties;

namespace ProjektNaZaliczenieWinForms
{
    public partial class Form1 : Form
    {
        const int boardSize = 8;
        const int cellSize = 50;

        int currentPlayer;
        bool isMoving;
        bool hasWonWhite;
        bool hasWonBlack;

        Button previousButton; //zmienna którą wykorzystuje do poznania koordynatów wybranego pionka
        Button pressedButton; // zmienna do wybrania pionka oraz przeszmieszczania go

        Image whiteFig, blackFig;

        int[,] board = new int[boardSize, boardSize];

        //Tablice z warunkami zwycięstwa
        int[,] boardW =
        {
            {3, 1, 3, 1, 3, 1, 3, 1},
            {1, 3, 1, 3, 1, 3, 1, 3}
        };

        int[,] boardB =
        {
            {3, 2, 3, 2, 3, 2, 3, 2},
            {2, 3, 2, 3, 2, 3, 2, 3}
        };

        Panel panel = new Panel(); //sztucznie utworzony panel do resetu gry
        public Form1()
        {
            InitializeComponent();

            whiteFig = new Bitmap(new Bitmap(@"C:\Users\Patryk\source\repos\ProjektNaZaliczenieWinForms\ProjektNaZaliczenieWinForms\Resources\w2.png"),
                new Size(cellSize - 10, cellSize - 10));
            blackFig = new Bitmap(new Bitmap(@"C:\Users\Patryk\source\repos\ProjektNaZaliczenieWinForms\ProjektNaZaliczenieWinForms\Resources\b2.png"),
                new Size(cellSize - 10, cellSize - 10));

            Game();
        }

        public void Game() //funckja zarządzająca grą
        {
            currentPlayer = 1;
            isMoving = false;
            previousButton = null;
            hasWonWhite = false;
            hasWonBlack = false;

            /*
            //testowy warunek zwycięstwa
            board = new int[,]
            {
                {3, 1, 3, 1, 3, 1, 3, 1},
                {1, 3, 1, 3, 1, 3, 0, 3},
                {3, 0, 3, 0, 3, 0, 3, 1},
                {0, 3, 0, 3, 0, 3, 0, 3},
                {3, 0, 3, 0, 3, 0, 3, 0},
                {0, 3, 0, 3, 0, 3, 2, 3},
                {3, 2, 3, 2, 3, 2, 3, 0},
                {2, 3, 2, 3, 2, 3, 2, 3},
            };
            */
            
            board = new int[,]
            {
                {3, 2, 3, 2, 3, 2, 3, 2},
                {2, 3, 2, 3, 2, 3, 2, 3},
                {3, 0, 3, 0, 3, 0, 3, 0},
                {0, 3, 0, 3, 0, 3, 0, 3},
                {3, 0, 3, 0, 3, 0, 3, 0},
                {0, 3, 0, 3, 0, 3, 0, 3},
                {3, 1, 3, 1, 3, 1, 3, 1},
                {1, 3, 1, 3, 1, 3, 1, 3}
            };
            


            CreateBoard();
        }

        public void CreateBoard() //utworzenie planszy z dynamicznie stworzonych przycisków
        {
            this.Width = boardSize * cellSize;
            this.Height = boardSize * cellSize;
            panel.Location = new Point(0, 0);
            panel.Size = new Size(600, 600);
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    Button button = new Button();
                    button.Location = new Point(j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
                    button.Click += new EventHandler(OnFigurePress); //zdarzenie obsługujące całą logike gry
                    button.Click += new EventHandler(pictureBox1_Click); //zdarzenie obsługujące wyświetlenie aktualnego i zwycięskiego gracza
                    if (board[i, j] == 1) button.Image = whiteFig;
                    else if (board[i, j] == 2) button.Image = blackFig;

                    if (i % 2 != 0 && j % 2 == 0) button.BackColor = Color.SaddleBrown;
                    else if (i % 2 == 0 && j % 2 != 0) button.BackColor = Color.SaddleBrown;
                    else button.BackColor = Color.White;
                    panel.Controls.Add(button);
                }
            }
            this.Controls.Add(panel);

        }
        public void SwitchPlayer()
        {
            currentPlayer = currentPlayer == 1 ? 2 : 1;

        }


        public Color GetPreviousButtonColor() //funckja resetująca kolor pola z pionkiem, któe zostało wcześniej wybrane
        {
            var x = previousButton.Location.X / cellSize;
            var y = previousButton.Location.Y / cellSize;
            if (y % 2 != 0 && x % 2 == 0)
                return Color.SaddleBrown;
            if (y % 2 == 0 && x % 2 != 0)
                return Color.SaddleBrown;
            return Color.White;
        }

        public void OnFigurePress(object sender, EventArgs e)
        {

            if (previousButton != null)
                previousButton.BackColor = GetPreviousButtonColor();

            pressedButton = sender as Button;

            if (board[pressedButton.Location.Y / cellSize, (pressedButton.Location.X / cellSize)] == currentPlayer) //wybór pionka należącego do gracza
            {
                pressedButton.BackColor = Color.Red; //podświietlenie wybranego pionka
                isMoving = true;
            }
            else
            {
                if (isMoving)//przemieszczenie pionka
                {
                    if (CanJump(pressedButton, previousButton) || CanMove(pressedButton, previousButton))
                    {
                        int tmp = board[pressedButton.Location.Y / cellSize, (pressedButton.Location.X / cellSize)];
                        board[pressedButton.Location.Y / cellSize, (pressedButton.Location.X / cellSize)] =
                            board[previousButton.Location.Y / cellSize, (previousButton.Location.X / cellSize)];
                        board[previousButton.Location.Y / cellSize, (previousButton.Location.X / cellSize)] = tmp;
                        pressedButton.Image = previousButton.Image;
                        previousButton.Image = null;
                        isMoving = false;
                        WinCondition();
                        if (hasWonWhite)
                        {
                            pictureBox1.Image = whiteFig;
                            label2.Text = "Wygral gracz:";
                        }
                        else if (hasWonBlack)
                        {
                            label2.Text = "Wygral gracz:";
                            pictureBox1.Image = blackFig;
                        }
                        else
                        {
                            SwitchPlayer();
                        }
                    }
                }
            }

            previousButton = pressedButton;
        }

        public bool CanMove(Button pressedButton, Button previousButton)//ruch pionka o jedno pole do porzodu w zależności od gracza
        {
            var pressedBLocation = new Point(pressedButton.Location.X, pressedButton.Location.Y);
            var previousBLocation = new Point(previousButton.Location.X, previousButton.Location.Y);
            if (GetPawnValue(pressedButton) != 0) return false;
            if (GetPawnValue(previousButton) != currentPlayer) return false;
            if (!(pressedBLocation.X == previousBLocation.X + 50 || pressedBLocation.X == previousBLocation.X - 50)) return false;
            if (currentPlayer == 2)
            {
                if (pressedBLocation.Y != previousBLocation.Y + 50) return false;
            }
            else
            {
                if (pressedBLocation.Y != previousBLocation.Y - 50) return false;
            }
            return true;
        }

        public bool CanJump(Button pressedButton, Button previousButton) //ruch gracza pozwalający na przeskoczenie pionka
        {
            var pressedBLocation = new Point(pressedButton.Location.X, pressedButton.Location.Y);
            var previousBLocation = new Point(previousButton.Location.X, previousButton.Location.Y);
            if (GetPawnValue(pressedButton) != 0) return false;
            if (GetPawnValue(previousButton) != currentPlayer) return false;
            if (!(pressedBLocation.X == previousBLocation.X + 100 || pressedBLocation.X == previousBLocation.X - 100)) return false;

            if (pressedBLocation.Y == previousBLocation.Y + 100
                && pressedBLocation.X == previousBLocation.X - 100)
            {
                if ((previousBLocation.Y + 50) / cellSize < boardSize && (previousBLocation.X - 50) / cellSize >= 0)
                {
                    if (board[(previousBLocation.Y + 50) / cellSize, (previousBLocation.X - 50) / cellSize] != 0)
                        return true;
                }
            }


            if (pressedBLocation.Y == previousBLocation.Y + 100
                && pressedBLocation.X == previousBLocation.X + 100)
            {
                if ((previousBLocation.Y + 50) / cellSize < boardSize && (previousBLocation.X + 50) / cellSize < boardSize)
                {
                    if (board[(previousBLocation.Y + 50) / cellSize, (previousBLocation.X + 50) / cellSize] != 0)
                        return true;
                }
            }

            if (pressedBLocation.Y == previousBLocation.Y - 100
                && pressedBLocation.X == previousBLocation.X + 100)
            {
                if ((previousBLocation.Y - 50) / cellSize >= 0 && (previousBLocation.X + 50) / cellSize < boardSize)
                {
                    if (board[(previousBLocation.Y - 50) / cellSize, (previousBLocation.X + 50) / cellSize] != 0)
                        return true;
                }
            }

            if (pressedBLocation.Y == previousBLocation.Y - 100
                && pressedBLocation.X == previousBLocation.X - 100)
            {
                if ((previousBLocation.Y - 50) / cellSize >= 0 && (previousBLocation.X - 50) / cellSize >= 0)
                {
                    if (board[(previousBLocation.Y - 50) / cellSize, (previousBLocation.X - 50) / cellSize] != 0)
                        return true;
                }
            }
            return false;
        }

        private void pictureBox1_Click(object sender, EventArgs e) 
        {
            pictureBox1.Image = currentPlayer == 1 ? whiteFig : blackFig;
        }

        private void button1_Click(object sender, EventArgs e) //reset gry
        {
            panel.Controls.Clear();
            label2.Text = "Tura gracza";
            hasWonWhite = false;
            hasWonBlack = false;
            pictureBox1.Image = whiteFig;
            Game();
        }

        public int GetPawnValue(Button button) //funkcja wspomagająca zwracająca jakim rodzajem jest pionek w danej lokalizacji
        {

            return board[button.Location.Y / cellSize, (button.Location.X / cellSize)];
        }

        public bool WinCondition()
        {
            int countWinConditionWhite = 0;
            int countWinConditionBlack = 0;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (board[i, j] == boardW[i, j]) countWinConditionWhite++;
                    if(board[i+6, j] == boardB[i, j]) countWinConditionBlack++;
                }
            }

            if (countWinConditionWhite == 16)
            {
                hasWonWhite = true;
                return hasWonWhite;
            }
            
            if(countWinConditionBlack == 16)
            {
                hasWonBlack = true;
                return hasWonBlack;
            }
            
            return true;
        }

    }
}
