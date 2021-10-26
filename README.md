# Jack Eatock - 2020-COMP1000-CW2 - Dungeon Crawler
> This game was built for my Software Engineering Module during my first semester at University Of Plymouth. 

I was given the task to build a Command Line based Dungeon Crawler. In summary the aim of the game is to navigate your player through a maze of monsters with the purpose of reaching the exit. The player can earn bonus points by collecting gold, however this comes with the added risk of engaging in combat with more monsters risking loosing the game. The second section of the document linked below states the requirements:
[Link](https://dle.plymouth.ac.uk/pluginfile.php/2082435/mod_resource/content/0/Comp1000Assignments.pdf)

## Video
[Video](https://www.youtube.com/watch?v=qsuRoKqpuBY&ab_channel=JackEatock)

## Controls 
* W - Move North
* A - Move West
* S - Move South
* D - Move East
* E - Pickup Gold
* Space - Attack Enemy. 

## Commands

* load simple.map - loads the simple map.
* load advanced.map - loads the advanced map.
* play - Starts the game using the currently loaded map.
* quit - Exits the game.

# How to interact with the executable? 

There are two ways to intereact with the executable, the first method has been added to simplify the process. 

1. Simply download "CrawlerLauncher.zip" 
   ![Image](https://i.gyazo.com/4abedf48c7bda7635e1e58bd8e096cc3.png)
2. Extract the compressed file. 
3. Run **"Crawler.exe"** either by **double clicking** on it, or by navigating to the appropraite file directory in the command line using **cd "The file directory".** Then typing **start Crawler.exe* to run the executable.  

Or alternatively you can:
1. Download / clone the entire repositry 
2. navigate to: **2020-comp1000-cw2-Jack-Eatock\Crawler\bin\Release\netcoreapp3.1**
3. Run **"Crawler.exe"** either by **double clicking** on it, or by navigating to the appropraite file directory in the command line using **cd "The file directory".** Then typing **start Crawler.exe** to run the executable.  

# What does the game currently look like? 

This shows after loading Simple.map:
![ImageSimpleMap](https://i.gyazo.com/6cbf864a7070208465f0973b6dad3af7.png)

This shows the combat with monsters. You can see both the player and monster stats are showcased on the right.
![ImageSimpleMapCombat](https://i.gyazo.com/74f1edfd786954aa6456511ecdda9203.png)

This shows after loading Advanced.map:
![ImageAdvancedMap](https://i.gyazo.com/591fe21d6446f9fdcfdc0d7948f9f674.png)

This shows after you completed the game on the advanced.map. You can see it asks the user if they would like to keep playing or exit.
![ImageSimpleMapExit](https://i.gyazo.com/d2d9a261a59cddaead8ce89fd07a9fd4.png)

## Have I met the requirments? : 

### Basic:
- [x] The user needs to be able to start the project from command line by simply calling the executable name, e.g. “Crawler.exe”.
- [x] The user needs to be able to pick and load a local map file from the maps folder by typing in “load Simple.Map” followed by ENTER.
- [x] The user can play a loaded map by typing in “play” followed by ENTER.
- [x] Your software needs to be able to load and display the “Simple.map” on a 2D grid.
- [x] The player always starts on “S“. 
- [x] Using “WASD” the user should be able to move and complete the map by entering the “E” tile.
- [x] All the map elements are appropriately added.

### Advanced:
- [x] "Using SPACE the player should be able to attack a position dealing 1 damage to a Monster" - The player is engaged in combat and can deal 2 damage/ attack.
- [x] "Using “E” the player can pick up gold when standing ontop of it. " - the gold collected is also shown in the UI.
- [x] Players do not need to press ENTER when making a move. 
- [x] "When player moves over a tile the tile is hidden and the player symbol is rendered until player leaves the tile." - This is shown when walking over a gold tile.
- [x] "The map can be replayed." - it will give you the option of selecting a map, or quiting the game. " Would you like to keep playing? "
- [x] "Monster may have more than 1 damage point. " - Each monster's stats are randomly generated. Some deal loads of damage but have little health etc.


## Extra functionality I have decided to add: 

The current game state clearly shows the different characters within the game by seperating them by colours. For example the Monsters have been made an intimidating dark Red,  making it easy for the user to spot their location from their surroundings.

I have also added a very intuitive ui on the right side of the display, this shows the players stats  health, gold collected and enemies slain. The ui also shows the enemies stats when in combat making the combat very easy to understand and updates the notice at the bottom with the most recent notifications, such as "Enemey slain" etc. 

Finally I have added a little option after finishing the game that allows the player to restart if they please.

## How is my code structured?

I structure my data types based on accessibility. This helps me to quickly understand what scope a variable is and continue accordingly. 

* Private variable  - Underscore + camelCase
* Public  variable  - PascalCase 
* Local   variables - camelCase

I also tried to take advantage of the power of using classes. I did this by creating a class called Character, it contained all variables a moveable character in the game would have, such as health, cordinates, strength, name etc. I then was able to set the player as a "Character" and also create an array of Character's in order to manage all the monsters. I think this was a very benefitial as it drastically reduced the length and complexity of my code. You can easily iterate through each monster and grab their cordinates.. or health etc.

## What resources have I used? 

During the development of this game, I very occassionally used a couple different resources. They are listed below: 

* [How to use markdown](https://guides.github.com/features/mastering-markdown/)
* [How to generate a random number](https://docs.microsoft.com/en-us/dotnet/api/system.random?view=net-5.0)
* [Updating the console screen without tearing](https://docs.microsoft.com/en-us/dotnet/api/system.console.setcursorposition?view=net-5.0)

...............................................................................................................................................................................
