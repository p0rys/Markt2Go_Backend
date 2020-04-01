using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Markt2Go.Data;
using Markt2Go.Model;
using Markt2Go.DTOs.User;

namespace Markt2Go.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public UserService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
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

        public async Task<ServiceResponse<GetUserDTO>> AddUser(string id, string name, string mail, AddUserDTO newUser)
        {
            ServiceResponse<GetUserDTO> serviceResponse = new ServiceResponse<GetUserDTO>();
            if (await _context.Users.FindAsync(id) == null)
            {
                var user = _mapper.Map<User>(newUser);
                user.Id = id;
                user.Name = name;
                user.Mail = mail;
                user.CreatedAt = DateTime.Now.ToUniversalTime();

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetUserDTO>(user);
            }
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"User with id '{id}' already exists.";
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
                else if (user.IsValidated == true)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"User with id '{updatedUser.Id}' is already validated and can't be updated";
                }
                else
                {
                    user.Firstname = updatedUser.Firstname;
                    user.Lastname = updatedUser.Lastname;
                    user.Address = updatedUser.Address;
                    user.Zip = updatedUser.Zip;
                    user.Town = updatedUser.Town;
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
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = _mapper.Map<GetUserDTO>(user);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Could not found user with id '{id}'";
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