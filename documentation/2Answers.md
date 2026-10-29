1. The pattern display will be shown in a row.  
2. The pattern will be divided up by sets by a character (I will demonstrate by using an asterisk)  
   In this example, a zero will represent one of the squares, and the asterisk will represent the break point.

   Example of two sets: 0000\*0000

   A row can have no more than 3 sets on them so if a patter has 6 sets, it will look like this:  
   0000\*0000\*0000  
   0000\*0000\*0000

3. Memory time will represent the amount of time the pattern will linger on screen before the player can input the pattern. Keep in mind that break points are more useful during the input phase than the memory phase, and at this point are mainly there to break up the patterns into smaller chunks.  
4. The player will reproduce the color sequence, they can reproduce it by either clicking the buttons on the screen, or by pressing the keys that they have chosen for their input method, which is possible through settings.  
5. The input timer is counted down for each level across the whole pattern. Whenever the player reaches a break point, the player will be allotted some time left on that timer. This is why break points are important to the input phase.

   In the example of a level with 6 sets, the player would reach 4 break points, but the input time would be counted down over the course of all 6 sets.  
6. When a player gets an incorrect guess, they will lose HP and have to restart the current set. This way it isn’t too punishing if they get something wrong.  
7. The player will start with a Maximum HP of 1\. This means that when they get one guess wrong, it’s an immediate game over. However, it is designed this way so it can scale upwards when the game gets more difficult. The player will be able to upgrade and get more maximum HP through the store. You can think of it as a mistake counter, but HP is more digestible for the players.  
8. There is HP regeneration. At the start, the HP regen will take place after a player has beaten an entire level, but future upgrades will reduce that to a certain number of sets instead. The final available upgrade will allow for Regen after 1 completed set, but it’s very expensive to get to that point. That would occur on the break point, which is another reason why they are critical to the input phase.  
9. A lot happens during the break point. The things we’ve discussed so far are: a brief time allotment for the player, a correctly inputted set will add the multiplier, possible HP Regeneration, it provides a start/stop moment for the player so they don’t have to try and input the entire pattern all over again, and so on. There will probably be more important information about break points in the future. Something to note here though, is that breakpoints are not a stopping point. There will likely be a sound cue to let you know that you’ve hit it, but the player won’t have to wait for an animation or confirmation to keep inputting their pattern. It’s solely there to help the player, not to hinder them.  
10. Inputs made during the memory phase, or a different phase, should be ignored entirely. The player should feel free to press buttons, with no expectation of penalty. Since this is a memory game, it’s implicitly encouraged to use every tool possible to perform the task.  
11. A level is determined by the number of Sets it has. Level one just has 1 set, level 2 has 2 sets, and so on. I may change that in later versions, but it will be best to keep it simple for this version. After a level is completed, the player will have the option to make purchases in the shop, similarly as if they had a game over. The difference however, is that a game over will send them back to the beginning, whereas with this they’ll be able to make purchases during their current run.  
12. The player will be able to save their game and quit a run at any time.  
13. For version 0 I want to make 15 base levels. We can discuss level scaling past that point in an infinite way.

    To go back to an earlier example, this is what level 15 will look like.  
    (0 \= colored square, \*=break point)  
    0000\*0000\*0000

	0000\*0000\*0000  
	0000\*0000\*0000  
	0000\*0000\*0000  
	0000\*0000\*0000

