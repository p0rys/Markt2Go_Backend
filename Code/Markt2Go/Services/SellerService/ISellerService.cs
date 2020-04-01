using System.Collections.Generic;
using System.Threading.Tasks;
using Markt2Go.DTOs.Seller;
using Markt2Go.Services;

namespace Markt2Go.Services.SellerService
{
    public interface ISellerService
    {
        Task<ServiceResponse<List<GetSellerDTO>>> GetAllSellers();
        Task<ServiceResponse<GetSellerDTO>> GetSellerById(long id);
        Task<ServiceResponse<List<GetMarketSellerDTO>>> GetSellersByMarketId(long marketId);
        Task<ServiceResponse<GetMarketSellerDTO>> GetSellerByIdAndMarketId(long sellerId, long marketId);

        Task<ServiceResponse<GetSellerDTO>> AddSeller(string userId, AddSellerDTO newSeller);
        Task<ServiceResponse<GetSellerDTO>> UpdateSeller(UpdateSellerDTO updatedSeller);
        Task<ServiceResponse<GetSellerDTO>> DeleteSeller(long id);

        Task<ServiceResponse<GetMarketSellerDTO>> AddMarketToSeller(AddMarketSellerDTO newMarketSeller);
        Task<ServiceResponse<GetMarketSellerDTO>> UpdateMarketSeller(UpdateMarketSellerDTO updatedMarketSeller);
        Task<ServiceResponse<GetMarketSellerDTO>> DeleteMarketFromSeller(long id, long marketId);
    }
}