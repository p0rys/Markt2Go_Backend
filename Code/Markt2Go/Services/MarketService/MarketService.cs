using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Markt2Go.Data;
using Microsoft.EntityFrameworkCore;
using Markt2Go.Model;
using Markt2Go.DTOs.Market;

namespace Markt2Go.Services.MarketService
{
    public class MarketService : IMarketService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public MarketService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;   
            _context = context;
        }



        public async Task<ServiceResponse<List<GetMarketDTO>>> GetAllMarkets()
        {            
            ServiceResponse<List<GetMarketDTO>> serviceResponse = new ServiceResponse<List<GetMarketDTO>>();
            List<Market> dbMarkets = await _context.Markets.ToListAsync();
            serviceResponse.Data = dbMarkets.Select(x => _mapper.Map<GetMarketDTO>(x)).ToList();
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetMarketDTO>> GetMarketById(long id)
        {
            ServiceResponse<GetMarketDTO> serviceResponse = new ServiceResponse<GetMarketDTO>();
            Market dbMarket = await _context.Markets.FindAsync(id);
            serviceResponse.Data = _mapper.Map<GetMarketDTO>(dbMarket);
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetSellerMarketDTO>>> GetMarketsBySellerId(long sellerId)
        {
            ServiceResponse<List<GetSellerMarketDTO>> serviceResponse = new ServiceResponse<List<GetSellerMarketDTO>>();
            List<MarketSeller> dbSellerMarket = await _context.MarketSellers.Include(x => x.Market).Include(x => x.Seller).Where(x => x.SellerId == sellerId).ToListAsync();
            serviceResponse.Data = dbSellerMarket.Select(x => _mapper.Map<GetSellerMarketDTO>(x)).ToList();
            return serviceResponse;
        }        
        public async Task<ServiceResponse<GetSellerMarketDTO>> GetMarketByIdAndSellerId(long marketId, long sellerId)
        {
            ServiceResponse<GetSellerMarketDTO> serviceResponse = new ServiceResponse<GetSellerMarketDTO>();
            MarketSeller dbSellerMarket = await _context.MarketSellers.Include(x => x.Market).Include(x => x.Seller).FirstOrDefaultAsync(x => x.SellerId == sellerId && x.MarketId == marketId);
            if (dbSellerMarket != null)
            {
                serviceResponse.Data = _mapper.Map<GetSellerMarketDTO>(dbSellerMarket);
            }   
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Could not found market with id '{marketId}' on seller with id '{sellerId}'";
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetMarketDTO>> AddMarket(AddMarketDTO newMarket)
        {
            ServiceResponse<GetMarketDTO> serviceResponse = new ServiceResponse<GetMarketDTO>();
            var market = _mapper.Map<Market>(newMarket);

            await _context.Markets.AddAsync(market);
            await _context.SaveChangesAsync();

            serviceResponse.Data = _mapper.Map<GetMarketDTO>(market);
            return serviceResponse;
        }
        
        public async Task<ServiceResponse<GetMarketDTO>> UpdateMarket(UpdateMarketDTO updatedMarket)
        {
            ServiceResponse<GetMarketDTO> serviceResponse = new ServiceResponse<GetMarketDTO>();
            try 
            {
                var market = await _context.Markets.FindAsync(updatedMarket.Id);
                if (market != null)
                {
                    market.Name = updatedMarket.Name;
                    market.DayOfWeek = updatedMarket.DayOfWeek;
                    market.StartTime = updatedMarket.StartTime;
                    market.EndTime = updatedMarket.EndTime;
                    market.Location = updatedMarket.Location;
                    market.Visible = updatedMarket.Visible;

                    _context.Markets.Update(market);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = _mapper.Map<GetMarketDTO>(market);
                }   
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found market with id '{updatedMarket.Id}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        
        public async Task<ServiceResponse<GetMarketDTO>> DeleteMarket(long id)
        {
            ServiceResponse<GetMarketDTO> serviceResponse = new ServiceResponse<GetMarketDTO>();
            try 
            {
                var market = await _context.Markets.FindAsync(id);
                if (market != null)
                {
                    _context.Markets.Remove(market);
                    await _context.SaveChangesAsync();                    
                    serviceResponse.Data = _mapper.Map<GetMarketDTO>(market);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found market with id '{id}'";
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