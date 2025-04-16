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
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _context;

    public string? ErrorMessage { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment environment, AppDbContext context)
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

            if (files == null || files.Count == 0)
            {
                ErrorMessage = "Por favor, selecione pelo menos um arquivo ROFL.";
                return Page();
            }

            // Limite máximo de arquivos (opcional)
            //const int maxFiles = 10;
            //if (files.Count > maxFiles)
            //{
            //    ErrorMessage = $"Máximo de {maxFiles} arquivos por vez.";
            //    return Page();
            //}
            
            const long maxFileSize = 100 * 1024 * 1024; // 100MB
            if (files.Count > maxFileSize)
            {
                ErrorMessage = $"Arquivos excede o tamanho máximo de 100MB";
                return Page();
            }

            var jsonFiles = new List<FileContentResult>();
            var convertedFiles = new List<ConvertedFile>();
            var tempFiles = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    // Validação do tipo de arquivo
                    if (Path.GetExtension(file.FileName).ToLower() != ".rofl")
                    {
                        throw new Exception($"Arquivo inválido: {file.FileName}");
                    }

                    // Cria diretórios
                    var roflPath = Path.Combine(_environment.ContentRootPath, "Uploads", "Rofl");
                    var jsonPath = Path.Combine(_environment.ContentRootPath, "Uploads", "Json");
                    Directory.CreateDirectory(roflPath);
                    Directory.CreateDirectory(jsonPath);

                    // Gera nomes únicos
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    var roflFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{timestamp}.rofl";
                    var roflFilePath = Path.Combine(roflPath, roflFileName);

                    // Salva o arquivo ROFL
                    using (var stream = new FileStream(roflFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    tempFiles.Add(roflFilePath);

                    // Processa o arquivo
                    var options = new ReplayReaderOptions { LoadPayload = true, Verbose = true };
                    var result = await ReplayReader.ReadReplayAsync(roflFilePath, options);

                    if (result.Type == ReplayType.Unknown || result.Result == null)
                    {
                        throw new Exception($"Falha ao ler o arquivo: {file.FileName}");
                    }

                    // Serializa para JSON
                    string jsonContent;
                    if (result.Type == ReplayType.ROFL)
                    {
                        var rofl = (ROFL)result.Result;
                        var jsonrofl = new JSONROFL<ROFL>();
                        jsonrofl.data = rofl;
                        jsonrofl.match = file.FileName.Replace(".rofl", "");
                        jsonContent = JsonSerializer.Serialize(jsonrofl, new JsonSerializerOptions { WriteIndented = true });
                    }
                    else
                    {
                        var rofl2 = (ROFL2)result.Result;
                        var jsonrofl = new JSONROFL<ROFL2>();
                        jsonrofl.data = rofl2;
                        jsonrofl.match = file.FileName.Replace(".rofl", "");
                        jsonContent = JsonSerializer.Serialize(jsonrofl, new JsonSerializerOptions { WriteIndented = true });
                    }
                    

                    // Salva JSON
                    var jsonFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{timestamp}.json";
                    var jsonFilePath = Path.Combine(jsonPath, jsonFileName);
                    await System.IO.File.WriteAllTextAsync(jsonFilePath, jsonContent);
                    tempFiles.Add(jsonFilePath);

                    // Prepara para download
                    jsonFiles.Add(File(
                        System.Text.Encoding.UTF8.GetBytes(jsonContent),
                        "application/json",
                        jsonFileName
                    ));

                    // Registra no banco de dados
                    convertedFiles.Add(new ConvertedFile
                    {
                        OriginalFileName = file.FileName,
                        JsonFileName = jsonFileName,
                        JsonContent = jsonContent,
                        FileSize = file.Length,
                        UserEmail = User.Identity?.Name,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro no arquivo {file.FileName}");
                    // Adicione à mensagem de erro sem interromper o processamento
                    ErrorMessage += $"Erro no arquivo {file.FileName}: {ex.Message}\n";
                }
            }
            // Salva todos os registros no banco de dados
            if (convertedFiles.Any())
            {
                await _context.ConvertedFiles.AddRangeAsync(convertedFiles);
                await _context.SaveChangesAsync();
            }

            // Se houve erros e nenhum arquivo foi processado com sucesso
            if (!string.IsNullOrEmpty(ErrorMessage) && !jsonFiles.Any())
            {
                return Page();
            }

            // Se múltiplos arquivos, retorna ZIP
            if (jsonFiles.Count > 1)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var jsonFile in jsonFiles)
                        {
                            var entry = archive.CreateEntry(jsonFile.FileDownloadName);
                            using (var entryStream = entry.Open())
                            using (var streamWriter = new StreamWriter(entryStream))
                            {
                                await streamWriter.WriteAsync(
                                    Encoding.UTF8.GetString(jsonFile.FileContents));
                            }
                        }
                    }

                    // Limpa arquivos temporários
                    foreach (var tempFile in tempFiles)
                    {
                        System.IO.File.Delete(tempFile);
                    }

                    return File(memoryStream.ToArray(), "application/zip", "replays.zip");
                }
            }

            // Caso único arquivo
            return jsonFiles.First();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro geral no processamento");
            ErrorMessage = "Ocorreu um erro geral: " + ex.Message;
            return Page();
        }
    }
}