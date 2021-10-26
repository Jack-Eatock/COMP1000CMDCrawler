using System;
using System.Collections.Generic;

namespace Crawler
{
   /**
    * Simple class used to store the current coords of a character,
    * their name, symbol and stats. This class is used to manage
    * the player and monsters. And could easily be used to add more 
    * characters in future. The default values are used for the player
    * while the monsters stats are generated on creation.
    */
    public class Character // Defaults are for the player!
    {
        // Possibly health ? Speed ? or Damage ? 
        public string Name = "-NA-"; // Default name is for the monsters only
        public int CordsX = 0;
        public int CordsY = 0;
        public int Health = 8;
        public int Strength = 2; // Damage per hit 
        public char CharacterSymbol = '@';
        public char LastTile = '.';
    }

    /**
    * The main class of the Dungeon Crawler Application
    * 
    * You may add to your project other classes which are referenced.
    * Complete the templated methods and fill in your code where it says "Your code here".
    * Do not rename methods or variables which already exist or change the method parameters.
    * You can do some checks if your project still aligns with the spec by running the tests in UnitTest1
    * 
    * For Questions do contact us!
    */
    public class CMDCrawler
    {
        /**
         * use the following to store and control the next movement of the yser
         */
        public enum PlayerActions { NOTHING, NORTH, EAST, SOUTH, WEST, PICKUP, ATTACK, QUIT };
        private PlayerActions action = PlayerActions.NOTHING;

        /// Added Variables \\\
        private char[] _inPassableObjects = new char[] {'M', '#', '@' };
        private Character _player = new Character();

        // Map Management \\
        private string   _fileRoot = @"~/../../../../../Crawler/maps/";// Set to /maps on release // Every pc will have this file location. Used to make the program work on all.
        private string[] _currentMapText;   // Simply used to store the raw text format of the original version of the current map loaded.
        private char[][] _originalMap;      // Jagged array used to load and store the currently used map, Shows original.            ( NOT EDITED ) 
        private char[][] _currentMapTiles;  // Jagged array used to load and store the currently used map, Shows current state.       ( IS EDITED  ) 

        // General Variables \\
        private bool _gameRunning = false;
        private int  _objectLocationX, _objectLocationY; // Used to keep track of the coords of the current item we are interacting with.

        private bool _waitingOnKeepPlaying = false; // Boolean used when waiting for the player to answer a prompt. Pauses all uneeded inputs.

        // Enemies \\
        private List<Character> _enemies = new List<Character>(); // An array of all the enemies on the current map. 
        private int _maxMonMoveAttempts = 6; // When the monster tries to move. This is how many times they will try before giving up.
        private bool _combatActive = false;  // Shows if the user is engaged in combat. disables movement.
        private Character _opponentInCombat = new Character(); // Reference to the current monster the user is engaged in combat with.
        private string[] _monsterNames = new string[] {"Alex", "Jack" ,"Stan", "Kile", "Ewan", "Etan", "Swen" }; // Random names for the monsters.

        // Ui variables \\
        private int _goldCollected = 0; // player stats.
        private int _enemiesSlain = 0;
        private string _noticeMessage = "Type Play to begin!"; // used to easily notify the user of game events.

        /**
         * tracks if the game is running
         */
        private bool active = true;

        // Constructor, Sets the default values on launch
        public CMDCrawler()
        {
            _opponentInCombat.Health = 0;
            _opponentInCombat.Strength = 0;

        }


        /**
         * Reads user input from the Console
         * 
         * Please use and implement this method to read the user input.
         * 
         * Return the input as string to be further processed
         * 
         */
        private string ReadUserInput()
        {
            string inputRead;

            // When game is running we want it to read the key immedietely.
            if (_gameRunning) { inputRead = Console.ReadKey().Key.ToString(); }

            else  { inputRead = Console.ReadLine(); }  // Outside of play so that they can load levels etc.    .

            return inputRead;
        }