14. Again, at this point, I just want to make the working version that we have into version 0\. Additional difficulty scaling will need to be addressed past that point so I can have a more clear cut answer.  
15. The pattern should truly be random, which means that consecutive colors may appear.  
16. Yes.  
17. Patterns are completely random for the player. They will need to remember colors, so I don’t think anything is off the table.  
18. I do think we need to add a seed feature, but only for debugging, just so I can see what happens at the end of the game before launch.  
19. Bits scale upward exponentially. The first level will award 1 bit for the completed set, level 2 will award 2 bits, but level 3 will award 4, then level 4 8 bits, and so on.  
20. The multiplier is based on consecutively completed sets. These will carry over from level to level. This way someone can have some momentum after the first few levels.  
21. The multiplier will reset after losing HP, which is done through an incorrect guess. Auto complete is incredibly useful since the multiplier will still scale up if you use it.  
22. Fractional bits will remain in your score, but will not be very useful, since store items are sold at whole numbers. It remains useful since you can keep that momentum from level to level.  
23. Bits are awarded for each completed level.  
24. Bits will be permanently saved due to the rogue-like nature of the game. When you’re hit with a game over, you’re prompted to spend the bits in the store.  
25. The player won’t intentionally be penalized, but any unsaved game closing will result in lost bits.  
26. The completion of a level will award bits, but it will not add to the multiplier.  
27. Higher levels award a higher amount of bits earned. The goal however, is to simply beat the game.  
28. Store purchases are permanent in a given save file. There is only one store purchase that can adjust back down, and it is the starting level upgrade.  
29. Any upgrade purchased will be effective the moment it is purchased and it will remain that way until a new game is started or that save file is deleted.  
30. The player may purchase as many upgrades they desire in-between levels, so long as they have the bits required.  
31. Yes please refer to this chart:  
    \-Input Time: 50  
    \-Memory Time: 20  
    \-HP: 25  
    \-Regen: 10  
    \-Cached Memory: 10  
    \-Auto-Complete: 5  
    \-Auto Cooldown: 5  
    \-Breakpoint: 5  
    \-Show: 3  
    \-Starting Level: n/a (15 is the max level for now)  
