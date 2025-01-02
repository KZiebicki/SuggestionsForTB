using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace AITaskAnalyzer
{
    public class AIClient
    {
        IChatClient Client { get; set; }
        public IConfiguration Configuration { get; }
        public string Culture { get; set; } = "pl-PL";

        public AIClient(IChatClient client, IConfiguration configuration)
        {
            Client = client;
            Configuration = configuration;
        }

        public async Task<string?> GenerateResponse(string inputFilePath, string? usersFilePath)
        {
            string? systemPrompt = null;
            try
            {
                systemPrompt = await File.ReadAllTextAsync(Configuration["AIClient:SystemPromptPath"]);
            }
            catch (Exception)
            {
                throw new Exception("Missing systemPrompt!");
            }
            if (systemPrompt == null)
                throw new Exception("Missing systemPrompt!");

            // System message and conditional user message
            List<ChatMessage> messages = new()
            {
                new ChatMessage
                {
                    Role = ChatRole.System,
                    Text = systemPrompt
                }
            };
            if(!string.IsNullOrWhiteSpace(usersFilePath))
            {
                string usersContent = await File.ReadAllTextAsync(usersFilePath);
                messages.Add(new ChatMessage
                {
                    Role = ChatRole.User,
                    Text = $"[USER LIST]:\n\n{usersContent}"
                });
            }

            string fileContent = await File.ReadAllTextAsync(inputFilePath);
            // Add the main user message with the file content
            messages.Add(new ChatMessage
            {
                Role = ChatRole.User,
                Text = $"[TASK LIST]:\n\n{fileContent}"
            });

            // Send the messages to OpenAI
            var response = await Client.CompleteAsync(messages);

            if (response.Choices.Count == 0)
                return null;

            return response.Choices[0].Text;
        }

        public void CreateExcelFile(string outputFileName, string content)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            var worksheet = package.Workbook.Worksheets.Add("Tasks");

            // Parse the AI response content
            var tasks = ParseTasks(content);
            if (tasks == null)
            {
                throw new InvalidDataException("Invalid content format for Excel generation.");
            }

            // Create header row
            worksheet.Cells[1, 1].Value = "Task Identifier";
            worksheet.Cells[1, 2].Value = "Parent Task";
            worksheet.Cells[1, 3].Value = "Task Title";
            worksheet.Cells[1, 4].Value = "Due Date";
            worksheet.Cells[1, 5].Value = "Complete";
            worksheet.Cells[1, 6].Value = "Assigned To";
            worksheet.Cells[1, 7].Value = "Description";
            worksheet.Cells[1, 8].Value = "Estimated Time";
            worksheet.Cells[1, 9].Value = "Suggestion";

            int row = 2;
            foreach (var task in tasks)
            {
                worksheet.Cells[row, 1].Value = task.Identifier;
                worksheet.Cells[row, 2].Value = task.ParentTask;
                worksheet.Cells[row, 3].Value = task.Title;
                worksheet.Cells[row, 4].Value = task.DueDate;
                worksheet.Cells[row, 5].Value = task.Complete;
                worksheet.Cells[row, 6].Value = task.AssignedTo;
                worksheet.Cells[row, 7].Value = task.Description;
                worksheet.Cells[row, 8].Value = task.EstimatedTime;
                worksheet.Cells[row, 9].Value = task.Suggestion;
                row++;
            }

            // Save to file
            File.WriteAllBytes(outputFileName, package.GetAsByteArray());
        }

        private List<TaskItem>? ParseTasks(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<TaskItem>>(content, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(Culture)
                });
            }
            catch
            {
                return JsonConvert.DeserializeObject<DataItem>(content, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo(Culture)
                })?.Data;
            }
        }

        private class TaskItem
        {
            public string Identifier { get; set; }
            public string ParentTask { get; set; }
            public string Title { get; set; }
            public string DueDate { get; set; }
            public string Complete { get; set; }
            public string AssignedTo { get; set; }
            public string Description { get; set; }
            public decimal EstimatedTime { get; set; }
            public string Suggestion { get; set; }
        }

        private class DataItem
        {
            public List<TaskItem> Data { get; set; }
        }
    }
}