        /**
         * Processed the user input string
         * 
         * takes apart the user input and does control the information flow
         *  * initializes the map ( you must call InitializeMap)
         *  * starts the game when user types in Play
         *  * sets the correct playeraction which you will use in the GameLoop
         */
        public void ProcessUserInput(string input)
        {
            // Your Code here
            // NOTHING, NORTH, EAST, SOUTH, WEST, PICKUP, ATTACK, QUIT
            input = input.ToUpper();

            if (!GameIsRunning())
            {
                // When the player wins or looses, a prompt will show up asking if they want to keep playing.
                // This will make sure that no other input is processed other than either yes or no to the question.
                if (_waitingOnKeepPlaying) 
                {
                    switch (input)     { 
                        default: action = PlayerActions.NOTHING; break;
                        case "YES"  : _waitingOnKeepPlaying = false; restartGame(); break;
                        case "NO"   :  action = PlayerActions.QUIT; break;
                    }
                    return;
                }

                // Assuming the game is not running and we are not waiting to see if the player wants to restart. 
                // We should check for commands related to loading and starting the game. 
                switch (input){
                    default:  action = PlayerActions.NOTHING;  break;
                    case "LOAD SIMPLE.MAP":    if (InitializeMap("Simple.map"))   {  DisplayMap(_originalMap); };   break;
                    case "LOAD ADVANCED.MAP":  if (InitializeMap("Advanced.map")) {  DisplayMap(_originalMap); };   break;
                    case "PLAY": Play(); break;
                    case "QUIT": action = PlayerActions.QUIT; break;
                }
                return;
            }

            // So the game is running, we should ignore Load / play etc commands and focus on real time input.
            // Process the real time inputs to move the character appropriately and to exit out of game running if the user presses q.
            // Sets the current action approprietely.
            switch (input) { 
                default:            action = PlayerActions.NOTHING;         break;
                case "W":           action = PlayerActions.NORTH;           break;
                case "D":           action = PlayerActions.EAST;            break;
                case "S":           action = PlayerActions.SOUTH;           break;
                case "A":           action = PlayerActions.WEST;            break;
                case "E":           action = PlayerActions.PICKUP;          break;
                case "SPACEBAR":    action = PlayerActions.ATTACK;          break;        
                case "Q":          _gameRunning = false;  restartGame(); break;
            }
        }


        /**
         * - Takes in a character and checks if that character is within a 1 tile distance.
         * - Returns the cords of the tile the item is in if it is within the range.
        */

        private bool CheckIfItemIsNearby(char item, bool isPlayer = true, int startCordsX = 0, int startCordsY = 0)
        {
            bool hit = false; // This will stay false, unless the item in question is within a one tile distance.
            int x; int y;  // used to store the cords of the item in question, if the item is within range.

            if (isPlayer)  { x = _player.CordsX; y = _player.CordsY;  } // This function will defaultly be set to use the player cords.
            else           { x = startCordsX;    y = startCordsY;     } // But I repurposed the functionality to also be used for any cords. E.g monsters.         

            if      (_currentMapTiles[y - 1][x] == item)  { hit = true; y -= 1; }    // North
            else if (_currentMapTiles[y + 1][x] == item)  { hit = true; y += 1; }    // South
            else if (_currentMapTiles[y][x + 1] == item)  { hit = true; x += 1; }    // East
            else if (_currentMapTiles[y][x - 1] == item)  { hit = true; x -= 1; }    // West
            if (hit) { _objectLocationX = x; _objectLocationY = y; } // The item was found within range. Outputs true and stores the cords of the item.

            return hit;
        }

