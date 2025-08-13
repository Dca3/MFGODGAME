namespace MafiaMMORPG.Domain.ValueObjects;

public class QuestProgress
{
    public int StepIndex { get; set; }           // Görev adımı (0..N)
    public string? StepCode { get; set; }        // İsteğe bağlı sembolik kod
    public bool Completed { get; set; }          // Bu adım tamam mı
    public DateTime UpdatedAt { get; set; }      // Son güncelleme
    public string? Notes { get; set; }           // Opsiyonel açıklama
}
