using FluentValidation;

namespace SmartReceipt.Application.Features.Receipts.Commands.CreateReceipt;

public class CreateReceiptCommandValidator : AbstractValidator<CreateReceiptCommand>
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public CreateReceiptCommandValidator()
    {
        When(x => x.UseAiProcessing, () =>
        {
            RuleFor(x => x.ImageFile)
                .NotNull()
                .WithMessage("AI işleme için görsel dosyası gereklidir.")
                .Must(file => file != null && file.Length > 0)
                .WithMessage("Görsel dosyası boş olamaz.")
                .Must(file => file != null && file.Length <= MaxFileSizeBytes)
                .WithMessage($"Görsel dosyası en fazla {MaxFileSizeBytes / (1024 * 1024)} MB olabilir.")
                .Must(file =>
                {
                    if (file == null) return false;
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    return _allowedExtensions.Contains(extension);
                })
                .WithMessage($"Geçerli görsel formatları: {string.Join(", ", _allowedExtensions)}");
        });

        When(x => !x.UseAiProcessing, () =>
        {
            RuleFor(x => x.ManualData)
                .NotNull()
                .WithMessage("Manuel giriş için fiş verileri gereklidir.");

            When(x => x.ManualData != null, () =>
            {
                RuleFor(x => x.ManualData!.StoreName)
                    .NotEmpty()
                    .WithMessage("Mağaza adı gereklidir.")
                    .MaximumLength(200)
                    .WithMessage("Mağaza adı en fazla 200 karakter olabilir.");

                RuleFor(x => x.ManualData!.TotalAmount)
                    .GreaterThan(0)
                    .WithMessage("Toplam tutar 0'dan büyük olmalıdır.");

                RuleFor(x => x.ManualData!.ReceiptDate)
                    .NotEmpty()
                    .WithMessage("Fiş tarihi gereklidir.")
                    .LessThanOrEqualTo(DateTime.Now.AddDays(1))
                    .WithMessage("Fiş tarihi gelecekte olamaz.");

                RuleFor(x => x.ManualData!.Items)
                    .NotEmpty()
                    .WithMessage("En az bir ürün kalemi gereklidir.");
            });
        });
    }
}
