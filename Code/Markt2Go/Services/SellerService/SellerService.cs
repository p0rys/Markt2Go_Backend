using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Markt2Go.Data;
using Markt2Go.DTOs.Seller;
using Markt2Go.Model;
using Microsoft.EntityFrameworkCore;

namespace Markt2Go.Services.SellerService
{
    public class SellerService : ISellerService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public SellerService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }


        public async Task<ServiceResponse<List<GetSellerDTO>>> GetAllSellers()
        {
            ServiceResponse<List<GetSellerDTO>> serviceResponse = new ServiceResponse<List<GetSellerDTO>>();
            List<Seller> dbSellers = await _context.Sellers.ToListAsync();
            serviceResponse.Data = dbSellers.Select(x => _mapper.Map<GetSellerDTO>(x)).ToList();
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetSellerDTO>> GetSellerById(long id)
        {
            ServiceResponse<GetSellerDTO> serviceResponse = new ServiceResponse<GetSellerDTO>();
            Seller dbSeller = await _context.Sellers.FindAsync(id);
            serviceResponse.Data = _mapper.Map<GetSellerDTO>(dbSeller);
            return serviceResponse;
        }
        public async Task<ServiceResponse<List<GetMarketSellerDTO>>> GetSellersByMarketId(long marketId)
        {
            ServiceResponse<List<GetMarketSellerDTO>> serviceResponse = new ServiceResponse<List<GetMarketSellerDTO>>();
            List<MarketSeller> dbMarketSeller = await _context.MarketSellers.Include(x => x.Seller).Include(x => x.Market).Where(x => x.MarketId == marketId).ToListAsync();
            serviceResponse.Data = dbMarketSeller.Select(x =>_mapper.Map<GetMarketSellerDTO>(x)).ToList();
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetMarketSellerDTO>> GetSellerByIdAndMarketId(long sellerId, long marketId)
        {
            ServiceResponse<GetMarketSellerDTO> serviceResponse = new ServiceResponse<GetMarketSellerDTO>();
            MarketSeller dbMarketSeller = await _context.MarketSellers.Include(x => x.Seller).Include(x => x.Market).FirstOrDefaultAsync(x => x.MarketId == marketId && x.SellerId == sellerId);
            if (dbMarketSeller != null)
            {            
                serviceResponse.Data = _mapper.Map<GetMarketSellerDTO>(dbMarketSeller);
            }
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Could not found seller with id '{sellerId}' on market with id '{marketId}'";
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetSellerDTO>> AddSeller(string userId, AddSellerDTO newSeller)
        {
            ServiceResponse<GetSellerDTO> serviceResponse = new ServiceResponse<GetSellerDTO>();
            try
            {
                var seller = _mapper.Map<Seller>(newSeller);

                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    // add user to seller to update seller id on user entity        
                    seller.Users.Add(user);
                    await _context.Sellers.AddAsync(seller);
                    await _context.SaveChangesAsync();

                    serviceResponse.Data = _mapper.Map<GetSellerDTO>(seller);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not add seller because user with id '{userId}' was not found.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetSellerDTO>> UpdateSeller(UpdateSellerDTO updatedSeller)
        {
            ServiceResponse<GetSellerDTO> serviceResponse = new ServiceResponse<GetSellerDTO>();
            try
            {
                var seller = await _context.Sellers.FindAsync(updatedSeller.Id);
                if (seller != null)
                {
                    seller.Name = updatedSeller.Name;
                    seller.Category = updatedSeller.Category;

                    _context.Sellers.Update(seller);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = _mapper.Map<GetSellerDTO>(seller);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found seller with id '{updatedSeller.Id}'.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetSellerDTO>> DeleteSeller(long id)
        {
            ServiceResponse<GetSellerDTO> serviceResponse = new ServiceResponse<GetSellerDTO>();
            try
            {
                var seller = await _context.Sellers.FindAsync(id);
                if (seller != null)
                {
                    _context.Sellers.Remove(seller);
                    await _context.SaveChangesAsync();

                    serviceResponse.Data = _mapper.Map<GetSellerDTO>(seller);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found seller with id '{id}'.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetMarketSellerDTO>> AddMarketToSeller(AddMarketSellerDTO newMarketSeller)
        {
            ServiceResponse<GetMarketSellerDTO> serviceResponse = new ServiceResponse<GetMarketSellerDTO>();
            try
            {
                if (!await _context.MarketSellers.AnyAsync(x => x.MarketId == newMarketSeller.MarketId && x.SellerId == newMarketSeller.SellerId))
                {
                    var seller = await _context.Sellers.FindAsync(newMarketSeller.SellerId);
                    var market = await _context.Markets.FindAsync(newMarketSeller.MarketId);

                    if (seller == null)
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = $"Could not found seller with id '{newMarketSeller.SellerId}'";
                    }
                    else if (market == null)
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = $"Could not found market with id '{newMarketSeller.MarketId}'";
                    }
                    else
                    {
                        MarketSeller marketSeller = _mapper.Map<MarketSeller>(newMarketSeller);

                        await _context.MarketSellers.AddAsync(marketSeller);
                        await _context.SaveChangesAsync();

                        // reload data from database because market and seller objects are not included otherwise
                        var dbMarketSeller = await _context.MarketSellers.Include(x => x.Market).Include(x => x.Seller).SingleAsync(x => x.Id == marketSeller.Id);
                        serviceResponse.Data = _mapper.Map<GetMarketSellerDTO>(dbMarketSeller);
                    }
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Market with id '{newMarketSeller.MarketId} already exists for seller with id '{newMarketSeller.SellerId}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetMarketSellerDTO>> UpdateMarketSeller(UpdateMarketSellerDTO updatedMarketSeller)
        {
            ServiceResponse<GetMarketSellerDTO> serviceResponse = new ServiceResponse<GetMarketSellerDTO>();
            try
            {
                var marketSeller = await _context.MarketSellers.Include(x => x.Market).Include(x => x.Seller).FirstOrDefaultAsync(x => x.SellerId == updatedMarketSeller.SellerId && x.MarketId == updatedMarketSeller.MarketId);
                if (marketSeller != null)
                {
                    marketSeller.Description = updatedMarketSeller.Description;
                    marketSeller.Portfolio = updatedMarketSeller.Portfolio;
                    marketSeller.LastReservationOffset = updatedMarketSeller.LastReservationOffset;
                    marketSeller.DebitCardAccepted = updatedMarketSeller.DebitCardAccepted;
                    marketSeller.CreditCardAccepted = updatedMarketSeller.CreditCardAccepted;
                    marketSeller.Visible = updatedMarketSeller.Visible;

                    // TODO: figure out why this is not working here
                    //_context.MarketSellers.Update(dbMarketSeller);
                    await _context.SaveChangesAsync();

                    serviceResponse.Data = _mapper.Map<GetMarketSellerDTO>(marketSeller);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found market with id '{updatedMarketSeller.MarketId}' on seller with id '{updatedMarketSeller.SellerId}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetMarketSellerDTO>> DeleteMarketFromSeller(long id, long marketId)
        {
            ServiceResponse<GetMarketSellerDTO> serviceResponse = new ServiceResponse<GetMarketSellerDTO>();
            try
            {
                var marketSeller = await _context.MarketSellers.Include(x => x.Market).Include(x => x.Seller).SingleOrDefaultAsync(x => x.SellerId == id && x.MarketId == marketId);
                if (marketSeller != null)
                {
                    _context.MarketSellers.Remove(marketSeller);
                    await _context.SaveChangesAsync();

                    serviceResponse.Data = _mapper.Map<GetMarketSellerDTO>(marketSeller);;
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found market with id '{marketId}' on seller with id '{id}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
    }
}