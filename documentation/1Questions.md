# Min-OS Contextual Questions

This file records the 94 contextual questions used to clarify the Min-OS project plan.

## Gameplay and pattern rules

1. What does the display look like during a pattern: four squares in a row, a 2x2 grid, four independent squares, or another layout?
2. How is a pattern divided into sets, and does the player complete each four-color set separately or reproduce the whole level continuously?
3. Is Memory Time the total duration for a set, the duration per color, or something else?
4. Does the player reproduce only the color sequence, or positions as well?
5. Is Input Time measured per color, per set, or across the whole level?
6. What happens after a wrong input: lose HP and continue, lose HP and restart the set, or restart the level?
7. What exactly is HP: a mistake counter, a regenerating resource, or both?
8. What triggers HP regeneration, and how often can it occur?
9. What happens during a breakpoint, and can the player continue without waiting for confirmation?
10. What happens when a key is pressed during an invalid phase such as memory display or a transition?
11. What happens after completing a level?
12. Can the player voluntarily exit a run?
13. What is the maximum level for version 0 and how is later scaling handled?
14. How does difficulty scale besides the number of sets?
15. Can the same color appear consecutively?
16. Can the same color appear at a set boundary?
17. Are patterns completely random, including difficult combinations?
18. Should runs use a debug-only seed?

## Scoring and currency

19. What is the base number of bits awarded per completed level/set?
20. Is the multiplier based on consecutive sets across the entire run and across levels?
21. When does the multiplier reset?
22. How are fractional bit rewards stored and displayed?
23. Are bits awarded per set or per completed level?
24. Are bits permanently banked after game over?
25. What happens to unsaved bits when the game closes?
26. Does completing a level award a bonus or only the normal level reward?
27. What purpose do higher-level rewards serve besides helping the player beat the game?

## Store and upgrades

28. Are store purchases permanent within a save file?
29. Do purchases apply immediately, only on the next run, or both?
30. Can players buy unlimited upgrades between levels as long as they can afford them?
31. What are the maximum purchase counts for each upgrade?
32. How do store prices increase?
33. What exactly does Starting Level do?
34. How can the player lower Starting Level after upgrading it?
35. How does Cached Memory work and which sets remain visible?
36. How does Auto-Complete work and how many sets does one charge complete?
37. What does Auto Cooldown reduce and how is cooldown measured?
38. During which phase can Auto-Complete be used?
39. What does Show reveal?
40. How many Show uses are available per level?
41. What exactly does the Regen upgrade change?
42. What exactly does the Breakpoint upgrade change?
43. Are upgrade effects additive or multiplicative?

## Platform and input

44. What platform is required for version 0?
45. What MonoGame and .NET versions should be targeted?
46. Is the numpad the only supported gameplay input?
47. Should the number row be supported?
48. Should mouse, gamepad, or touch input be supported?
49. What is the numpad-to-color mapping?
50. Is Num Lock required?
51. Should the game warn the player if Num Lock is required by the platform?
52. Should the on-screen keypad match the physical keypad layout?
53. Should controls be remappable?
54. Should menus and the store be mouse-controlled?
55. What key pauses the game and how is the pause menu controlled?
56. Should holding a key ever create repeated inputs?

## Screens and UI

57. Is there a title screen?
58. Is there a tutorial in version 0?
59. Is pausing supported?
60. Do timers stop while paused?
61. What is the game-over-to-store flow?
62. How can the player return to the menu from a run or store?
63. Is there a summary/leaderboard page and what does it display?
64. Should screen transitions be instant in version 0?
65. What is the art direction?
66. What fonts should be used?
67. What colors, text borders, and page layouts should be used?
68. What should the Start Menu contain?
69. What should the Settings Menu contain?
70. What should the Leader Board display?
71. What should the Store Menu display?
72. What should the Game Over screen display?
73. What should the Game Screen layout be?
74. How should the memory line render sets and breakpoints?
75. How should the nine-button game pad render and support color accessibility?

## Audio

76. Is music required for version 0?
77. Should each color have a sound cue?
78. Are there separate sound assets for each color?
79. Can SFX be disabled independently from music?
80. Is audio timing gameplay-critical?

## Persistence and development

81. What data must be saved?
82. How many save files are supported?
83. When are save files selected or created?
84. What should happen if a save file is corrupt?
85. Is crash recovery required?
86. Which settings and progression values persist across launches?
87. Is save-data versioning required for version 0?
88. Which MonoGame content workflow should be used?
89. Should balance be stored in code or external data?
90. Which test framework should be used?
91. What should automated tests cover?
92. What platforms/configurations must be tested before release?
93. What debug tools are needed?
94. Are telemetry, run statistics, or replay recording required?