        /**
         * - Checks each tile around the player and attacks if there is a monster.
         */
        private void TryAttackNearby()
        {
            // The user is engaged in combat.
            if (_combatActive)
            {
                _opponentInCombat.Health -= _player.Strength;   // Deal appropriate damage to the enemy.
                _player.Health -= _opponentInCombat.Strength;   // Deal appropriate damage to player.
                
                // Provide a notice to the user of the damage dealt.
                _noticeMessage = $"You dealt {_player.Strength} dmg. Opponent dealt {_opponentInCombat.Strength} dmg.";

                if (_player.Health <= 0) // Checks if the player is dead. If they are end the game and ask if they want to replay.
                {
                    _noticeMessage = "You Died!";
                    _gameRunning = false;
                    _combatActive = false;
                }

                if (_opponentInCombat.Health <= 0) // Checks if the enemy engaged in combat is dead. 
                {
                    _noticeMessage = "You slayed a monster! +2 hp"; // Tell the user they won. And the reward they recieve.
                    _opponentInCombat.Name = "-NA-"; // Reset the UI. 
                    _player.Health += 2;
                    _enemiesSlain++;
                    _currentMapTiles[_opponentInCombat.CordsY][_opponentInCombat.CordsX] = '.'; // Replace the monster's character with a '.'
                    _combatActive = false; // End combat.
                    _opponentInCombat.Health = 0; _opponentInCombat.Strength = 0; // Reset the UI.
                    _enemies.Remove(_opponentInCombat); // Remove the monster from the list of monsters.
                }
                return;
            }

            // Not currently in combat so engage in combat!
            if (CheckIfItemIsNearby('M'))
            {
                // if there is a monster nearby get reeference to the Monster.
                foreach (Character monster in _enemies){
                    // Figure out which monster is at the location the user is interacting with.
                    if (monster.CordsX == _objectLocationX && monster.CordsY == _objectLocationY) {  _opponentInCombat = monster; break; } 
                }

                _combatActive = true; // This ensures that next time they press space, it will attack instead of engaging in combat.
               _noticeMessage = "Combat Started! Press Space to attack!"; // Update the notice, let the user know what is happening.
            }
        }

        /**
         * -This function should when called, Check the current tile the player is on
         * -If the tile is matches a character that can be picked up, perform
         * -the appropriate action. 
         */
        private void TryPickUp()
        {
            // If the user walks over gold and presses the interact button.
            if (_player.LastTile == 'G') 
            {
                _noticeMessage = "Gold Collected!"; // Notify the user that they collected gold.
                _goldCollected++; // Increment their gold collected.
                _player.LastTile = '.'; // replace the gold tile with a empty '.' tile
            }

            // Can add more interactables below:
        }


        /** 
         * This function resets all appropriate values and sets everything up ready to restart the game.
         * Everything should ran as if it was the first time playing. 
         */
        private void restartGame()
        {
            Console.CursorVisible = true; // Show the cusor, makes it easier for the user to know where to type.
            Console.Clear(); // Clear the screen
            // Reseting a bunch of variables.
            _enemiesSlain = 0;  _goldCollected = 0; _currentMapText = null; _enemies.Clear(); _noticeMessage = "Type Play to begin!"; _combatActive = false; _opponentInCombat.Name = "-NA-";
            Console.Write("Welcome back! Please type ");
            Console.BackgroundColor = ConsoleColor.DarkGreen; Console.Write("Load Simple.map");
            Console.ResetColor(); Console.Write(" or ");
            Console.BackgroundColor = ConsoleColor.DarkRed; Console.Write("Load Advanced.map");
            Console.WriteLine(); Console.ResetColor();

        }


