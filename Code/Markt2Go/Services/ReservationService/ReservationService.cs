using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Markt2Go.Data;
using Markt2Go.DTOs.Reservation;
using Markt2Go.DTOs.Timeslot;
using Markt2Go.Model;
using Markt2Go.Services.MailService;
using Markt2Go.Shared.Enums;
using Markt2Go.Shared.Helper;
using Markt2Go.Shared.Extensions;

namespace Markt2Go.Services.ReservationService
{
    public class ReservationService : IReservationService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IMailService _mailService;

        public ReservationService(IMapper mapper, DataContext context, IMailService mailService)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (mailService == null)
                throw new ArgumentNullException(nameof(mailService));

            _mapper = mapper;
            _context = context;
            _mailService = mailService;
        }


        public async Task<ServiceResponse<List<GetReservationDTO>>> GetAllReservations()
        {
            ServiceResponse<List<GetReservationDTO>> serviceResponse = new ServiceResponse<List<GetReservationDTO>>();
            serviceResponse.Data = await GetReservations();
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetReservationDTO>> GetReservationById(long id)
        {
            ServiceResponse<GetReservationDTO> serviceResponse = new ServiceResponse<GetReservationDTO>();
            var reservation = await _context.Reservations                
                .Include(x => x.MarketSeller)
                .Include(x => x.MarketSeller.Market)
                .Include(x => x.MarketSeller.Seller)
                .Include(x => x.User)
                .Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (reservation != null)
            {
                serviceResponse.Data = _mapper.Map<GetReservationDTO>(reservation);
            }
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Could not found reservation with id '{id}'";
            }
            return serviceResponse;
        }
        public async Task<ServiceResponse<List<GetReservationDTO>>> GetReservationsByUser(string userId, long? marketId, DateTime createdSince, DateTime pickupSince, DateTime pickup, StatusEnum? status)
        {
            ServiceResponse<List<GetReservationDTO>> serviceResponse = new ServiceResponse<List<GetReservationDTO>>();
            serviceResponse.Data = await GetReservations(userId: userId, marketId: marketId, createdSince: createdSince, pickupSince: pickupSince, pickup: pickup, status: status);
            return serviceResponse;
        }
        public async Task<ServiceResponse<List<GetReservationDTO>>> GetReservationsBySeller(long sellerId, long? marketId, DateTime createdSince, DateTime pickupSince, DateTime pickup, StatusEnum? status)
        {
            ServiceResponse<List<GetReservationDTO>> serviceResponse = new ServiceResponse<List<GetReservationDTO>>();
            serviceResponse.Data = await GetReservations(sellerId: sellerId, marketId: marketId, createdSince: createdSince, pickupSince: pickupSince, pickup: pickup, status: status);
            return serviceResponse;
        }
        /*
        public async Task<ServiceResponse<List<GetItemDTO>>> GetItemsByReservationId(long reservationId)
        {
            ServiceResponse<List<GetItemDTO>> serviceResponse = new ServiceResponse<List<GetItemDTO>>();
            List<Item> dbItems = await _context.Items.Where(x => x.ReservationId == reservationId).ToListAsync();
            serviceResponse.Data = dbItems.Select(x => _mapper.Map<GetItemDTO>(x)).ToList();
            return serviceResponse;
        }
        */
        public async Task<ServiceResponse<List<GetTimeslotDTO>>> GetTimeslots(long marketId, long sellerId)
        {
            ServiceResponse<List<GetTimeslotDTO>> serviceResponse = new ServiceResponse<List<GetTimeslotDTO>>();
            try
            {
                var marketSeller = await _context.MarketSellers.Include(x => x.Market).FirstOrDefaultAsync(x => x.MarketId == marketId && x.SellerId == sellerId);
                if (marketSeller != null && marketSeller.Market != null)
                {
                    var next = DateTime.Now.GetNextDate((DayOfWeek)marketSeller.Market.DayOfWeek);

                    var reservations = await _context.Reservations.Where(x => x.MarketSellerId == marketSeller.Id && x.Pickup.Date == next.Date).ToListAsync();
                    
                    var start = next.SetTime(marketSeller.Market.StartTime, DateTimeKind.Local);
                    var end = next.SetTime(marketSeller.Market.EndTime, DateTimeKind.Local);
                    var timeslots = DateTimeHelper.GetTimeslots(start, end, new TimeSpan(0, 15, 0));

                    serviceResponse.Data = timeslots.Select(x =>
                        new GetTimeslotDTO()
                        {
                            MarketId = marketId,
                            SellerId = sellerId,
                            Timestamp = x,
                            TimestampUTC = x.ToUniversalTime(),
                            ReservationCount = reservations.Count(y => y.Pickup.ToUniversalTime() == x.ToUniversalTime())
                        }).ToList();
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found seller with id '{sellerId}' on market with id '{marketId}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetReservationDTO>> AddReservation(AddReservationDTO newReservation)
        {
            ServiceResponse<GetReservationDTO> serviceResponse = new ServiceResponse<GetReservationDTO>();
            try
            {
                var marketSeller = await _context.MarketSellers.Include(x => x.Market).Include(x => x.Seller).FirstOrDefaultAsync(x => x.SellerId == newReservation.SellerId && x.MarketId == newReservation.MarketId);
                if (marketSeller != null)
                {
                    var next = DateTime.Now.GetNextDate((DayOfWeek)marketSeller.Market.DayOfWeek);
                    var lastReservation = next.AddHours(marketSeller.LastReservationOffset ?? 0);
                    if (DateTime.Now <= lastReservation)
                    {
                        var nextStart = next.SetTime(marketSeller.Market.StartTime, DateTimeKind.Local);
                        var nextEnd = next.SetTime(marketSeller.Market.EndTime, DateTimeKind.Local);
                        if (newReservation.Pickup >= nextStart.ToUniversalTime() && newReservation.Pickup <= nextEnd.ToUniversalTime())
                        {
                            Reservation reservation = _mapper.Map<Reservation>(newReservation);
                            reservation.MarketSellerId = marketSeller.Id;
                            reservation.CreatedAt = DateTime.Now.ToUniversalTime();

                            // add and save reservation
                            await _context.Reservations.AddAsync(reservation);
                            await _context.SaveChangesAsync();

                            // load references to return a complete reservation object
                            _context.Entry(reservation).Reference(x => x.MarketSeller).Load();
                            _context.Entry(reservation.MarketSeller).Reference(x => x.Market).Load();
                            _context.Entry(reservation.MarketSeller).Reference(x => x.Seller).Load();

                            // collect information for mail (no need for a null check -> db constraint would fail if reservation has invalid user id)
                            var user = await _context.Users.FindAsync(newReservation.UserId);
                            // send user mail
                            await _mailService.SendReservationConfirmation(user.Mail, CreatePlaceholders(reservation, marketSeller.Market, marketSeller.Seller, user));
                            // send seller mail
                            // TODO: add seller mail 

                            serviceResponse.Data = _mapper.Map<GetReservationDTO>(reservation);
                        }
                        else
                        {
                            serviceResponse.Success = false;
                            serviceResponse.Message = $"Pickup invalid! It is only possible to create reservations for the next market day (Start: {nextStart.ToString()}, End: {nextEnd.ToString()}).";
                        }
                    }
                    else
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = $"Reservation time limit exceeded for seller (id: '{newReservation.SellerId}') at market (id: '{newReservation.MarketId}'. The seller allows reservations only until {marketSeller.LastReservationOffset} minutes before market starts.";
                    }
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found combination of marketId '{newReservation.MarketId}' and sellerId '{newReservation.SellerId}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetReservationDTO>> UpdateReservation(UpdateReservationDTO updatedReservation)
        {
            ServiceResponse<GetReservationDTO> serviceResponse = new ServiceResponse<GetReservationDTO>();
            try
            {
                Reservation reservation = await _context.Reservations.FindAsync(updatedReservation.Id);
                if (reservation != null)
                {
                    reservation.Pickup = updatedReservation.Pickup;
                    reservation.Price = updatedReservation.Price;
                    reservation.UserComment = updatedReservation.UserComment;
                    // TODO: add items here (how to handle update items?)

                    _context.Reservations.Update(reservation);
                    await _context.SaveChangesAsync();

                    // TODO: add reservation updated user mail
                    // TODO: add reservation updated seller mail

                    serviceResponse.Data = _mapper.Map<GetReservationDTO>(reservation);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found reservation with id '{updatedReservation.Id}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        /*
        public async Task<ServiceResponse<GetItemDTO>> UpdateItem(long reservationId, UpdateItemDTO updatedItem)
        {
            ServiceResponse<GetItemDTO> serviceResponse = new ServiceResponse<GetItemDTO>();
            try
            {
                Item item = await _context.Items.FirstOrDefaultAsync(x => x.Id == updatedItem.Id);
                if (item != null)
                {
                    if (item.ReservationId == reservationId)
                    {
                        item.Name = updatedItem.Name;
                        item.Amount = updatedItem.Amount;
                        item.Unit = updatedItem.Unit;

                        _context.Items.Update(item);
                        await _context.SaveChangesAsync();
                        serviceResponse.Data = _mapper.Map<GetItemDTO>(item);
                    }
                    else
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = $"Item with id '{updatedItem.Id}' is not part of reservation with id '{reservationId}'";
                    }
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found item with id '{updatedItem.Id}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        */

        public async Task<ServiceResponse<GetReservationDTO>> SetReservationStatus(long id, SetReservationStatusDTO reservationStatus)
        {
            ServiceResponse<GetReservationDTO> serviceResponse = new ServiceResponse<GetReservationDTO>();
            try
            {
                Reservation reservation = await _context.Reservations
                    .Include(x => x.User)
                    .Include(x => x.Items)
                    .Include(x => x.MarketSeller.Market)
                    .Include(x => x.MarketSeller.Seller)
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (reservation != null)
                {
                    // before changing data in db -> check if accepted was changed (or just packed)
                    var acceptedChanged = (reservation.Accepted != reservationStatus.Accepted);

                    reservation.Accepted = reservationStatus.Accepted;
                    reservation.Price = reservationStatus.Price;
                    reservation.Packed = reservationStatus.Packed;
                    reservation.SellerComment = reservationStatus.SellerComment;

                    _context.Reservations.Update(reservation);
                    await _context.SaveChangesAsync();

                    var placeholders = CreatePlaceholders(reservation, reservation.MarketSeller.Market, reservation.MarketSeller.Seller, reservation.User);
                    // send user mail(s)
                    // TODO: refactor to only have one SendMail Method
                    if (acceptedChanged)
                    {
                        if (reservation.Accepted.Value)
                            await _mailService.SendReservationAccepted(reservation.User.Mail, placeholders);
                        else
                            await _mailService.SendReservationDeclined(reservation.User.Mail, placeholders);
                    }
                    if (reservation.Packed)
                        await _mailService.SendReservationPacked(reservation.User.Mail, placeholders);

                    serviceResponse.Data = _mapper.Map<GetReservationDTO>(reservation);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found reservation with id '{id}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetReservationDTO>> DeleteReservation(long id)
        {
            ServiceResponse<GetReservationDTO> serviceResponse = new ServiceResponse<GetReservationDTO>();
            try
            {
                Reservation reservation = await _context.Reservations.FindAsync(id);
                if (reservation != null)
                {
                    _context.Reservations.Remove(reservation);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = _mapper.Map<GetReservationDTO>(reservation);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found reservation with id '{id}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        /*
        public async Task<ServiceResponse<GetItemDTO>> DeleteItem(long reservationId, long itemId)
        {
            ServiceResponse<GetItemDTO> serviceResponse = new ServiceResponse<GetItemDTO>();
            try
            {
                Item item = await _context.Items.FirstOrDefaultAsync(x => x.Id == itemId);
                if (item != null)
                {
                    if (item.ReservationId == reservationId)
                    {
                        _context.Items.Remove(item);
                        await _context.SaveChangesAsync();
                        serviceResponse.Data = _mapper.Map<GetItemDTO>(item);
                    }
                    else
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = $"Item with id '{itemId}' is not part of reservation with id '{reservationId}'";
                    }
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found item with id '{itemId}'";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        */
    
        
        private async Task<List<GetReservationDTO>> GetReservations(long? id = null, long? sellerId = null, long? marketId = null, string userId = null,
            DateTime createdSince = new DateTime(), DateTime pickupSince = new DateTime(), DateTime pickup = new DateTime(), StatusEnum? status = null)
        {
            bool onlyNew = (status == StatusEnum.New);
            bool? accepted = null;
            bool? packed = null;

            if (status != null && !onlyNew)
            {
                if (status == StatusEnum.Packed)
                {
                    accepted = true;
                    packed = true;
                }
                else if (status == StatusEnum.Accepted)
                    accepted = true;
                else if (status == StatusEnum.Declined)
                    accepted = false;
            }

            var dbReservations = await _context.Reservations
                .Include(x => x.MarketSeller)
                .Include(x => x.MarketSeller.Market)
                .Include(x => x.MarketSeller.Seller)
                .Include(x => x.User)
                .Include(x => x.Items)
                .Where(x => (id.HasValue ? x.Id == id.Value : true)
                    && (sellerId.HasValue ? x.MarketSeller.SellerId == sellerId.Value : true)
                    && (marketId.HasValue ? x.MarketSeller.MarketId == marketId.Value : true)
                    && (!string.IsNullOrEmpty(userId) ? x.UserId == userId : true)
                    && x.CreatedAt >= createdSince
                    && x.Pickup >= pickupSince
                    && (pickup != new DateTime() ? x.Pickup.Date == pickup.Date : true)
                    && (accepted.HasValue ? x.Accepted == accepted.Value : (onlyNew ? x.Accepted == null : true))
                    && (packed.HasValue ? x.Packed == packed.Value : true))
                .ToListAsync();

            return dbReservations.Select(x => _mapper.Map<GetReservationDTO>(x)).ToList();
        }
        private Dictionary<string, string> CreatePlaceholders(Reservation reservation, Market market, Seller seller, User user)
        {
            var itemTable = string.Join("\r\n", reservation.Items.Select(x => $"\t-\t\t {x.Amount} {x.Unit} {x.Name}"));
            return new Dictionary<string, string>()
                {
                    { "{recipientName}", $"{reservation.User.Firstname} {reservation.User.Lastname}" },
                    { "{sellerName}", $"{seller.Name}" },
                    { "{marketName}", $"{market.Name}" },
                    { "{reservationId}", $"{(reservation.Id)}" },
                    { "{pickupTime}", $"{String.Format("{0:f}", reservation.Pickup.ToLocalTime())}" },
                    { "{pickupTimeShort}", $"{String.Format("{0:g}", reservation.Pickup.ToLocalTime())}" },
                    { "{sellerComment}", $"{reservation.SellerComment}" },
                    { "{price}", $"{(reservation.Price)}" },
                    { "{itemTable}", $"{itemTable}" }
                };
        }
    }
}