using Microsoft.Extensions.AI;
using OpenAI;
using AISuggestionsForTB;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        //SUPPORTED ARGUMENTS:
        //-i [inputFilePath] = specifies the input prompt
        //-o [outputFilePath] = specifies the output path OPTIONAL. DEFAULT = Same as the input path.
        //-u [usersFilePath] = specifies the users prompt for better suggestions generation. OPTIONAL DEFAULT = null
        //-c [culture] = specifies the culture. OPTIONAL DEFAULT = pl-PL

        // Handle commandline arguments
        string? inputFilePath = null;
        string? outputFileName = null;
        string? usersFilePath = null;
        string? culture = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-i") { inputFilePath = args[i + 1]; }
            else if (args[i] == "-o") { outputFileName = args[i + 1]; }
            else if (args[i] == "-u") { usersFilePath = args[i + 1]; }
            else if (args[i] == "-c") { culture = args[i + 1]; }
        }

        if (string.IsNullOrWhiteSpace(inputFilePath))
        {
            Console.Write("Enter the input file path: ");
            inputFilePath = Console.ReadLine();
        }
        if (string.IsNullOrWhiteSpace(inputFilePath) || !File.Exists(inputFilePath))
        {
            Console.WriteLine("Input file does not exist.");
            return;
        }

        if (string.IsNullOrWhiteSpace(outputFileName))
            outputFileName = Path.Combine(Path.GetDirectoryName(inputFilePath), Path.GetFileNameWithoutExtension(inputFilePath) + ".xlsx");

        if (string.IsNullOrWhiteSpace(culture))
            culture = "pl-PL";

        //Configuration
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfiguration config = builder.Build();

        if(config["AIClient:ApiKey"] == null || config["AIClient:AIModel"] == null)
        {
            Console.WriteLine("Missing AIClient:ApiKey or AIClient:AIModel in appsettings.json!");
            return;
        }

        //Generate AI Response
        var AISuggestionsClient = new AIClient(new OpenAIClient(config["AIClient:ApiKey"]).AsChatClient(config["AIClient:AIModel"]), config);
        AISuggestionsClient.Culture = culture;
        string? AiResponse = await AISuggestionsClient.GenerateResponse(inputFilePath, usersFilePath);

        if (AiResponse == null)
        {
            Console.WriteLine("Got an empty response from AI Client!");
            return;
        }

        //Generate Excel Based on response
        AISuggestionsClient.CreateExcelFile(outputFileName, AiResponse);

        return;
    }

    
}