        /**
         * The Main Game Loop. 
         * It updates the game state.
         * 
         * This is the method where you implement your game logic and alter the state of the map/game
         * use playeraction to determine how the character should move/act
         * the input should tell the loop if the game is active and the state should advance
         */
        public void GameLoop(bool active)
        {

            // The user is in combat, only the attack action should be considered.
            if (_combatActive) 
            {
                DisplayMap(_currentMapTiles, true); // Displays the map and Updates UI.
                if (action == PlayerActions.ATTACK) { TryAttackNearby(); } // If the user attacks, it will try to attack nearby.
                return; // We want to end the method here, otherwise the player can move while in combat.
            }

            if (!_gameRunning) { return; } // If the game is not running we have nothing further to do in this loop.

            // Handles performing the user's actions. The actions are set in ProcessUserInput
            // and performed accordingly here. 
            switch ((int) action)
            {
                case 0: break;  // Nothing      
                case 1: case 2: case 3: case 4: AttemptToMovePlayer(); MoveEnemies(); break; // Movement \\
                case 5: TryPickUp(); break;  //  PickUp  \\
                case 6: TryAttackNearby(); break;  //  ATTACK  \\
            }

            // Finally display the current map, hide the cursor and show the UI.
            DisplayMap(_currentMapTiles, true);
        }


        /**
         * This function should iterate through each monster within the _enemies array. 
         * for each monster, there should be a chance of the monster deciding to move or stay still.
         * If they choose to move, a random direction should be chosen and the monster should be 
         * moved accordingly. Replacing the M wiht a '.' and the new location with a 'M'.
         */
        private void MoveEnemies()
        {
            Character tempEnemy = new Character(); // Used to temporarily store the current monster being moved.
            Random rnd = new Random(); // This is used to generate random numbers using rnd.next().
            int newXPos = 0; int newYPos = 0; int dir = 0; // Stores the cords of the pos the monster is moving to and the direction.

            // Iterates through each of the monsters.
            for (int enemyCount = 0; enemyCount < _enemies.Count; enemyCount++)
            {
                tempEnemy = _enemies[enemyCount]; // Grabs a easy reference to the current monster to move.

                // They don't always move. Makes the game easier 
                if (rnd.Next(0, 3) == 1) { continue; }  // 1 in 3 chance of not moving

                // Choose a random direction and have them try to move.
                for (int i = 0; i < _maxMonMoveAttempts; i++)
                {
                    newXPos = tempEnemy.CordsX; // Grabs the current monster pos before moving
                    newYPos = tempEnemy.CordsY; // This can then be adjusted to find the new pos, based on their direction.

                    dir = rnd.Next(1, 5); // Choose a random num between 1 and 4. the outer limity is exlusive
                    switch (dir) {
                        case 1: newXPos += 1; break; // Adjust the cords accordingly
                        case 2: newXPos -= 1; break;
                        case 3: newYPos += 1; break;
                        case 4: newYPos -= 1; break;
                    }

                    // If the monster should move.. allow the generic movement function to handle it. Reduces code and improves readability.
                    if (SetCharacterPos(newXPos, newYPos, false, tempEnemy)) { break; }
                }
            }
        }


        /**
         * This function was made to handle what must occur when the usertypes play into the console. 
         * It needs to:
         *  - Ensure a map is loaded.
         *  - Display the original map.
         *  - Find the cords of the starting point. (S)
         *  - Replace the starting point with the player (@) and store their position.
         */

