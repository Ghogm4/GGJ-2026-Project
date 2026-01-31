using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SysHttpClient = System.Net.Http.HttpClient;
public partial class Customer : Resource
{
    private struct Message
    {
        public string Role;
        public string Text;
        public Message(string role, string text)
        {
            Role = role;
            Text = text;
        }
    }
    public int Patience { get; set; } = 100;
    public List<int> BornTraits { get; init; } = new();
    public List<int> FinalTraits { get; private set; } = new();
    private static readonly SysHttpClient _httpClient = new();
    private List<Message> _messageHistory = new();
    private int BornTraitsBitMask
    {
        get
        {
            if (field != 0) return field;

            int bitmask = 0;
            foreach (var trait in BornTraits)
            {
                bitmask |= 1 << trait;
            }
            return field = bitmask;
        }
    } = 0;
    private bool IsSatisfied => _triggeredTraitsBitMask == BornTraitsBitMask;
    private string _age = "";
    private string _sex = "";
    private string _story = "";
    private int _triggeredTraitsBitMask = 0;
    private int _triggeredTraits = 0;
    private Texture2D _portrait = null;
    private Customer() { }
    public static async Task<Customer> New()
    {
        int traitIndex = Mathf.Abs((int)GD.Randi() % Mask.PersonaTraits.Count);
        int[] traits = Mask.PersonaTraits[traitIndex];
        string age = GetAge();
        string sex = GetSex();
        return new Customer
        {
            BornTraits = [.. traits],
            FinalTraits = [.. traits],
            _age = age,
            _sex = sex,
            _story = await GetStoryAsync(age, sex).ConfigureAwait(false)
        };
    }
    public void WearMask(Mask mask)
    {
        if (mask.Mode == MaskMode.Union)
        {
            FinalTraits = BornTraits.Union(mask.Traits).ToList();
        }
        else if (mask.Mode == MaskMode.Intersection)
        {
            FinalTraits = BornTraits.Intersect(mask.Traits).ToList();
        }
    }

    public async Task SendAndGetResponseAsync(string text)
    {
        _messageHistory.Add(new Message("user", text));
        JsonDocument doc = await SendPostAsync().ConfigureAwait(false);
        if (doc == null)
        {
            _messageHistory.RemoveAt(_messageHistory.Count - 1);
            return;
        }

        string respSentence = doc.RootElement
            .GetProperty("response")
            .GetString();
        _messageHistory.Add(new Message("assistant", respSentence));
    }
    private async Task<JsonDocument> SendPostAsync()
    {
        try
        {
            var url = "https://www.test.com/v1";

            var payload = new
            {
                messageHistory = _messageHistory.Select(m => m.Text).ToArray(),
                finalTraits = FinalTraits.ToArray(),
                story = _story,
                age = _age,
                sex = _sex,
                patience = Patience
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
            var respStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonDocument.Parse(respStr);
        }
        catch (Exception ex)
        {
            GD.PushError("SendPostAsync error: " + ex.Message);
            return null;
        }
    }
    private static string GetAge()
    {
        return "";
    }
    private static string GetSex() => GD.Randf() < 0.5f ? "male" : "female";
    private static string ExtractSingleStringFromJson(JsonDocument doc)
    {
        if (doc.RootElement.ValueKind == JsonValueKind.String)
        {
            return doc.RootElement.GetString();
        }

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.String)
                return prop.Value.GetString();
        }

        GD.PushError("ExtractSingleStringFromJson: No string found in JSON.");
        return null;
    }
    private static async Task<string> GetStoryAsync(string age, string sex)
    {
        var url = "https://www.test.com/v2";
        const int maxRetries = 3;

        foreach (var attempt in Enumerable.Range(1, maxRetries))
        {
            try
            {
                var payload = new
                {
                    age,
                    sex
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
                var respStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(respStr);
                return ExtractSingleStringFromJson(doc);
            }
            catch (Exception ex)
            {
                GD.PushError($"GetStory error attempt {attempt}: {ex.Message}");
                if (attempt == maxRetries) break;
                await Task.Delay(500).ConfigureAwait(false);
            }
        }

        return null;
    }
}