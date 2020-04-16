using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Markt2Go.Data;
using Markt2Go.Model;
using Markt2Go.DTOs.User;
using Markt2Go.Services.Auth0Service;

namespace Markt2Go.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IAuth0Service _auth0Service;

        public UserService(IMapper mapper, DataContext context, IAuth0Service auth0Service)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (auth0Service == null)
                throw new ArgumentNullException(nameof(auth0Service));

            _mapper = mapper;
            _context = context;
            _auth0Service = auth0Service;
        }

        public async Task<ServiceResponse<List<GetUserDTO>>> GetAllUsers()
        {
            ServiceResponse<List<GetUserDTO>> serviceResponse = new ServiceResponse<List<GetUserDTO>>();
            List<User> users = await _context.Users.ToListAsync();
            serviceResponse.Data = users.Select(c => _mapper.Map<GetUserDTO>(c)).ToList();
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetUserDTO>> GetUser(string id)
        {
            ServiceResponse<GetUserDTO> serviceResponse = new ServiceResponse<GetUserDTO>();
            User user = await _context.Users.FindAsync(id);
            serviceResponse.Data = _mapper.Map<GetUserDTO>(user);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetUserDTO>> AddUser(string userToken, AddUserDTO newUser)
        {
            ServiceResponse<GetUserDTO> serviceResponse = new ServiceResponse<GetUserDTO>();
            try
            {
                var userInformation = await _auth0Service.GetUserInfo(userToken);
                if (userInformation.ContainsKey("sub") && userInformation.ContainsKey("name") && userInformation.ContainsKey("email"))
                {
                    var userId = userInformation["sub"].ToString();
                    if (await _context.Users.FindAsync(userId) == null)
                    {
                        var user = _mapper.Map<User>(newUser);
                        user.Id = userId;
                        user.Name = userInformation["name"].ToString();
                        user.Mail = userInformation["email"].ToString();
                        user.CreatedAt = DateTime.Now.ToUniversalTime();

                        await _context.Users.AddAsync(user);
                        await _context.SaveChangesAsync();

                        serviceResponse.Data = _mapper.Map<GetUserDTO>(user);
                    }
                    else
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = $"User with id '{userId}' already exists.";
                    }
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not get all needed user information from auth0.";
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetUserDTO>> UpdateUser(UpdateUserDTO updatedUser)
        {
            ServiceResponse<GetUserDTO> serviceResponse = new ServiceResponse<GetUserDTO>();
            try
            {
                var user = await _context.Users.FindAsync(updatedUser.Id);
                if (user == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found user with id '{updatedUser.Id}'";
                }
                else
                {
                    user.Firstname = updatedUser.Firstname;
                    user.Lastname = updatedUser.Lastname;
                    user.Phone = updatedUser.Phone;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = _mapper.Map<GetUserDTO>(user);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }
        public async Task<ServiceResponse<GetUserDTO>> DeleteUser(string id)
        {
            ServiceResponse<GetUserDTO> serviceResponse = new ServiceResponse<GetUserDTO>();
            try
            {
                var user = await _context.Users.FindAsync(id);
                var reservations = _context.Reservations.Where(x => x.UserId == id);
                var hasOpenReservations = await reservations.AnyAsync(x => x.Pickup > DateTime.UtcNow);

                // check for open reservations
                if (hasOpenReservations)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"User with id '{id}' has still open reservations and could not be deleted.";
                    return serviceResponse;
                }

                // try to delete auth0 user
                if (await _auth0Service.DeleteUser(id))
                {
                    // delete user from database if present
                    if (user != null)
                    {
                        // delete "old" reservations and user
                        _context.Reservations.RemoveRange(reservations);
                        _context.Users.Remove(user);
                        await _context.SaveChangesAsync();
                        serviceResponse.Data = _mapper.Map<GetUserDTO>(user);
                    }
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not delete user with id '{id}' from auth0.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Exception = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse> UpdateUserSellerId(string userId, long sellerId)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    if (sellerId == 0)
                    {
                        user.SellerId = null;
                    }
                    else
                    {
                        user.SellerId = sellerId;
                    }

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found user with id '{userId}'";
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