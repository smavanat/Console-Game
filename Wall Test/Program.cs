using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks.Sources;

namespace Wall
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Wall grid = new Wall();
            grid.GenerateTileGrid(15, 15);
            grid.Move();
        }
    }
    public class Wall
    {
        public Node[,] grid;
        int floorMaxSize = 300;
        int gridSizeX = 15;
        int gridSizeY = 15;
        int spawnCount = 0;
        int score = 0;
        List<Enemy> enemies;
        public Player player;
        public bool isDead = false;

        public void GenerateTileGrid(int width, int hieght)
        {
            int currentFloorCount = 0;
            Player positionChecker = new Player(gridSizeX/2, gridSizeY/2);
            Random randMovement = new Random();
            player = new Player(15 / 2, 15 / 2);
            enemies = new List<Enemy>();

            //Create grid of empty nodes
            grid = new Node[width, hieght];
            //Populates the 2D array
            for (int x = 0; x < hieght; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    grid[x, y] = new Node(0,x,y);//middle
                }
            }
            //Use the Drunkard's Walk algorithm to populate the grid with floor tiles.
            while (currentFloorCount < floorMaxSize)
            {
                int direction = randMovement.Next(0,4);
                switch(direction)
                {
                    case 0:
                        //Checks if the imaginary 'player' used as an agent for the drunkard's walk will not ove outside of the array.
                        if (!CheckIfOutOfBounds(positionChecker.xPos - 1, positionChecker.yPos))
                        {
                            positionChecker.xPos--;
                            grid[positionChecker.xPos, positionChecker.yPos] = new Node(3, positionChecker.xPos, positionChecker.yPos);
                        }  
                        break;
                    case 1:
                        if (!CheckIfOutOfBounds(positionChecker.xPos + 1, positionChecker.yPos))
                        {
                            positionChecker.xPos++;
                            grid[positionChecker.xPos, positionChecker.yPos] = new Node(3, positionChecker.xPos, positionChecker.yPos);
                        }
                        break;
                    case 2:
                        if (!CheckIfOutOfBounds(positionChecker.xPos, positionChecker.yPos - 1))
                        {
                            positionChecker.yPos--;
                            grid[positionChecker.xPos, positionChecker.yPos] = new Node(3, positionChecker.xPos, positionChecker.yPos);
                        } 
                        break;
                    case 3:
                        if (!CheckIfOutOfBounds(positionChecker.xPos, positionChecker.yPos + 1))
                        {
                            positionChecker.yPos++;
                            grid[positionChecker.xPos, positionChecker.yPos] = new Node(3, positionChecker.xPos, positionChecker.yPos);
                        }
                        break;
                }
                currentFloorCount++;
                //Do this after generating the floor to cover up the outside with walls
                for (int x = 0; x < hieght; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        //Must do sides first otherwise grid looks wrong.
                        if ((x != 0 || x != hieght - 1) && (y == 0 || y == width - 1))
                        {
                            grid[x, y] = new Node(2,x,y);//sides
                        }
                        else if ((x == 0 || x == hieght - 1) && (y != 0 || y != width - 1))
                        {
                            grid[x, y] = new Node(1,x,y);//top+bottom
                        }
                    }
                }
                grid[gridSizeX / 2, gridSizeY / 2].image = player.sprite;
            }
            //While loop spawns enemies
            EnemySpawner(spawnCount);
            PortalSpawner();

            //This prints the 2D grid array out
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < grid.GetLength(1); i++)
            {
                for (int j = 0; j < grid.GetLength(0); j++)
                {
                    sb.Append(grid[i, j].image);
                    sb.Append(' ');
                }
                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());
        }
        //Movement 
        public void Move()
        {
            //Stores key presses
            ConsoleKey key;

            do
            {
                while (!Console.KeyAvailable)
                {
                    //
                }

                // Key is available - read it
                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.W)
                {
                    //Checks if moving will not place the player out of bounds of the array. If not, can move to that node.
                    if(!CheckIfOutOfBounds(player.xPos - 1, player.yPos))
                    {
                        if (IsWalkable(player.xPos - 1, player.yPos))
                        {
                            player.xPos--;
                            SwitchImages(player.xPos + 1, player.yPos, player.xPos, player.yPos, player.sprite);
                        }
                    }   
                }
                else if (key == ConsoleKey.S)
                {
                    if (!CheckIfOutOfBounds(player.xPos + 1, player.yPos))
                    {
                        if (IsWalkable(player.xPos + 1, player.yPos))
                        {
                            player.xPos++;
                            SwitchImages(player.xPos - 1, player.yPos, player.xPos, player.yPos,player.sprite);
                        }
                    }
                }
                else if (key == ConsoleKey.A)
                {
                    if (!CheckIfOutOfBounds(player.xPos, player.yPos - 1))
                    {
                        if (IsWalkable(player.xPos, player.yPos - 1))
                        {
                            player.yPos--;
                            SwitchImages(player.xPos, player.yPos + 1, player.xPos, player.yPos, player.sprite);
                        }
                    }
                }
                else if (key == ConsoleKey.D)
                {
                    if (!CheckIfOutOfBounds(player.xPos, player.yPos + 1))
                    {
                        if(IsWalkable(player.xPos, player.yPos + 1))
                        {
                            player.yPos++;
                            SwitchImages(player.xPos, player.yPos - 1, player.xPos, player.yPos,player.sprite);
                        }
                    }
                }
                else if (key == ConsoleKey.Spacebar)
                {
                    for(int i = player.xPos -1; i <= player.xPos + 1; i++)
                    {
                        //See if enemy present, if so attack
                        for(int  j = player.yPos -1; j <= player.yPos + 1; j++)
                        {
                            foreach(Enemy enemy in enemies)
                            {
                                if(enemy.xPos ==i  && enemy.yPos == j)
                                {
                                    enemy.health -= player.damage;
                                    Console.WriteLine(enemy.health);
                                }
                            }
                        }
                    }
                }
                else if(key == ConsoleKey.E)
                {
                    for (int i = player.xPos - 1; i <= player.xPos + 1; i++)
                    {
                        //See if enemy present, if so attack
                        for (int j = player.yPos - 1; j <= player.yPos + 1; j++)
                        {
                            if (grid[i,j].image == "PL")
                            {
                                GenerateTileGrid(15, 15);
                                score++;
                            }
                        }
                    }
                }
                EnemyMovement();

                if(player.health < 0)
                {
                    isDead = true;
                }

            } while (!isDead);
            Console.WriteLine($"You Died.\nYour score was {score}");
        }
        public bool CheckIfOutOfBounds(int x, int y)
        {
            //Checks if the location moved to is out of bounds of the array.
            if (x > gridSizeX - 1 || y > gridSizeY - 1)
                return true;
            else if (x < 0 || y < 0)
                return true;
            else 
                return false;
        }
        //Checks if the tile the player is moving to is a floor tile.
        public bool IsWalkable(int x, int y)
        {
            if (grid[x,y].image == "##" || grid[x,y].image == "#P")
            {
                return true;
            }
            return false;
        }
        //Switches the tile back to floor after the player has moved off of it. 
        public void SwitchImages(int oldX, int oldY, int newX, int newY, string newImage)
        {
            Console.Clear();
            string oldImage = grid[newX, newY].image;
            if (oldImage == "##" || grid[oldX, oldY] == grid[gridSizeX/2, gridSizeY/2])
                grid[oldX, oldY].image = "##";
            grid[newX, newY].image = newImage;

            //This prints the 2D grid array out
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < grid.GetLength(1); i++)
            {
                for (int j = 0; j < grid.GetLength(0); j++)
                {
                    sb.Append(grid[i, j].image);
                    sb.Append(' ');
                }
                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());
        }
        //Spawns enemies at random positions within the grid
        public void EnemySpawner(int maxSize)
        {
            while (maxSize < 3)
            {
                Random enemyPos = new Random();
                int row = enemyPos.Next(grid.GetLength(0));
                int column = enemyPos.Next(grid.GetLength(1));
                if (grid[row, column].tag == "walkable")
                {
                    grid[row, column].image = "EE";
                    enemies.Add(new Enemy(row, column));
                    maxSize++;
                }
            }
        }

        public void PortalSpawner()
        {
            Random portalPos = new Random();
            int i = 0;
            while (i < 1)
            {
                int row = portalPos.Next(grid.GetLength(0));
                int column = portalPos.Next(grid.GetLength(1));
                if (grid[row, column].tag == "walkable")
                {
                    grid[row, column].image = "PL";
                    grid[row, column].tag = "unwalkable";
                    i++;
                }
            }
        }
        
        public void EnemyMovement()
        {
            Random randMovement = new Random();
            for (int i = 0; i < enemies.Count; i++)
            {
                //Check each node in the grid. If node x,y <= enemyPos x+radius, y+radius|| x,y >= enemyPos x-2, y-2
                //Then check if that node contains the player. 
                //Then move towards that node
                if(MathF.Abs(player.xPos - enemies[i].xPos) <=3 && MathF.Abs(player.yPos - enemies[i].yPos) <= 2)
                {
                    Console.WriteLine("Player Spotted");
                    if(MathF.Abs(player.xPos - enemies[i].xPos) == 1 || MathF.Abs(player.yPos - enemies[i].yPos) == 1)
                    {
                        Console.WriteLine("Attack");
                        player.health -= enemies[i].damage;
                        Console.WriteLine(player.health);
                    }
                    else if(player.xPos - enemies[i].xPos == -2 && !CheckIfOutOfBounds(enemies[i].xPos-1, enemies[i].yPos) && IsWalkable(enemies[i].xPos - 1, enemies[i].yPos))
                    {
                        enemies[i].xPos--;
                        SwitchImages(enemies[i].xPos+1, enemies[i].yPos, enemies[i].xPos, enemies[i].yPos, enemies[i].sprite);
                    }
                    else if (player.xPos - enemies[i].xPos ==2 && !CheckIfOutOfBounds(enemies[i].xPos + 1, enemies[i].yPos) && IsWalkable(enemies[i].xPos + 1, enemies[i].yPos))
                    {
                        enemies[i].xPos++;
                        SwitchImages(enemies[i].xPos - 1, enemies[i].yPos, enemies[i].xPos, enemies[i].yPos, enemies[i].sprite);
                    }
                    else if (player.yPos - enemies[i].yPos == -2 && !CheckIfOutOfBounds(enemies[i].xPos, enemies[i].yPos - 1) && IsWalkable(enemies[i].xPos, enemies[i].yPos - 1))
                    {
                        enemies[i].yPos--;
                        SwitchImages(enemies[i].xPos, enemies[i].yPos + 1, enemies[i].xPos, enemies[i].yPos, enemies[i].sprite);
                    }
                    else if (player.yPos - enemies[i].yPos == 2 && !CheckIfOutOfBounds(enemies[i].xPos, enemies[i].yPos + 1) && IsWalkable(enemies[i].xPos, enemies[i].yPos + 1))
                    {
                        enemies[i].yPos++;
                        SwitchImages(enemies[i].xPos, enemies[i].yPos - 1, enemies[i].xPos, enemies[i].yPos, enemies[i].sprite);
                    }
                }
                else
                {
                    int direction = randMovement.Next(0, 4);
                    switch (direction)
                    {
                        case 0:
                            //Checks if the imaginary 'player' used as an agent for the drunkard's walk will not ove outside of the array.
                            if (!CheckIfOutOfBounds(enemies[i].xPos - 1, enemies[i].yPos) && IsWalkable(enemies[i].xPos - 1, enemies[i].yPos))
                            {
                                enemies[i].xPos--;
                                SwitchImages(enemies[i].xPos + 1, enemies[i].yPos, enemies[i].xPos, enemies[i].yPos, enemies[i].sprite);
                            }
                            break;
                        case 1:
                            if (!CheckIfOutOfBounds(enemies[i].xPos + 1, enemies[i].yPos) && IsWalkable(enemies[i].xPos + 1, enemies[i].yPos))
                            {
                                enemies[i].xPos++;
                                SwitchImages(enemies[i].xPos - 1, enemies[i].yPos, enemies[i].xPos, enemies[i].yPos, enemies[i].sprite);
                            }
                            break;
                        case 2:
                            if (!CheckIfOutOfBounds(enemies[i].xPos, enemies[i].yPos - 1) && IsWalkable(enemies[i].xPos, enemies[i].yPos - 1))
                            {
                                enemies[i].yPos--;
                                SwitchImages(enemies[i].xPos, enemies[i].yPos + 1, enemies[i].xPos, enemies[i].yPos, enemies[i].sprite);
                            }
                            break;
                        case 3:
                            if (!CheckIfOutOfBounds(enemies[i].xPos, enemies[i].yPos + 1) && IsWalkable(enemies[i].xPos, enemies[i].yPos + 1))
                            {
                                enemies[i].yPos++;
                                SwitchImages(enemies[i].xPos, enemies[i].yPos - 1, enemies[i].xPos, enemies[i].yPos, enemies[i].sprite);
                            }
                            break;
                    }
                }
                if (enemies[i].health <= 0)
                {
                    SwitchImages(enemies[i].xPos, enemies[i].yPos, enemies[i].xPos, enemies[i].yPos, "##");
                    enemies.Remove(enemies[i]);
                }
            }
        }

    }

    public class Node
    {
        public string image;
        public string[] values = {"  ","==", "||", "##"};
        public string tag;
        public int xPos;
        public int yPos;
        
        public Node(int input, int x, int y)
        {
            image = values[input];
            xPos = x;
            yPos = y;
            if(image == "##")
            {
                tag = "walkable";
            }
            else
            {
                tag = "unwalkable";
            }
        }
    
}
}