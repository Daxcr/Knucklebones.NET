using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace CotLMinigames.Knucklebones;

public static class KBPolicy
{
    public const int ObservationSize = 150;
    public static readonly string ModelPath = Path.Combine(AppContext.BaseDirectory, "Content", "Models", "knucklebones-policy.onnx");

    private static readonly Lazy<InferenceSession?> session = new(LoadSession);

    private static InferenceSession? LoadSession()
    {
        try
        {
            return new InferenceSession(ModelPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Knucklebones] Couldn't load the AI model from {ModelPath}, falling back to the simple bot: {ex.Message}");
            return null;
        }
    }

    public static byte? ChooseColumn(KBGameMetadata.Table botTable, KBGameMetadata.Table playerTable, byte dice)
    {
        float[]? logits = Logits(botTable, playerTable, dice);
        if (logits == null)
            return null;

        List<byte>[] columns = [botTable.Left, botTable.Middle, botTable.Right];
        int best = -1;
        for (int c = 0; c < 3; c++)
        {
            if (!columns[c].Contains(0))
                continue;
            if (best == -1 || logits[c] > logits[best])
                best = c;
        }

        return best == -1 ? null : (byte)best;
    }

    public static float[]? Logits(KBGameMetadata.Table botTable, KBGameMetadata.Table playerTable, byte dice)
    {
        try
        {
            InferenceSession? model = session.Value;
            if (model == null)
                return null;

            DenseTensor<float> input = new(Encode(botTable, playerTable, dice), [1, ObservationSize]);
            using var results = model.Run([NamedOnnxValue.CreateFromTensor("obs", input)], ["logits"]);
            return results.First().AsTensor<float>().ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Knucklebones] AI model inference failed, falling back to the simple bot: {ex.Message}");
            return null;
        }
    }

    public static float[] Encode(KBGameMetadata.Table botTable, KBGameMetadata.Table playerTable, byte dice)
    {
        float[] obs = new float[ObservationSize];
        List<byte>[] botColumns = [botTable.Left, botTable.Middle, botTable.Right];
        List<byte>[] playerColumns = [playerTable.Left, playerTable.Middle, playerTable.Right];

        for (int c = 0; c < 3; c++)
        {
            EncodeSide(obs, c * 48, botColumns[c]);
            EncodeSide(obs, c * 48 + 24, playerColumns[c]);
        }

        if (dice >= 1 && dice <= 6)
            obs[144 + dice - 1] = 1;

        return obs;
    }

    private static void EncodeSide(float[] obs, int offset, List<byte> column)
    {
        for (int value = 1; value <= 6; value++)
        {
            int count = Math.Min(column.Count(n => n == value), 3);
            obs[offset + (value - 1) * 4 + count] = 1;
        }
    }
}
