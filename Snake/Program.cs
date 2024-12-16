using System;

using System.Collections.Generic;
using System.Threading;

int snakeX = 2;
int snakeY = 2;
int snakeStartLength = 8;
int snakeLength = snakeStartLength;
string snakeDirection = "Down ";
bool gameRunning = true;

// Trail to store the snake's body positions
Queue<(int x, int y)> snakeTrail = new();

const int HORIZONTAL = 1;
const int WALL_LEFT = 2;
const int WALL_RIGHT = 3;

int[,] playArea = new int[15, 15]
{
    { 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 },
    { 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3 }
};

Random random = new Random();
(int x, int y) foodPosition = (random.Next(1, playArea.GetLength(1) - 1), random.Next(1, playArea.GetLength(0) - 1));

// Input thread to handle key presses
void InputHandler()
{
    while (gameRunning)
    {
        if (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(intercept: true).Key;
            switch (key)
            {
                case ConsoleKey.W:
                    if (snakeDirection != "Down ") snakeDirection = "Up   ";
                    break;
                case ConsoleKey.A:
                    if (snakeDirection != "Right") snakeDirection = "Left ";
                    break;
                case ConsoleKey.S:
                    if (snakeDirection != "Up   ") snakeDirection = "Down ";
                    break;
                case ConsoleKey.D:
                    if (snakeDirection != "Left ") snakeDirection = "Right";
                    break;
            }
        }
    }
}

// Start input thread
Thread inputThread = new Thread(InputHandler);
inputThread.Start();

const int gameSpeed = 200; // Game update interval in ms
long nextUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds() + gameSpeed;

long gameStart = DateTimeOffset.Now.ToUnixTimeSeconds();

// Main game loop
while (gameRunning)
{
    // Calculate the time remaining until the next update
    long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    if (currentTime < nextUpdate)
    {
        Thread.Sleep((int)(nextUpdate - currentTime));
    }

    nextUpdate += gameSpeed; // Schedule the next update

    // Update snake trail
    snakeTrail.Enqueue((snakeX, snakeY));
    if (snakeTrail.Count > snakeLength)
    {
        snakeTrail.Dequeue();
    }

    // Collect food
    if (snakeX == foodPosition.x && snakeY == foodPosition.y)
    {
        snakeLength++;
        foodPosition = (random.Next(1, playArea.GetLength(1) - 1), random.Next(1, playArea.GetLength(0) - 1));
    }

    // Update snake position
    int nextX = snakeX, nextY = snakeY;
    switch (snakeDirection)
    {
        case "Up   ": nextY--; break;
        case "Left ": nextX--; break;
        case "Down ": nextY++; break;
        case "Right": nextX++; break;
    }

    // Check for collision
    if (playArea[nextY, nextX] == HORIZONTAL || playArea[nextY, nextX] == WALL_LEFT ||
        playArea[nextY, nextX] == WALL_RIGHT || snakeTrail.Contains((nextX, nextY)))
    {
        gameRunning = false;
        continue;
    }

    // Update snake head position
    snakeX = nextX;
    snakeY = nextY;

    // Render the play area
    Console.SetCursorPosition(0, 0);
    for (int y = 0; y < playArea.GetLength(0); y++)
    {
        for (int x = 0; x < playArea.GetLength(1); x++)
        {
            if (playArea[y, x] == 1)
                Console.Write("###"); // Wall
            else if (playArea[y, x] == 2)
                Console.Write("  #");
            else if (playArea[y, x] == 3)
                Console.Write("#  ");
            else if (snakeX == x && snakeY == y)
                Console.Write(" X "); // Snake head
            else if (snakeTrail.Contains((x, y)))
                Console.Write(" o "); // Snake body
            else if (foodPosition.x == x && foodPosition.y == y)
                Console.Write(" * "); // Food
            else
                Console.Write("   "); // Empty space
        }
        Console.WriteLine();
    }

    Console.WriteLine($"Length: {snakeLength}\n" +
                      $"Direction: {snakeDirection}\n\n" +
                      $"Time Elapsed: {DateTimeOffset.Now.ToUnixTimeSeconds() - gameStart} seconds");
}

// Game Over
gameRunning = false;
inputThread.Join();
Console.Clear();
Console.WriteLine($"Game Over! You collided with a wall or yourself.\n" +
                  $"Time Alive: {DateTimeOffset.Now.ToUnixTimeSeconds() - gameStart} seconds\n" +
                  $"Foods Eaten: {snakeLength - snakeStartLength}");
