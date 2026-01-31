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
    public int[] BornTraits { get; init; } = [];
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
    private int _ageGroup = 1;
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
        int ageGroup = GetAgeGroup();
        string sex = GetSex();
        return new Customer
        {
            BornTraits = traits,
            _ageGroup = ageGroup,
            _sex = sex,
            _story = await GetStoryAsync(ageGroup, sex).ConfigureAwait(false)
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
                ageGroup = _ageGroup,
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
    private static int GetAgeGroup() => Mathf.Abs((int)GD.Randi()) % 3 + 1;
    private static string GetSex() => GD.Randf() < 0.5f ? "male" : "female";
    private static async Task<string> GetStoryAsync(int ageGroup, string sex)
    {
        const string url = "https://www.test.com/v2";
        const int maxRetries = 3;

        foreach (var attempt in Enumerable.Range(1, maxRetries))
        {
            try
            {
                var payload = new
                {
                    ageGroup,
                    sex,
                    //traits = BornTraits
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
                var respStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return JsonDocument.Parse(respStr).RootElement.GetProperty("story").GetString();
            }
            catch (Exception ex)
            {
                GD.PushError($"GetBackground error attempt {attempt}: {ex.Message}");
                if (attempt == maxRetries) break;
                await Task.Delay(500).ConfigureAwait(false);
            }
        }

        return null;
    }
}