32. For now it will be an increased price metric, but I don’t have the details on what the pricing will be for each item in the store since the items don’t have an equal amount of purchases available.  
33. Starting level is an upgrade that will allow the player to increase what level they start on. This is the only upgrade that has a feature to adjust the level back down once purchased. It is, however, quite expensive since it puts you at a serious advantage.  
34. If a level is too difficult, the player can adjust it back down. This is why that mechanic is relevant. A player won’t likely be able to afford the first upgrade for this until at least the mid-game to early late-game.  
35. Cached Memory is a mechanic that allows a certain number of sets to linger in the pattern area(slightly opaque), so the player won’t have to remember those sets. Cached memory works forward, meaning that the first sets will have cached memory applied. Let’s use our level 15 example again.  
    (0 \= Colored Square, 1 \= Cached memory(which shows the correct pattern, \* \= breakpoint)  
    Cached Memory level 1 \= 1 set is visible  
    1111\*0000\*0000  
    0000\*0000\*0000  
    0000\*0000\*0000  
    0000\*0000\*0000  
    0000\*0000\*0000

36. Auto Complete is a mechanic that allows a certain number of sets to be completed automatically at the press of a button. This number is determined by how many upgrades you’ve purchased for this ability. It will add to the multiplier like a regularly completed set. This mechanic does consume a charge, which is what Auto Cooldown is referencing. Here is an example of what Auto Complete could look like.  
    (0 \= Colored Square, 1 \= Auto Complete(which shows how many sets are available to complete automatically, \* \= breakpoint)  
    Auto Complete level 2 \= able to complete 2 sets automatically on one charge  
    1111\*1111\*0000  
    0000\*0000\*0000  
    0000\*0000\*0000  
    0000\*0000\*0000  
    0000\*0000\*0000

37. Auto Cooldown reduces the amount of time it takes to use the Auto complete mechanic again. Auto Complete’s cool down is counted by levels. This makes it very rare to use, and only the final upgrade will allow for Auto complete usage once per level.  
38. Auto complete can only be used during the input phase.  
39. Show is a mechanic that will allow you to see the pattern one more time from the current set that you’re on.  
40. The amount of times you can use Show is dependent on what level of upgrade you have. The first purchase allows you to use Show once per level. Each upgrade further will increase the number of uses per level.  
41. The regen upgrade increases the speed at which you regain health. This is also counted by the number of sets. Each upgrade will decrease the number of sets before the next regen happens.  
42. A breakpoint upgrade will increase the time added to your input timer. Every time you hit a break point, a small amount of time is regained on your input timer.  
43. For simplicity, the upgrade effects are merely additional. This way it doesn’t explode off exponentially.  
44. For version 0, we simply want to create a version that works on my machine. We’ll worry about packaging in later developments.  
45. I’m currently running .NET version 10, I think. We’ll develop on my current .NET version.  
46. No, the game will support adding your own hot keys for input. It will also accept mouse inputs on screen.  
47. Yes, the numeric key row should be supported.  
48. Mouse will be supported, and touch screens may be supported in future versions.  
49. That recommendation is the right inclination, since it follows the number flow.  
    NumPad 7  NumPad 8  NumPad 9  
    NumPad 4  NumPad 5  NumPad 6  
    NumPad 1  NumPad 2  NumPad 3  
      
50. Num lock should not be required to work on the num pad.  
51. If there is no way to run the game using num pad without Num Lock, then the game should throw up a warning message for the player.  
52. The num pad layout should match the physical keyboard layout, this way the game remains consistent, no matter which way the player would like to play.  
53. Yes the game should absolutely support remappable controls.  
54. The menu and store should be controlled via mouse.  
55. The esc. Key should handle the pause button, but the pause menu should be controlled via mouse.  
56. Multiple inputs via holding a key down should never be accepted, since it can only run into problems down the line. You would also lose the satisfaction of individual inputs during the game loop.  
57. There is a title screen, see styling requirements down below  
58. There will be a tutorial, but we won’t need to worry about that in version 0  
59. Pausing is supported by pressing esc.  
60. If you pause, then the timers will stop. That is something that we can address in later versions however, since it disrupts the flow of gameplay.  
61. You will hit a game over screen when you lose, then be automatically directed to the store page before the next run begins. If you choose to go to the menu then you won’t need to be directed to the store page.  
62. Yes, but only by pressing the esc. Key to pause the game.  
63. There will be a summary page, see the styling requirements down below.  
64. For version 0, the screen transitions should be instant. We’ll worry about transitions in later versions.

Styling Requirements that should satisfy 65-75:  
The styling of version 0 will be fairly simple. There will be 4 pages that we’ll need to build.  
	\-Start Menu  
	\-Settings Menu  
	\-Leader Board Screen  
	\-Game Screen  
	\-Store Menu  
	\-Game Over  
Global:  
	Header Font: Wallpoet:   
	(this example is for HTML, but if it is not possible to use that in Monogame Framework then we’ll find a different solution)  
\<link rel="preconnect" href="https\://fonts.googleapis.com"\>  
\<link rel="preconnect" href="https\://fonts.gstatic.com" crossorigin\>  
\<link href="https\://fonts.googleapis.com/css2?family=Wallpoet\&display=swap" rel="stylesheet"\>

Body Font: Arial

Placement: Across all pages, the content needs to be in the center of the window with generous padding of the left and right.

Start Menu:  
Text:  
	\-Min-OS (Header Font)  
	\-Start Game (Option: Body Font)  
	\-Settings (Option: Body Font)  
	\-Leader Board (Option: Body Font)  
	\-Quit (Option: Body Font)

Colors:  
	Background Color: \#28517a  
	Text Color: \#4372a1  
	Text Border Color: \#ffffff

	(The Text Border shouldn’t be too thick, but thick enough to add emphasis.)

Settings Menu:  
	Text: (All Text should be Body Font)  
		\-Option 1:(\*This is where the player can input their hot keys)  
		\-Option 2:(\*This is where the player can input their hot keys)  
		\-Option 3:(\*This is where the player can input their hot keys)  
		\-Option 4:(\*This is where the player can input their hot keys)  
		\-Option 5:(\*This is where the player can input their hot keys)  
		\-Option 6:(\*This is where the player can input their hot keys)  
		\-Option 7:(\*This is where the player can input their hot keys)  
		\-Option 8:(\*This is where the player can input their hot keys)  
		\-Option 9:(\*This is where the player can input their hot keys)  
		\-Music Volume:  
		\-Sfx Volume:  
	  
	Colors:  
		  
		Background Color: \#28517a  
		Text Color: \#4372a1  
		Text Border Color: \#ffffff

	(The Text Border shouldn’t be too thick, but thick enough to add emphasis.)

Leader Board Screen: (Place all of the information in a table format)

	Text:(All Text should be Body Font, but subheaders should be boldened)  
		\-Completed Sets: \[Displayed Variable\]  
		\-Number of Game Overs:  
		\-Number of Inputs:   
		\-Highest Level:   
		\-Total Bits Earned: 

	Colors:  
		  
		Background Color: \#28517a  
		Text Color: \#4372a1  
		Text Border Color: \#ffffff

	(The Text Border shouldn’t be too thick, but thick enough to add emphasis.)

Store Menu: (Place all of the information in a table format)

	Text:(Text should be Boldened, and the number should be represented by an incremental slider composed of the number of purchases you can make for a given item.)  
\-Input Time: 50  
\-Memory Time: 20  
\-HP: 25  
\-Regen: 10  
\-Cached Memory: 10  
\-Auto-Complete: 5  
\-Auto Cooldown: 5  
\-Breakpoint: 5  
\-Show: 3  
\-Starting Level: n/a

Colors:  
	Background Color: \#3b3da1  
	Text Color: \#3337f5  
	Text Border Color: \#ffffff

Game Over:  
	Text:  
		\-Game Over(Header Font)  
		\-Final Score: \[Displayed Variable\](Boldened Body Font)  
		\-Try Again?(Body Font: Option)  
		\-Quit to Menu(Body Font: Option)

	Colors:  
		Background Color: \#8630a6  
		Text Color: \#a11b57  
		Text Border Color: \#ffffff

Game Screen:

	Memory Line: This is where the pattern will flash in front of the player. The sequence will be displayed by small pngs of the color they will need to select. This should be in the top third of the window. Each set will be separated by a “break point” which will appear as a bullet point.

This is the example I keep showing:

0000\*0000\*0000  
0000\*0000\*0000  
0000\*0000\*0000  
0000\*0000\*0000  
0000\*0000\*0000

IMPORTANT: The pattern will be shown on this memory line, and will be visible during the allotted memory time. Once the game moves to the input phase, the visible pattern will become invisible.

	Game Pad: This is where the colored buttons will appear. It should be in the bottom two thirds of the screen. Each button should be evenly spaced from each other and it should be placed in a 3x3 grid.

	Button: These are what make up the Game Pad, there are 9 colored buttons. I will eventually provide a png of what the button will look like which will have a color of \#ff0000, but for now, create regular squares with the given colors. Instead of making 9 different colored pngs, the game will take that png and change the hue of the button to match the following colors:

\#ae00ff  
\#ff00ff  
\#0000ff  
\#00ffff  
\#00ff00  
\#ffff00  
\#ff7300  
\#ffffff

76\. Music will not be required for version 0, however, we should create an audio manager to make sure it can be expanded later on.  
77\. Each color should have a sound attached to it, and played during a key press.  
78\. I will create multiple mp3 files for each sound. There will be 9 sounds corresponding with each button.  
79\. Sound cues may be disabled and will be grouped with sound effect(sfx)  
80\. Audio timing won’t be integral to the gameplay.  
81\. Since, a run will have all of the store data linger across attempts, then the save file will save all of that.  
82\. There will be 3 save files available for the player.  
83\. Save files will be found when directing from the Title screen to the Game screen  
84\. If a file is corrupted, then just delete it. We will find a way to more gracefully deal with corrupted files in future versions.  
85\. Crash recovery won’t be necessary until future versions  
86\. All settings should persist when the game closes and re-opens. Save files, also need to persist.  
87\. Save data may be possible on version 0, but I’m also unaware of how difficult it is to implement save files.  
88\. I’m not sure which of these to use, but we should develop with the newer workflow so future versions are supported.  
89\. Game balance should live in C\#, and not too easily changed through JSON files.  
90\. I’m not sure what test frameworks are available. This will be handled with a QA AI agent.  
91\. Automated tests should handle the game logic, but will again be handled by the QA agent.  
92\. I’m not sure on this point.  
93\. Debug mode will be needed for manual testing.  
94\. None of these will be necessary for this version.