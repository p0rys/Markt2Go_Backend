using System;
using System.Collections.Generic;

using Markt2Go.Model;

namespace Markt2Go.Services.FileService
{
    public interface IFileService
    {
        byte[] CreateReservationExcelFile(string marketName, DateTime date, List<Reservation> reservations);
    }
}