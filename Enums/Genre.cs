using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
    public enum Genre
    {
        Unknown = -1,

        // Core Genres
        None, // N/A
        Action,
        Adventure,
        Education,
        Entertainment,
        ObbyAndPlatformer,
        PartyAndCasual,
        Puzzle,
        RPG,
        RoleplayAndAvatarSim,
        Shooter,
        Shopping,
        Simulation,
        Social,
        SportsAndRacing,
        Strategy,
        Survival,
        UtilityAndOther,

        // Sub-genres

        //Action
        BattlegroundsAndFighting,
        MusicAndRhythm,
        OpenWorldAction,

        // Adventure
        Exploration,
        ScavengerHunt,
        Story,

        // Education
        // <void>

        // Entertainment
        MusicAndAudio,
        ShowcaseAndHub,
        Video,

        //Obby & Platformer
        ClassicObby,
        Runner,
        TowerObby,

        // Party & Casual
        ChildhoodGame,
        ColoringAndDrawing,
        Minigame,
        Quiz,

        // Puzzle
        EscapeRoom,
        MatchAndMerge,
        Word,

        // RPG
        ActionRPG,
        OpenWorldAndSurvivalRPG,
        TurnbasedRPG,

        // Roleplay & Avatar Sim
        AnimalSim,
        DressUp,
        Life,
        MorphRoleplay,
        PetCare,

        // Shooter
        BattleRoyale,
        DeathmatchShooter,
        PvEShooter,

        // Shopping
        AvatarShopping,

        // Simulation
        Idle,
        IncrementalSimulator,
        PhysicsSim,
        Sandbox,
        Tycoon,
        VehicleSim,

        // Social
        // <void>
        
        // Sports & Racing
        Racing,
        Sports,

        // Strategy
        BoardAndCardGames,
        TowerDefense,

        // Survival
        OneVsAll,
        Escape,

        // Utility & Other
        // <void>
    }
}
