# AITaskAnalyzer

AITaskAnalyzer is a utility designed to enhance suggestions and provide AI-driven insights based on user prompts. This documentation will guide you through the configuration and usage of the tool.

---

## Configuration

To configure AITaskAnalyzer, locate and modify the `appsettings.json` file. Below is a sample configuration:

```json
{
  "AIClient": {
    "ApiKey": "[YOUR-API-KEY]",
    "AIModel": "[YOUR-OPENAI-MODEL]",
    "SystemPromptPath": "[YOUR-PROMPT-FOR-THE-SYSTEM-FILE-PATH-TXT]"
  }
}
```

- **`ApiKey`**: Your API key for accessing the AI service.
- **`AIModel`**: The model used for generating suggestions (e.g., `gpt-4`).
- **`SystemPromptPath`**: The file path to the system prompt text file.

---

## Usage

The tool supports the following command-line arguments:

| Argument         | Description                                                                                              | Required | Default                |
|-------------------|----------------------------------------------------------------------------------------------------------|----------|------------------------|
| `-i [inputFilePath]` | Specifies the input file path. This file should contain exported tasks                                | **Yes**  | -                     |
| `-o [outputFilePath]` | Specifies the output file path. If not provided, defaults to the same path as the input file.         | No       | Same as input path     |
| `-u [usersFilePath]` | Specifies a file containing users information for improved suggestion generation. Optional.           | No       | `null`                |
| `-c [culture]`      | Specifies the culture for localization (e.g., `en-US`, `pl-PL`). Optional.                              | No       | `pl-PL`               |

### Example Commands

1. **Basic Usage**:  
   Generate suggestions using an input file and save the results to the same location:
   ```bash
   AITaskAnalyzer -i input.txt
   ```

2. **Specify Output File**:  
   Generate suggestions and save the results to a custom output file:
   ```bash
   AITaskAnalyzer -i input.txt -o output.xlsx
   ```

3. **Use User Prompts**:  
   Provide additional users information to enhance suggestion quality. If provided tasks will be assigned to users specified in this file:
   ```bash
   AITaskAnalyzer -i input.txt -u users.txt
   ```

4. **Set Culture**:  
   Specify a culture for deserializing numbers:
   ```bash
   AITaskAnalyzer -i input.txt -c en-US
   ```