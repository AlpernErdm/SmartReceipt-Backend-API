using Mapster;
using SmartReceipt.Application.DTOs;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Application.Common.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Receipt, ReceiptDto>()
            .Map(dest => dest.Items, src => src.Items);

        config.NewConfig<ReceiptItem, ReceiptItemDto>();

        config.NewConfig<CreateReceiptDto, Receipt>()
            .Map(dest => dest.Items, src => src.Items);

        config.NewConfig<CreateReceiptItemDto, ReceiptItem>();

        config.NewConfig<ScannedReceiptItemDto, ReceiptItem>();

        config.NewConfig<ReceiptScanResultDto, Receipt>()
            .Map(dest => dest.Items, src => src.Items.Adapt<List<ReceiptItem>>());
    }
}
