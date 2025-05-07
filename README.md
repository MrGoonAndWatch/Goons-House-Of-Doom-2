# Goons-House-Of-Doom-2
 
## About this project

This is a sequel to a game I made in one month (~60 hours) ported over from Unity to Godot 4.4.1. While the original was a sort of self imposed month long game jam this game is being worked on at a much slower pace with only a loose estimated release date of end of October 2026.

The plan is also to collaborate with more creators on this project to create an overall similar but much more polished experience compared to the first game, including most if not all features I had to cut due to the 1 month deadline (with an all new story and setting of course).

## First Time Setup

Due to the distribution rights on some of the third party assets I use in this game there are several files intentionally not included in the repo that you will need to successfully run the project. Ultimately you should replace these with your own assets but to assist in getting the project set up I have provided some dummy files inside of the "_setup" folder at the root of this repository. Please feel free to copy, paste, and rename the empty files from here in to the following places in your cloned repository to get things running quickly:

* /audio/music/10MinutesTillBadEnd.wav
* /audio/music/Hall of Confused Clowns.wav
* /audio/sound/Pain.ogg

It is highly recommended to make this change *before* opening the project in Godot as the import files may be wiped out and replaced with incorrect default values.

## Usage / Project Contribution

This project is currently listed under CC0 and can thus be used freely for any purpose without the need for attribution. My intent is to keep it this way through release, however some third party assets may eventually be used that will require crediting the creator. If this becomes the case I will organize said resources appropriately and leave instructions on which assets require attribution.

## How to play the game

The latest tech demo build can be found here on my itchio page: https://mrgoonandwatch.itch.io/goons-house-of-doom-2

Alternatively you can clone this repository and build/run the game yourself. It's recommended to use the same version of Godot as whatever the latest main branch is using. I haven't tested other versions, but whichever version you try you'll need the .NET build as the scripts are entirely in C#. I can also say with high confidence Godot 3.x or earlier will likely not work due to some massive engine changes between these major versions.

Long term I will be upgrading this project to the latest stable 4.x version of Godot as newer versions become available.