        public void Play()
        {

            // First ensure that a map is loaded.
            if (_currentMapText == null) { Console.WriteLine("No map loaded. Please load a map first!");  return;  }

            _gameRunning = true;
            // Locate the starting position for the character!
            bool flag = false; // Flag will stay false unless S is found. This will allow us to be able to confirm the S was replaced.
            int tempX = 0; int tempY = 0; // Temporarily stores the cordinates of the S.

            for (int y = 0; y < _currentMapTiles.Length; y++){
                for (int x = 0; x < _currentMapTiles[y].Length; x++){
                    if (_currentMapText[y][x] == 'S')
                    {
                        flag = true;
                        tempX = x;
                        tempY = y;
                    }
                }
            }

            // Check it was able to find the starting point!
            if (!flag) { Console.WriteLine("Unable to locate starting point !"); return; }

            // Now put the user at the starting point.
            if (!SetCharacterPos(tempX, tempY, true)) { Console.WriteLine("Failed to set Player pos!"); }

            // Locate the enemy positions and set their stats.
            Random rnd = new Random(); // This is used to generate random stats for the enemies. For example they could have a health of 5.

            // Iterate through each tile on the map looking for each 'M' character that shows up.       
            for (int y = 0; y < _currentMapTiles.Length; y++){
                for (int x = 0; x < _currentMapTiles[y].Length; x++){
                    if (_currentMapText[y][x] == 'M') // A monster was found! 
                    {
                        // We found a monster now we need to set it as a character and provide it with some stats.
                        Character tempChar = new Character();
                        tempChar.CordsX = x; tempChar.CordsY = y; // Set the monsters cords
                        tempChar.CharacterSymbol = 'M';  // Tell the monster what character it is. 
                        tempChar.Health = rnd.Next(1, 8); tempChar.Strength = rnd.Next(1, 4); // Randomising stats.
                        tempChar.Name = _monsterNames[rnd.Next(0, _monsterNames.Length)]; // Selects random name.
                        // Finally add the temp Character ( the new monster ) to the _enemies array. This will allow for easy referance later.
                        _enemies.Add(tempChar);
                    }
                }
            }

            // Finally clear the console, as the game is about to start. 
            _noticeMessage = "Goodluck!"; // Set a nice welcome notification instead of it being blank.
            Console.Clear();
 
        }

        /**
         * This function should move the player to requested coords on the map.
         *  - It must first check the map is loaded and the game is running. 
         *  - Check that the requested cords are within the maps range.
         *  - Check that the requested cords is not within a unpassable object. ( e.g wall, monster )
         *  - If this is the initial run simply put the player at the cords.
         *  - Otherwise move the player and replace the cords they were originaly on. 
         */
        public bool SetCharacterPos(int x, int y, bool isInitial = false, Character CharacterToMove = null)
        {
            bool isPlayer = false;
            if (CharacterToMove == null) { CharacterToMove = _player; isPlayer = true; }

            // First make sure the game is running and map is loaded!
            if (_currentMapText == null ||!_gameRunning) {  return false; }

            // Checking the requested coords are within range. 
            if (y > _currentMapTiles.Length || x > _currentMapTiles[y].Length)  { return false; }

            // Checking the requested cords is not an impassable object
            foreach (char obj in _inPassableObjects){
                // It is an impassable object, Output a notice message and return false.
                if (obj == _currentMapTiles[y][x]) { if (isPlayer) { _noticeMessage = "Obtruction, Unable to pass object!"; }  return false;  }
            }
    
            // Check if this is the initialisation ( Only relevant if player ) , if it is not we will also need to replace the last pos of player with empty space.
            if (!isInitial)
            {
                // Check if the tile itneracting with has an effect.
                if (_currentMapTiles[y][x] == 'E' && isPlayer)
                {
                    _currentMapTiles[y][x] = '@';
                    _currentMapTiles[CharacterToMove.CordsY][CharacterToMove.CordsX] = CharacterToMove.LastTile;
                    Console.Clear();

                    _waitingOnKeepPlaying = true;
                    GetCurrentMapState();
                    _noticeMessage = "You won! Type 'Yes' to keep playing or 'no' to quit!";
                    DisplayMap(_currentMapTiles);
                  
                    //_currentMapText = null;
                    _gameRunning = false;
                    return true;
                }
                
                _currentMapTiles[CharacterToMove.CordsY][CharacterToMove.CordsX] = CharacterToMove.LastTile;
                CharacterToMove.LastTile = _currentMapTiles[y][x];
            }
             
            // Finally move the player and store their new cords!
            _currentMapTiles[y][x] = CharacterToMove.CharacterSymbol;
            CharacterToMove.CordsX = x; CharacterToMove.CordsY = y;
            return true;
        }

        /**
         * Calculates the direction the player should move. 
         * Then works out the new cords of the player after moving and requests for the SetCharacterPos to move the player.
         */
        private void AttemptToMovePlayer()
        {
            int newX = _player.CordsX;
            int newY = _player.CordsY;

            switch (action)  {
                case PlayerActions.NORTH:  newY -= 1;   break;
                case PlayerActions.EAST:   newX += 1;   break;
                case PlayerActions.SOUTH:  newY += 1;   break;
                case PlayerActions.WEST:   newX -= 1;   break;
                default:  break;
            }
            SetCharacterPos(newX, newY);
        }


