using System.Collections.Generic;
using System.Threading.Tasks;
using CargoCaptain.Models;
using CargoCaptain.Enums;

namespace CargoCaptain.Interfaces
{
    public interface ICustomsService
    {
        Task<IEnumerable<CustomsDeclaration>> GetAllDeclarationsAsync();
        
        Task<IEnumerable<CustomsDeclaration>> GetPendingDeclarationsAsync();
        
        Task<IEnumerable<CustomsDeclaration>> GetCompletedDeclarationsAsync();
        
        Task<CustomsDeclaration?> GetDeclarationByIdAsync(int id);
        
        Task<CustomsDeclaration?> GetDeclarationByBookingIdAsync(int bookingId);
        
        Task FileDeclarationAsync(CustomsDeclaration declaration);
        
        Task UpdateDeclarationStatusAsync(int id, ClearanceStatus newStatus);
        
        Task<decimal> CalculateDutyAsync(string hsCode, decimal declaredValue);
        
        Task<bool> ValidateHSCodeAsync(string hsCode);
        
        Task<bool> VerifyBookingDocumentsExistAsync(int bookingId);
        
        Task TransitionToImportCustomsAsync(int bookingId);
    }
}
