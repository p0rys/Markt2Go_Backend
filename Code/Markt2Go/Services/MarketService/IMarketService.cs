using System.Collections.Generic;
using System.Threading.Tasks;
using Markt2Go.DTOs.Market;

namespace Markt2Go.Services.MarketService
{
    public interface IMarketService
    {
        Task<ServiceResponse<List<GetMarketDTO>>> GetAllMarkets();
        Task<ServiceResponse<GetMarketDTO>> GetMarketById(long id);
        Task<ServiceResponse<List<GetSellerMarketDTO>>> GetMarketsBySellerId(long sellerId);
        Task<ServiceResponse<GetSellerMarketDTO>> GetMarketByIdAndSellerId(long marketId, long sellerId);

        Task<ServiceResponse<GetMarketDTO>> AddMarket(AddMarketDTO newMarket);
        Task<ServiceResponse<GetMarketDTO>> UpdateMarket(UpdateMarketDTO updatedMarket);
        Task<ServiceResponse<GetMarketDTO>> DeleteMarket(long id);
    }
}