        /**
        * Map and GameState get initialized
        * mapName references a file name 
        * Create a private object variable for storing the map in Crawler and using it in the game.
        */
        public bool InitializeMap(String mapName)
        {

            bool initSuccess = false;
            Console.Clear();

            // The try is used to catch any exception when attempting to load the map.
            try
            {
                // First read the text file of the map and store it in a simple string array.
                _currentMapText = System.IO.File.ReadAllLines(_fileRoot + mapName);
  
                // Initizalise the maps.
                _originalMap = new char[_currentMapText.Length][];      // Set the Y axis of the jagged array to the number of rows in the map.
                
                for (int row = 0; row < _currentMapText.Length; row++)
                {
                    _originalMap[row] = _currentMapText[row].ToCharArray();      // Loop through each row of the map and convert into characters, store in Map jagged array.
                }

                _currentMapTiles = _originalMap;  // Initialize the second jagged array that will be used to store changes to the map.

                Console.WriteLine("Initialized Map successfully!");
                initSuccess = true;
            }

            catch (Exception)
            {
                Console.WriteLine("Failed to initialize map!");
                throw;
            }

            return initSuccess;
        }

        /**
         * Returns a representation of the currently loaded map
         * before any move was made.
         */

        public char[][] GetOriginalMap()
        {
            char[][] map = new char[0][];

            // Your code here
            Console.WriteLine("Displaying Original map...");

            // First make sure there is actually a map loaded otherwise Return an empty map 
            if (_currentMapText == null) { Console.WriteLine("There is no map loaded! Please load a map first."); return map; }

            // This creates the jagged array.
            _originalMap = new char[_currentMapText.Length][];      // Set the Y axis of the jagged array to the number of rows in the map.
            for (int row = 0; row < _currentMapText.Length; row++)
            {
                _originalMap[row] = _currentMapText[row].ToCharArray();      // Loop through each row of the map and convert into characters, store in Map jagged array.
            }

            return _originalMap;
        }

        /**
         * Returns the current map state 
         * without altering it 
         */
        public char[][] GetCurrentMapState()
        {
            // First make sure there is actually a map loaded Otherwise Return an empty map 
            if (_currentMapText == null) { Console.WriteLine("There is no map loaded! Please load a map first."); return new char[0][];  }
            return _currentMapTiles; // Return the edited map.
        }

