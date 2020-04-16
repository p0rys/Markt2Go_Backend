using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

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
using Markt2Go.Services.FileService;
using Markt2Go.Services.Auth0Service;

namespace Markt2Go.Services.ReservationService
{
    public class ReservationService : IReservationService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IMailService _mailService;
        private readonly IFileService _fileService;
        private readonly IAuth0Service _auth0Service;

        public ReservationService(IMapper mapper, DataContext context, IMailService mailService, IFileService fileService, IAuth0Service auth0Service)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (mailService == null)
                throw new ArgumentNullException(nameof(mailService));
            if (fileService == null)
                throw new ArgumentNullException(nameof(fileService));
            if (auth0Service == null)
                throw new ArgumentNullException(nameof(auth0Service));

            _mapper = mapper;
            _context = context;
            _mailService = mailService;
            _fileService = fileService;
            _auth0Service = auth0Service;
        }


        public async Task<ServiceResponse<List<GetReservationDTO>>> GetAllReservations()
        {
            ServiceResponse<List<GetReservationDTO>> serviceResponse = new ServiceResponse<List<GetReservationDTO>>();
            var reservations = await GetReservations();
            serviceResponse.Data = reservations.Select(x => _mapper.Map<GetReservationDTO>(x)).ToList();
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
            var reservations = await GetReservations(userId: userId, marketId: marketId, createdSince: createdSince, pickupSince: pickupSince, pickup: pickup, status: status);
            serviceResponse.Data = reservations.Select(x => _mapper.Map<GetReservationDTO>(x)).ToList(); 
            return serviceResponse;
        }
        public async Task<ServiceResponse<List<GetReservationDTO>>> GetReservationsBySeller(long sellerId, long? marketId, DateTime createdSince, DateTime pickupSince, DateTime pickup, StatusEnum? status)
        {
            ServiceResponse<List<GetReservationDTO>> serviceResponse = new ServiceResponse<List<GetReservationDTO>>();
            var reservations = await GetReservations(sellerId: sellerId, marketId: marketId, createdSince: createdSince, pickupSince: pickupSince, pickup: pickup, status: status);
            serviceResponse.Data = reservations.Select(x => _mapper.Map<GetReservationDTO>(x)).ToList(); 
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
        public async Task<ServiceResponse<byte[]>> GetReservationsAsExcelFile(long marketId, long sellerId, DateTime pickup, StatusEnum? status)
        {
            ServiceResponse<byte[]> serviceResponse = new ServiceResponse<byte[]>();
            try
            {
                var market = await _context.Markets.FindAsync(marketId);
                var reservations = await GetReservations(marketId: marketId, sellerId: sellerId, pickup: pickup, status: status);
                var orderedReservations = reservations.OrderBy(x => x.Pickup).ToList();
                if (market != null)
                {
                    serviceResponse.Data = _fileService.CreateReservationExcelFile(market.Name, pickup, orderedReservations);                    
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found market with id '{marketId}'.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }


        public async Task<ServiceResponse<GetReservationDTO>> AddReservation(string userToken, AddReservationDTO newReservation)
        {
            ServiceResponse<GetReservationDTO> serviceResponse = new ServiceResponse<GetReservationDTO>();
            try
            {               
                var user = await _context.Users.FindAsync(newReservation.UserId); 
                if (user == null)
                {                    
                    var userInformation = await _auth0Service.GetUserInfo(userToken);

                    // check if user is verified
                    if (userInformation.ContainsKey("email_verified") && !Convert.ToBoolean(userInformation["email_verified"].ToString()))
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = $"User email is not verified.";
                        return serviceResponse;
                    }

                    // check if data from auth0 are valid 
                    if (!userInformation.ContainsKey("sub") || !userInformation.ContainsKey("name") || !userInformation.ContainsKey("email"))
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = $"Could not get all needed user information from auth0.";
                        return serviceResponse;
                    }

                    user = new User();
                    user.Id = userInformation["sub"].ToString();
                    user.Name = userInformation["name"].ToString();
                    user.Mail = userInformation["email"].ToString();
                    user.CreatedAt = DateTime.Now.ToUniversalTime();

                    // user is added to context, but NOT saved to db at this point
                    await _context.Users.AddAsync(user);
                }

                var marketSeller = await _context.MarketSellers.Include(x => x.Market).Include(x => x.Seller).FirstOrDefaultAsync(x => x.SellerId == newReservation.SellerId && x.MarketId == newReservation.MarketId);
                if (marketSeller != null)
                {
                    // get next market day
                    var next = DateTime.Now.GetNextDate((DayOfWeek)marketSeller.Market.DayOfWeek);
                    // get start and end time on the next market day
                    var nextStart = next.SetTime(marketSeller.Market.StartTime, DateTimeKind.Local);
                    var nextEnd = next.SetTime(marketSeller.Market.EndTime, DateTimeKind.Local);
                    // get the time until the seller will accepted reservations
                    var lastReservation = nextStart.AddHours(marketSeller.LastReservationOffset ?? 0);
                    // check if seller is still accepting reservations
                    if (DateTime.Now <= lastReservation)
                    {                        
                        if (newReservation.Pickup >= nextStart.ToUniversalTime() && newReservation.Pickup <= nextEnd.ToUniversalTime())
                        {
                            Reservation reservation = _mapper.Map<Reservation>(newReservation);
                            reservation.MarketSellerId = marketSeller.Id;
                            reservation.CreatedAt = DateTime.Now.ToUniversalTime();

                            // add information to user obj if user would be rembered
                            if (newReservation.RememberMe)
                            {
                                user.Firstname = newReservation.Firstname;
                                user.Lastname = newReservation.Lastname;
                                user.Phone = newReservation.Phone;
                            }

                            // add and save reservation (this also saves the user created above)
                            await _context.Reservations.AddAsync(reservation);
                            await _context.SaveChangesAsync();

                            // load references to return a complete reservation object
                            _context.Entry(reservation).Reference(x => x.MarketSeller).Load();
                            _context.Entry(reservation.MarketSeller).Reference(x => x.Market).Load();
                            _context.Entry(reservation.MarketSeller).Reference(x => x.Seller).Load();

                            // send user mail
                            await _mailService.SendReservationConfirmation(user.Mail, CreatePlaceholders(reservation, marketSeller.Market, marketSeller.Seller));
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

                    var placeholders = CreatePlaceholders(reservation, reservation.MarketSeller.Market, reservation.MarketSeller.Seller);
                    // send user mail(s)
                    // TODO: refactor to only have one SendMail Method
                    if (acceptedChanged)
                    {
                        if (reservation.Accepted.Value)
                            await _mailService.SendReservationAccepted(reservation.User.Mail, placeholders);
                        else
                            await _mailService.SendReservationDeclined(reservation.User.Mail, placeholders);
                    }
                    //if (reservation.Packed)
                    //    await _mailService.SendReservationPacked(reservation.User.Mail, placeholders);

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
    
        
        private async Task<List<Reservation>> GetReservations(long? id = null, long? sellerId = null, long? marketId = null, string userId = null,
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

            return dbReservations.ToList();
        }
        private Dictionary<string, string> CreatePlaceholders(Reservation reservation, Market market, Seller seller)
        {
            var itemTable = string.Join("\r\n", reservation.Items.Select(x => $"\t-\t\t {x.Amount.ToString(CultureInfo.GetCultureInfo("de-DE"))} {x.Unit} {x.Name}"));
            return new Dictionary<string, string>()
                {
                    { "{recipientName}", $"{reservation.Firstname} {reservation.Lastname}" },
                    { "{sellerName}", $"{seller.Name}" },
                    { "{marketName}", $"{market.Name}" },
                    { "{reservationId}", $"{(reservation.Id)}" },
                    { "{pickupTime}", $"{reservation.Pickup.ToLocalTime().ToString("f", CultureInfo.GetCultureInfo("de-DE"))} Uhr" },
                    { "{pickupTimeShort}", $"{reservation.Pickup.ToLocalTime().ToString("g", CultureInfo.GetCultureInfo("de-DE"))} Uhr" },
                    { "{sellerComment}", $"{reservation.SellerComment}" },
                    { "{price}", $"{(reservation.Price)}" },
                    { "{itemTable}", $"{itemTable}" }
                };
        }
 
    }
}