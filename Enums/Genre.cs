namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates the genre of an experience.
    /// </summary>
    public enum Genre
    {
        /// <summary>
        /// Unknown exeprience genre.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// A genre has not been assigned to this experience.
        /// </summary>
        None, // N/A

        // Below descriptions are provided by Roblox Corporation.
        // Props to them for fitting descriptions!
        // https://create.roblox.com/docs/production/publishing/experience-genres#genre-and-subgenre-descriptions

        // Core Genres

        /// <summary>
        /// Experiences that emphasize physical challenges and quick reflexes. They normally involve combat or other fast-paced gameplay.
        /// </summary>
        Action,

        /// <summary>
        /// Experiences focused on elements such as exploration, solving challenges, and/or interacting with characters to progress through a story or to complete a goal.
        /// </summary>
        Adventure,

        /// <summary>
        /// Experiences focused on learning specific skills or subjects.
        /// </summary>
        Education,

        /// <summary>
        /// Experiences meant to entertain through consumption or creation of content, including audio, visual, or other forms of media.
        /// </summary>
        Entertainment,

        /// <summary>
        /// Experiences where players navigate surfaces and obstacles to progress. Player actions often involve jumping, climbing, or changing directions.
        /// </summary>
        ObbyAndPlatformer,

        /// <summary>
        /// Experiences focused on casual social play with other players.
        /// </summary>
        PartyAndCasual,

        /// <summary>
        /// Experiences focused on problem-solving challenges to progress.
        /// </summary>
        Puzzle,

        /// <summary>
        /// Experiences where players embody characters in a fictional world, making choices that affect their journey. Players progress through a system of rules, like stats and abilities.
        /// </summary>
        RPG,

        /// <summary>
        /// Experiences where players immerse themselves in various roles, often with avatar customization. They emphasize creativity, social interaction, and personal expression.
        /// </summary>
        RoleplayAndAvatarSim,

        /// <summary>
        /// Experiences where players shoot ranged weapons to defeat other players or enemy units.
        /// </summary>
        Shooter,

        /// <summary>
        /// Experiences that support online shopping for users to purchase digital or real-life goods.
        /// </summary>
        Shopping,

        /// <summary>
        /// Experiences simulating real-world systems, processes, and activities. The focus is on performing specific activities like managing businesses or operating vehicles.
        /// </summary>
        Simulation,

        /// <summary>
        /// Experiences that primarily serve to connect people through hanging out, communication, or sharing.
        /// </summary>
        Social,

        /// <summary>
        /// Experiences focused on sports or racing related competition.
        /// </summary>
        SportsAndRacing,

        /// <summary>
        /// Experiences that emphasize the use of skillful thinking or strategic planning.
        /// </summary>
        Strategy,

        /// <summary>
        /// Experiences where the objective is for players to survive, escape, or defeat something that is threatening them.
        /// </summary>
        Survival,

        /// <summary>
        /// Experiences that don't fit into the other genres, including utility experiences that provide value to users in some way.
        /// </summary>
        UtilityAndOther,

        // Sub-genres

        //Action

        /// <summary>
        /// Experiences focused on combat between two or more characters. They often feature a variety of different combat mechanics.
        /// </summary>
        BattlegroundsAndFighting,

        /// <summary>
        /// Experiences that challenge a player's sense of rhythm. They most often require players to press buttons in sequence to the beat of music.
        /// </summary>
        MusicAndRhythm,

        /// <summary>
        /// Experiences where players can freely explore large worlds with an emphasis on action-oriented gameplay such as fighting and other combat.
        /// </summary>
        OpenWorldAction,

        // Adventure

        /// <summary>
        /// Experiences where players freely explore worlds. They often involve players uncovering hidden secrets, landmarks, or other unique details at their own pace.
        /// <para>Subgenre of 'Adventure'.</para>
        /// </summary>
        Exploration,

        /// <summary>
        /// Experiences where the objective is to find and collect a series of objects.
        /// <para>Subgenre of 'Adventure'.</para>
        /// </summary>
        ScavengerHunt,

        /// <summary>
        /// Experiences focused on providing players a narrative experience. They often tell a story through a series of levels, puzzles, and challenges.
        /// <para>Subgenre of 'Adventure'.</para>
        /// </summary>
        Story,

        // Education
        // <void>

        // Entertainment

        /// <summary>
        /// Experiences for listening, discovering, or creating music and audio.
        /// <para>Subgenre of 'Entertainment'.</para>
        /// </summary>
        MusicAndAudio,

        /// <summary>
        /// Experiences that act as a demo, show off an immersive environment, or highlight and portal to other experiences.
        /// <para>Subgenre of 'Entertainment'.</para>
        /// </summary>
        ShowcaseAndHub,

        /// <summary>
        /// Experiences for watching or creating video content.
        /// <para>Subgenre of 'Entertainment'.</para>
        /// </summary>
        Video,

        //Obby & Platformer

        /// <summary>
        /// Experiences where players jump between platforms to progress.
        /// <para>Subgenre of 'Obby &amp; Platformer'.</para>
        /// </summary>
        ClassicObby,

        /// <summary>
        /// Experiences where players automatically move and must avoid obstacles to continue.
        /// <para>Subgenre of 'Obby &amp; Platformer'.</para>
        /// </summary>
        Runner,

        /// <summary>
        /// Experiences where players climb upwards through a series of platforms and obstacles.
        /// <para>Subgenre of 'Obby &amp; Platformer'.</para>
        /// </summary>
        TowerObby,

        // Party & Casual

        /// <summary>
        /// Experiences recreating classic childhood games like tag or hide-and-seek.
        /// <para>Subgenre of 'Party &amp; Casual'.</para>
        /// </summary>
        ChildhoodGame,

        /// <summary>
        /// Experiences that focus on coloring or drawing as the primary gameplay mechanic.
        /// <para>Subgenre of 'Party &amp; Casual'.</para>
        /// </summary>
        ColoringAndDrawing,

        /// <summary>
        /// Experiences made up of short round-based games.
        /// <para>Subgenre of 'Party &amp; Casual'.</para>
        /// </summary>
        Minigame,

        /// <summary>
        /// Experiences centered around trivia or quizzes.
        /// <para>Subgenre of 'Party &amp; Casual'.</para>
        /// </summary>
        Quiz,

        // Puzzle

        /// <summary>
        /// Experiences focused on solving puzzles to escape a room or building.
        /// <para>Subgenre of 'Puzzle'.</para>
        /// </summary>
        EscapeRoom,

        /// <summary>
        /// Experiences where players combine items to create new ones.
        /// <para>Subgenre of 'Puzzle'.</para>
        /// </summary>
        MatchAndMerge,

        /// <summary>
        /// Experiences where players create, guess, or find words.
        /// <para>Subgenre of 'Puzzle'.</para>
        /// </summary>
        Word,

        // RPG

        /// <summary>
        /// RPG experiences focused on real-time combat.
        /// <para>Subgenre of 'RPG'.</para>
        /// </summary>
        ActionRPG,

        /// <summary>
        /// RPG experiences where players traverse an open world, often challenging the player to survive.
        /// <para>Subgenre of 'RPG'.</para>
        /// </summary>
        OpenWorldAndSurvivalRPG,

        /// <summary>
        /// RPG experiences involving turn-based combat.
        /// <para>Subgenre of 'RPG'.</para>
        /// </summary>
        TurnbasedRPG,

        // Roleplay & Avatar Sim

        /// <summary>
        /// Experiences where players take on the role of an animal in a virtual world.
        /// <para>Subgenre of 'Roleplay &amp; Avatar Sim'.</para>
        /// </summary>
        AnimalSim,

        /// <summary>
        /// Experiences centered around dressing up avatars.
        /// <para>Subgenre of 'Roleplay &amp; Avatar Sim'.</para>
        /// </summary>
        DressUp,

        /// <summary>
        /// Experiences where players create and live out scenarios that mimic everyday life.
        /// <para>Subgenre of 'Roleplay &amp; Avatar Sim'.</para>
        /// </summary>
        Life,

        /// <summary>
        /// Experiences focused on unstructured roleplay where players take on predefined characters that don't resemble their avatars.
        /// <para>Subgenre of 'Roleplay &amp; Avatar Sim'.</para>
        /// </summary>
        MorphRoleplay,

        /// <summary>
        /// Experiences where players raise or take care of a pet.
        /// <para>Subgenre of 'Roleplay &amp; Avatar Sim'.</para>
        /// </summary>
        PetCare,

        // Shooter

        /// <summary>
        /// Shooter experiences where many players all fight each other. The last player or team standing wins.
        /// <para>Subgenre of 'Shooter'.</para>
        /// </summary>
        BattleRoyale,

        /// <summary>
        /// Shooter experiences where the primary objective is eliminating the other players or team.
        /// <para>Subgenre of 'Shooter'.</para>
        /// </summary>
        DeathmatchShooter,

        /// <summary>
        /// Shooter experiences where players primarily battle against computer-controlled enemies.
        /// <para>Subgenre of 'Shooter'.</para>
        /// </summary>
        PvEShooter,

        // Shopping

        /// <summary>
        /// Experiences that help users browse and purchase avatar items.
        /// <para>Subgenre of 'Shopping'.</para>
        /// </summary>
        AvatarShopping,

        // Simulation

        /// <summary>
        /// Experiences with little to no player input or interaction.
        /// <para>Subgenre of 'Simulation'.</para>
        /// </summary>
        Idle,

        /// <summary>
        /// Experiences where progression involves simple repetitive actions to increase a counter. As players progress, they often unlock new capabilities, levels, and characters.
        /// <para>Subgenre of 'Simulation'.</para>
        /// </summary>
        IncrementalSimulator,

        /// <summary>
        /// Experiences focused on physics and interactions within the environment to simulate reactions.
        /// <para>Subgenre of 'Simulation'.</para>
        /// </summary>
        PhysicsSim,

        /// <summary>
        /// Experiences providing players with tools and resources to build and customize an environment.
        /// <para>Subgenre of 'Simulation'.</para>
        /// </summary>
        Sandbox,

        /// <summary>
        /// Experiences simulating the management of a business or base. They often involve collecting money from "droppers" used to upgrade the base.
        /// <para>Subgenre of 'Simulation'.</para>
        /// </summary>
        Tycoon,

        /// <summary>
        /// Experiences centered around driving or operating vehicles. This often includes cars, planes, boats, or other vehicles.
        /// <para>Subgenre of 'Simulation'.</para>
        /// </summary>
        VehicleSim,

        // Social
        // <void>

        // Sports & Racing

        /// <summary>
        /// Experiences featuring a racing competition, where the objective is to achieve the fastest time.
        /// <para>Subgenre of 'Sports &amp; Racing'.</para>
        /// </summary>
        Racing,

        /// <summary>
        /// Experiences focused on the practice of real-life sports.
        /// <para>Subgenre of 'Sports &amp; Racing'.</para>
        /// </summary>
        Sports,

        // Strategy

        /// <summary>
        /// Experiences inspired by traditional board and card games in real life. They typically involve a combination of luck and/or skillful thinking.
        /// <para>Subgenre of 'Strategy'.</para>
        /// </summary>
        BoardAndCardGames,

        /// <summary>
        /// Experiences where players strategically position defensive units along a path to prevent waves of enemies from reaching the end of the path.
        /// <para>Subgenre of 'Strategy'.</para>
        /// </summary>
        TowerDefense,

        // Survival

        /// <summary>
        /// Experiences where players have different roles and a win condition. Typically one player is "it" and the others need to escape or defeat them.
        /// <para>Subgenre of 'Survival'.</para>
        /// </summary>
        OneVsAll,

        /// <summary>
        /// Experiences challenging players to make a successful escape in order to survive a threat.
        /// <para>Subgenre of 'Survival'.</para>
        /// </summary>
        Escape,

        // Utility & Other
        // <void>
    }
}