        /**
        * This function simplifies displaying the maps. Instead of what I had before of displaying each map individually. 
        * Instead both maps are displayed using the same function. This reduces code significantly, makes it much easier 
        * to understand and allows for the Ui to be implemented much more logically.
        */
        private void DisplayMap(char[][] mapToLoad, bool SetCursor = false)
        {      
            Console.SetCursorPosition(0, 0); // Sets the cursor back to the top left. This allows the map to be overwritten, removing stuttering.
            if (Console.CursorVisible && SetCursor) { Console.CursorVisible = false; } // If SetCursor true - The cursor's visibility will be switched
            else if (!Console.CursorVisible && !SetCursor) { Console.CursorVisible = true; } // The cursor is hidden to prevent flicekring displaying the map.

            // Iterates through each row of the map, starting from the top down.
            for (int y = 0; y < mapToLoad.Length; y++)
            {
                if (mapToLoad[y].Length == 0) { continue; } // If the row is empty don't go any further.

                // Iterates through each collum of the current row. Left to right.
                for (int x = 0; x < mapToLoad[y].Length; x++)
                {
                    // Setting the appropriate colour to draw, depending on the character. E.g M would be red.
                    switch (mapToLoad[y][x]){
                        default : break;
                        case '#': Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.Black; break;
                        case 'G': Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case '.': Console.ForegroundColor = ConsoleColor.Gray; break;
                        case 'E': Console.ForegroundColor = ConsoleColor.Green; break;
                        case 'M': Console.ForegroundColor = ConsoleColor.Red; break;
                        case '@': case 'S': Console.ForegroundColor = ConsoleColor.Cyan; break;
                    }

                    // After choosing the appropriate colour for the specific character, write the character to the screen and reset the colour.
                    Console.Write(mapToLoad[y][x]); Console.ResetColor();
                }

                // Displaying UI
                // So we have been moving down the rows one by one.
                // We are currently at the end horizontally of the current row. 
                // So we can check the row we are on and add the appropriate Ui to the end of it if we please.

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                switch (y)
                {
                    case 2: Console.Write($"   [------ Player ------] ");                    Console.Write($"   [----- Opponent -----] "); break;
                    case 3: Console.Write($"   [ Player health  : { _player.Health} ] ");    Console.Write($"   [ Name     : { _opponentInCombat.Name }    ] "); break;
                    case 4: Console.Write($"   [ Gold Collected : {_goldCollected } ] ");    Console.Write($"   [ Health   : { _opponentInCombat.Health  }       ] "); break;
                    case 5: Console.Write($"   [ Enemies Slain  : { _enemiesSlain } ] ");    Console.Write($"   [ Strength : { _opponentInCombat.Strength}       ] "); break;
                    case 6: Console.Write($"   [--------------------] ");                    Console.Write($"   [--------------------] "); break;
                    case 8:
                        Console.Write(new string(' ', 50));
                        Console.SetCursorPosition(Console.CursorLeft - 50, Console.CursorTop);
                        Console.Write($"   Notice : {_noticeMessage}"); break; // This provides a nice little notification displayer to notify the user.
                }

                // After writing each character for the row in their correct colours and 
                // adding the appropriate Ui to the end of it. We can start a new line and reset the colour for the next row.
                Console.ResetColor();  Console.WriteLine();
            }
        }

        /**
         * Returns the current position of the player on the map
         * The first value is the x coordinate and the second is the y coordinate on the map
         */
        public int[] GetPlayerPosition()
        {
            int[] position = { 0, 0 };

            // First make sure there is actually a map loaded otherwise Return an empty map
            if (_currentMapText == null) { return position; }

            // Now find their location.
            for (int y = 0; y < _currentMapTiles.Length; y++)           // Loop through each row.
            {
                for (int x = 0; x < _currentMapTiles[y].Length; x++)    // Within each row loop through each character.
                {
                    if (_currentMapTiles[y][x] == '@' || _currentMapTiles[y][x] == 'S')
                    {
                        //Console.WriteLine("x" + x + " y" + y);
                        return new int[] {x, y};
                    }
                }
            }

            return position;
        }

        /**
        * Returns the next player action
        * This method does not alter any internal state
        */
        public int GetPlayerAction() { return (int) action; }

        /**
         * Returns the current gamestate. Is the game running?
         */
        public bool GameIsRunning()  { bool running = _gameRunning; return running; }

        /**
         * Main method and Entry point to the program
         * ####
         * Do not change! 
        */
        static void Main(string[] args)
        {
            CMDCrawler crawler = new CMDCrawler();
            string input = string.Empty;
            Console.WriteLine("Welcome to the Commandline Dungeon!" +Environment.NewLine+ 
                "May your Quest be filled with riches!"+Environment.NewLine);
            
            // Loops through the input and determines when the game should quit
            while (crawler.active && crawler.action != PlayerActions.QUIT)
            {
                Console.WriteLine("Your Command: ");
                input = crawler.ReadUserInput();
                Console.WriteLine(Environment.NewLine);

                crawler.ProcessUserInput(input);
            
                crawler.GameLoop(crawler.active);
            }

            Console.WriteLine("See you again" +Environment.NewLine+ 
                "In the CMD Dungeon! ");
        }
    }
}