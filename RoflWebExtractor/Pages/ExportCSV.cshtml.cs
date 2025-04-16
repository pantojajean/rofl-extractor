using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fraxiinus.Rofl.Extract.Data;
using Fraxiinus.Rofl.Extract.Data.Models;
using System.Text.Json;
using RoflWebExtractor.Data;
using RoflWebExtractor.Models;

namespace RoflWebExtractor.Pages;

[Authorize]
public class ExportCSV : PageModel
{
    private readonly ILogger<ExportCSV> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _context;

    public string? ErrorMessage { get; set; }

    public ExportCSV(ILogger<ExportCSV> logger, IWebHostEnvironment environment, AppDbContext context)
    {
        _logger = logger;
        _environment = environment;
        _context = context;
    }

    public void OnGet()
    {
    }

    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var files = Request.Form.Files;
            var matchList = new List<MatchStats>();

            if (files == null || files.Count == 0)
            {
                ErrorMessage = "Por favor, selecione pelo menos um arquivo ROFL.";
                return Page();
            }

            const long maxFileSize = 500 * 1024 * 1024; // 100MB
            if (files.Sum(f => f.Length) > maxFileSize)
            {
                ErrorMessage = "Arquivos excedem o tamanho máximo de 100MB.";
                return Page();
            }

            var csvData = new StringBuilder();
            csvData.AppendLine(
                "PLAYER,ASSISTS,CHAMPIONS_KILLED,MINIONS_KILLED,NUM_DEATHS,PENTA_KILLS,SKIN,TEAM,TEAM_POSITION,TIME_PLAYED,TOTAL_DAMAGE_DEALT_TO_CHAMPIONS,TOTAL_DAMAGE_DEALT_TO_TURRETS,TOTAL_GOLD_EARNED,VISION_SCORE,WIN,CONFRONTO");

            foreach (var file in files)
            {
                try
                {
                    if (Path.GetExtension(file.FileName).ToLower() != ".rofl")
                    {
                        throw new Exception($"Arquivo inválido: {file.FileName}");
                    }

                    var roflPath = Path.Combine(_environment.ContentRootPath, "Uploads", "Rofl");
                    Directory.CreateDirectory(roflPath);

                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    var roflFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{timestamp}.rofl";
                    var roflFilePath = Path.Combine(roflPath, roflFileName);

                    using (var stream = new FileStream(roflFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var options = new ReplayReaderOptions { LoadPayload = true, Verbose = true };

                    var result = await ReplayReader.ReadReplayAsync(roflFilePath, options);

                    if (result.Type == ReplayType.Unknown || result.Result == null)
                    {
                        throw new Exception($"Falha ao ler o arquivo: {file.FileName}");
                    }

                    if (result.Type == ReplayType.ROFL)
                    {
                        var rofl = (ROFL)result.Result;

                        foreach (var player in rofl.Metadata.PlayerStatistics)
                        {
                            matchList.Add(MatchStats.FromRofl(player, file.FileName, "LSC"));
                            csvData.AppendLine($"{player.Name},{player.Assists}," +
                                               $"{player.ChampionsKilled},{player.MinionsKilled}," +
                                               $"{player.NumDeaths},{player.PentaKills},{player.Skin}," +
                                               $"{player.Team},{player.PlayerRole},{player.TimePlayed}," +
                                               $"{player.TotalDamageDealtToChampions}," +
                                               $"{player.TotalDamageDealtToTurrets}," +
                                               $"{player.GoldSpent}," +
                                               $"{player.VisionScore},{player.Win},{file.FileName}");
                        }
                    }
                    else
                    {
                        var rofl2 = (ROFL2)result.Result;

                        foreach (var player in rofl2.Metadata.PlayerStatistics)
                        {
                            matchList.Add(MatchStats.FromRofl2(player, file.FileName, "LSC"));
                            csvData.AppendLine($"{player.RiotIdGameName}#{player.RiotIdTagLine},{player.Assists}," +
                                               $"{player.ChampionsKilled},{player.MinionsKilled}," +
                                               $"{player.NumDeaths},{player.PentaKills},{player.Skin}," +
                                               $"{player.Team},{player.PlayerRole},{player.TimePlayed}," +
                                               $"{player.TotalDamageDealtToChampions}," +
                                               $"{player.TotalDamageDealtToTurrets}," +
                                               $"{player.GoldSpent}," +
                                               $"{player.VisionScore},{player.Win},{file.FileName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro no arquivo {file.FileName}");
                    ErrorMessage += $"Erro no arquivo {file.FileName}: {ex.Message}\n";
                }
            }

            if (matchList.Any())
            {
                await _context.MatchStats.AddRangeAsync(matchList);
                await _context.SaveChangesAsync();
            }
            

            var csvFileName = $"match_data_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var csvFilePath = Path.Combine(_environment.ContentRootPath, "Uploads", csvFileName);
            await System.IO.File.WriteAllTextAsync(csvFilePath, csvData.ToString());

            var csvBytes = Encoding.UTF8.GetBytes(csvData.ToString());
            return File(csvBytes, "text/csv", csvFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro geral no processamento");
            ErrorMessage = "Ocorreu um erro geral: " + ex.Message;
            return Page();
        }
    }
}