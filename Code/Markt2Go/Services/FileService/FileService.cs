using System;
using System.Collections.Generic;
using System.IO;

using OfficeOpenXml;

using Markt2Go.Model;

namespace Markt2Go.Services.FileService
{
    public class FileService : IFileService
    {
        public byte[] CreateReservationExcelFile(string marketName, DateTime date, List<Reservation> reservations)
        {       
            if (reservations == null)
                throw new ArgumentNullException(nameof(reservations));

            byte[] fileData = null;

            using (var p = new ExcelPackage())
            {
                ExcelWorksheet ws = p.Workbook.Worksheets.Add("Reservierungen");
                int row = 1;
                
                // columns sizes to match din a4 width
                ws.Column(1).Width = 10;
                ws.Column(2).Width = 15;
                ws.Column(3).Width = 20;
                ws.Column(4).Width = 19;
                ws.Column(5).Width = 15;
                ws.Column(6).Width = 7.5;

                row = ws.InsertHeader(row, marketName, date);                    
                foreach (var reservation in reservations)
                {
                    row = ws.InsertReservationHeader(row, reservation);
                    row = ws.InsertItemHeader(row);                            
                    foreach (var item in reservation.Items)                        
                        row = ws.InsertItem(row, item);    
                    
                    row++;
                }

                // use memory stream to decode file to byte[]
                using (var ms = new MemoryStream())
                {
                    p.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    fileData = ms.ToArray();
                }
            }

            return fileData;
        }
    }
}