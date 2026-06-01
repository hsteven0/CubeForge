using CubeForge.Trainers.Shared;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CubeForge.Views
{
    public interface ICaseInfo
    {
        int Number { get; set; }
    }

    public class CaseSelection<TCase> where TCase : ICaseInfo
    {
        public List<TCase> AllCases { get; private set; } = new();
        public HashSet<int> KnownCases { get; } = new();
        public HashSet<int> SelectedTrainerCases { get; } = new();

        public void LoadFromJson(string jsonPath, string trainerName)
        {
            string json;

            try
            {
                json = ResourceLoader.ReadText($"Assets/{Path.GetFileName(jsonPath)}");
            }
            catch
            {
                throw new FileNotFoundException($"Could not find {trainerName} JSON.", jsonPath);
            }

            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement casesElement = 
                document.RootElement.ValueKind == JsonValueKind.Object && 
                document.RootElement.TryGetProperty("cases", out JsonElement rootCases)
                ? rootCases
                : document.RootElement;

            AllCases = JsonSerializer.Deserialize<List<TCase>>(casesElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<TCase>();

            SelectAll();
        }

        public void SelectAll()
        {
            SelectedTrainerCases.Clear();

            foreach (TCase trainerCase in AllCases)
                SelectedTrainerCases.Add(trainerCase.Number);
        }

        public void SelectUnknownOnly()
        {
            SelectedTrainerCases.Clear();

            foreach (TCase trainerCase in AllCases)
            {
                if (!KnownCases.Contains(trainerCase.Number))
                    SelectedTrainerCases.Add(trainerCase.Number);
            }
        }

        public void SelectKnownOnly()
        {
            SelectedTrainerCases.Clear();

            foreach (TCase trainerCase in AllCases)
            {
                if (KnownCases.Contains(trainerCase.Number))
                    SelectedTrainerCases.Add(trainerCase.Number);
            }
        }

        public void ClearSelection()
        {
            SelectedTrainerCases.Clear();
        }
    }
}
