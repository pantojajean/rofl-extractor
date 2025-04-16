using System.ComponentModel.DataAnnotations;
using Fraxiinus.Rofl.Extract.Data.Models;
using Fraxiinus.Rofl.Extract.Data.Models.Rofl;
using Fraxiinus.Rofl.Extract.Data.Models.Rofl2;

namespace RoflWebExtractor.Models;

public class MatchStats
{
    [Key] public int Id { get; set; }
    public string? Match { get; set; } = string.Empty;
    public string? Tournament { get; set; } = string.Empty;

    [Required] public string? RiotGameName { get; set; } = string.Empty;

    public string? Champion { get; set; } = string.Empty;
    public string? Position { get; set; } = string.Empty;
    public string? Role { get; set; } = string.Empty;
    
    public string? Side { get; set; } = string.Empty;
    public string? Level { get; set; }

    // Estatísticas de combate
    public string?  Kills { get; set; }
    public string?  Deaths { get; set; }
    public string?  Assists { get; set; }
    public string?  DoubleKills { get; set; }
    public string?  TripleKills { get; set; }
    public string?  QuadraKills { get; set; }
    public string?  PentaKills { get; set; }
    public string?  LargestKillingSpree { get; set; }
    public string?  LargestMultiKill { get; set; }

    // Estatísticas de farm e dano
    public string?  CreepScore { get; set; }
    public string?  GoldEarned { get; set; }
    public string?  GoldSpent { get; set; }
    public string?  TotalDamageDealt { get; set; }
    public string?  TotalDamageDealtToChampions { get; set; }
    public string?  MagicDamageDealtToChampions { get; set; }
    public string?  PhysicalDamageDealtToChampions { get; set; }
    public string?  TrueDamageDealtToChampions { get; set; }
    public string?  DamageToStructures { get; set; }

    // Estatísticas de visão
    public string?  VisionScore { get; set; }
    public string?  WardsPlaced { get; set; }
    public string?  WardsKilled { get; set; }

    // Objetivos
    public string?  TurretTakedowns { get; set; }
    public string?  DragonKills { get; set; }
    public string?  BaronKills { get; set; }
    public string?  HeraldKills { get; set; }

    // Controle de grupo
    public string?  CrowdControlScore { get; set; }
    public string?  TimeCCingOthers { get; set; }

    // Tempo
    public string?  TimePlayed { get; set; }
    public string?  TotalTimeDead { get; set; }

    // Resultado
    public string? Win { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Método de conversão
    public static MatchStats FromRofl(PlayerStats stats, string matchId, string tournamentId)
    {
        return new MatchStats
        {
            Match = matchId,
            Tournament = tournamentId,
            RiotGameName = stats.Name,
            Champion = stats.Skin,
            Position = stats.TeamPosition,
            Role = stats.PlayerRole,
            Side = stats.Team,
            Level = stats.Level,

            Kills = stats.ChampionsKilled,
            Deaths = stats.NumDeaths,
            Assists = stats.Assists,
            DoubleKills = stats.DoubleKills,
            TripleKills = stats.TripleKills,
            QuadraKills = stats.QuadraKills,
            PentaKills = stats.PentaKills,
            LargestKillingSpree = stats.LargestKillingSpree,
            LargestMultiKill = stats.LargestMultiKill,

            CreepScore = stats.MinionsKilled,
            GoldEarned = stats.GoldEarned,
            GoldSpent = stats.GoldSpent,
            TotalDamageDealt = stats.TotalDamageDealt,
            TotalDamageDealtToChampions = stats.TotalDamageDealtToChampions,
            MagicDamageDealtToChampions = stats.MagicDamageDealtToChampions,
            PhysicalDamageDealtToChampions = stats.PhysicalDamageDealtToChampions,
            TrueDamageDealtToChampions = stats.TrueDamageDealtToChampions,
            DamageToStructures = stats.TotalDamageDealtToTurrets,

            VisionScore = stats.VisionScore,
            WardsPlaced = stats.WardPlaced,
            WardsKilled = stats.WardKilled,

            TurretTakedowns = stats.TurretTakedowns,
            DragonKills = stats.DragonKills,
            BaronKills = stats.BaronKills,
            HeraldKills = stats.RiftHeraldKills, // ou outra fonte de dados

            CrowdControlScore = stats.TotalTimeCrowdControlDealt,
            TimeCCingOthers = stats.TimeCCingOthers,

            TimePlayed = stats.TimePlayed,
            TotalTimeDead = stats.TotalTimeSpentDead,

            Win = stats.Win
        };
    }
    
      public static MatchStats FromRofl2(PlayerStats2 stats, string matchId, string tournamentId)
    {
        return new MatchStats
        {
            Match = matchId,
            Tournament = tournamentId,
            RiotGameName = $"{stats.RiotIdGameName}#{stats.RiotIdTagLine}",
            Champion = stats.Skin,
            Position = stats.TeamPosition,
            Role = stats.PlayerRole,
            Side = stats.Team,
            Level = stats.Level,

            Kills = stats.ChampionsKilled,
            Deaths = stats.NumDeaths,
            Assists = stats.Assists,
            DoubleKills = stats.DoubleKills,
            TripleKills = stats.TripleKills,
            QuadraKills = stats.QuadraKills,
            PentaKills = stats.PentaKills,
            LargestKillingSpree = stats.LargestKillingSpree,
            LargestMultiKill = stats.LargestMultiKill,

            CreepScore = stats.MinionsKilled,
            GoldEarned = stats.GoldEarned,
            GoldSpent = stats.GoldSpent,
            TotalDamageDealt = stats.TotalDamageDealt,
            TotalDamageDealtToChampions = stats.TotalDamageDealtToChampions,
            MagicDamageDealtToChampions = stats.MagicDamageDealtToChampions,
            PhysicalDamageDealtToChampions = stats.PhysicalDamageDealtToChampions,
            TrueDamageDealtToChampions = stats.TrueDamageDealtToChampions,
            DamageToStructures = stats.TotalDamageDealtToTurrets,

            VisionScore = stats.VisionScore,
            WardsPlaced = stats.WardPlaced,
            WardsKilled = stats.WardKilled,

            TurretTakedowns = stats.TurretTakedowns,
            DragonKills = stats.DragonKills,
            BaronKills = stats.BaronKills,
            HeraldKills = stats.RiftHeraldKills, // ou outra fonte de dados

            CrowdControlScore = stats.TotalTimeCrowdControlDealt,
            TimeCCingOthers = stats.TimeCCingOthers,

            TimePlayed = stats.TimePlayed,
            TotalTimeDead = stats.TotalTimeSpentDead,

            Win = stats.Win
        };
    }
}