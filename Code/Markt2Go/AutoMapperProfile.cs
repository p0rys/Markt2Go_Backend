using System;
using System.Linq;
using AutoMapper;
using Markt2Go.DTOs.Item;
using Markt2Go.DTOs.Market;
using Markt2Go.DTOs.Reservation;
using Markt2Go.DTOs.Seller;
using Markt2Go.DTOs.Timeslot;
using Markt2Go.DTOs.User;
using Markt2Go.Model;
using Markt2Go.Shared.Extensions;

namespace Markt2Go
{
    public class LastReservationResolver : IMemberValueResolver<MarketSeller, GetMarketSellerDTO, Market, DateTime>
    {
        public DateTime Resolve(MarketSeller source, GetMarketSellerDTO destination, Market sourceMember, DateTime destMember, ResolutionContext context)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (sourceMember == null)
                throw new ArgumentNullException(nameof(sourceMember));
                
            DateTime next = DateTime.Now.GetNextDate((DayOfWeek)sourceMember.DayOfWeek);
            next = next.SetTime(sourceMember.StartTime, DateTimeKind.Local).ToUniversalTime();
            return next.AddHours(source.LastReservationOffset ?? 0);
        }
    }
   
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, GetUserDTO>();
            CreateMap<AddUserDTO, User>();

            CreateMap<Market, GetMarketDTO>();

            CreateMap<AddMarketDTO, Market>();

            CreateMap<Seller, GetSellerDTO>();
            CreateMap<AddSellerDTO, Seller>();

            CreateMap<MarketSeller, GetSellerMarketDTO>()                
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MarketId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Market.Name))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.Market.DayOfWeek))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Market.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Market.EndTime))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Market.Location));
            CreateMap<MarketSeller, GetMarketSellerDTO>()                
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SellerId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Seller.Name))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Seller.Category))
                .ForMember(dest => dest.LastReservation, opt => opt.MapFrom<LastReservationResolver, Market>(src => src.Market));               
            CreateMap<AddMarketSellerDTO, MarketSeller>();

            CreateMap<Reservation, GetReservationDTO>()
                .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketSeller.MarketId))
                .ForMember(dest => dest.Market, opt => opt.MapFrom(src => src.MarketSeller.Market))
                .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.MarketSeller.SellerId))
                .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.MarketSeller.Seller));
            CreateMap<AddReservationDTO, Reservation>();

            CreateMap<Item, GetItemDTO>();
            CreateMap<AddItemDTO, Item>();

            CreateMap<IGrouping<DateTime, Reservation>, GetTimeslotDTO>()
                .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.First().MarketSeller.MarketId))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.TimestampUTC, opt => opt.MapFrom(src => src.Key.ToUniversalTime()))
                .ForMember(dest => dest.ReservationCount, opt => opt.MapFrom(src => src.Count()));
        }
    }
}