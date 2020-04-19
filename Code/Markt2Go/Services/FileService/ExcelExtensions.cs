using System;
using System.Globalization;
using Markt2Go.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Markt2Go.Services.FileService
{
    public static class ExcelExtensions
    {
        public static int InsertHeader(this ExcelWorksheet worksheet, int row, string marketName, DateTime marketDate)
        {
            worksheet.Cells[row, 1, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[row, 1, row, 6].Merge = true;

            worksheet.SetValue(row, 1, $"{marketName} - {marketDate.ToString("D", CultureInfo.GetCultureInfo("de-DE"))}" );            
            row += 2;

            return row;
        }
        public static int InsertReservationHeader(this ExcelWorksheet worksheet, int row, Reservation reservation)
        {
            // background color for complete header
            worksheet.Cells[row, 1, row + 1, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[row, 1, row + 1, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            // border for complete header
            worksheet.Cells[row, 1, row + 1, 6].Style.Border.BorderAround(ExcelBorderStyle.Medium);


            // first row
            worksheet.Cells[row, 1, row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 5, row, 5].Style.Font.Bold = true;
            worksheet.Cells[row, 2, row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[row, 5, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            worksheet.SetValue(row, 1, "Nummer:");
            worksheet.SetValue(row, 2, reservation.Id);
            worksheet.SetValue(row, 5, "Uhrzeit:");
            worksheet.SetValue(row, 6, reservation.Pickup.ToLocalTime().ToString("t", CultureInfo.GetCultureInfo("de-DE")));
            row++;

            // second row
            worksheet.Cells[row, 1, row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 2, row, 6].Merge = true;

            worksheet.SetValue(row, 1, "Besteller:");
            worksheet.SetValue(row, 2, $"{reservation.Firstname} {reservation.Lastname} ({reservation.Phone})");      
            row++;
            
            // return next row
            return row;
        }  
        public static int InsertItemHeader(this ExcelWorksheet worksheet, int row)
        {
            worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;
            worksheet.Cells[row, 1, row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 2, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 5, row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 6, row, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.SetValue(row, 1, "Artikel-Nr.");
            worksheet.SetValue(row, 2, "Artikelname");
            worksheet.SetValue(row, 5, "Menge");
            worksheet.SetValue(row, 6, "Einheit");
            row++;

            return row;
        }
        public static int InsertItem(this ExcelWorksheet worksheet, int row, Item item)
        {
            worksheet.Cells[row, 1, row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 2, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 5, row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 6, row, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 2, row, 4].Merge = true;

            worksheet.SetValue(row, 1, item.ArticleId ?? "n.v.");
            worksheet.SetValue(row, 2, item.Name);
            worksheet.SetValue(row, 5, item.Amount.ToString("F"));
            worksheet.SetValue(row, 6, item.Unit);
            row++;

            return row;
        }
    }
}