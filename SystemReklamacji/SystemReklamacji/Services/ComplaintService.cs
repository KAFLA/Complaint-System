using ReklamacjeSystem.Models;
using ReklamacjeSystem.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ReklamacjeSystem.Services
{
    // Usługa zarządzania reklamacjami
    public class ComplaintService
    {
        private readonly ComplaintRepository _complaintRepository; // Repozytorium reklamacji
        private readonly UserRepository _userRepository; // Repozytorium użytkowników (potrzebne do przypisywania)

        // Konstruktor przyjmujący instancje repozytoriów
        public ComplaintService(ComplaintRepository complaintRepository, UserRepository userRepository)
        {
            _complaintRepository = complaintRepository;
            _userRepository = userRepository;
        }

        // Pobiera wszystkie reklamacje
        public async Task<IEnumerable<Complaint>> GetAllComplaints()
        {
            return await _complaintRepository.GetAllAsync();
        }

        // Pobiera reklamację po ID
        public async Task<Complaint> GetComplaintById(int id)
        {
            return await _complaintRepository.GetByIdAsync(id);
        }

        // Tworzy nową reklamację
        public async Task AddComplaint(Complaint complaint)
        {
            if (string.IsNullOrWhiteSpace(complaint.Title))
            {
                throw new ArgumentException("Tytuł reklamacji nie może być pusty.");
            }
            complaint.CreatedAt = DateTime.Now;
            complaint.Status = ComplaintStatus.New; // Nowa reklamacja zawsze zaczyna się od statusu 'New'
            await _complaintRepository.AddAsync(complaint);
        }

        // Aktualizuje istniejącą reklamację
        public async Task UpdateComplaint(Complaint complaint)
        {
            if (string.IsNullOrWhiteSpace(complaint.Title))
            {
                throw new ArgumentException("Tytuł reklamacji nie może być pusty.");
            }
            await _complaintRepository.UpdateAsync(complaint);
        }

        // Usuwa reklamację
        public async Task DeleteComplaint(int id, UserRole currentUserRole)
        {
            // Przykład: Tylko Admin może usuwać reklamacje
            if (currentUserRole != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("Tylko administratorzy mogą usuwać reklamacje.");
            }
            await _complaintRepository.DeleteAsync(id);
        }

        // Zmienia status reklamacji
        public async Task ChangeComplaintStatus(int complaintId, ComplaintStatus newStatus, UserRole currentUserRole)
        {
            var complaint = await _complaintRepository.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                throw new KeyNotFoundException("Reklamacja nie znaleziona.");
            }

            // Przykładowa logika przejścia statusów:
            // Nowa -> W toku -> Rozwiązana -> Zamknięta
            // Admin może zawsze zmienić status
            if (currentUserRole != UserRole.Admin)
            {
                if (complaint.Status == ComplaintStatus.New && newStatus != ComplaintStatus.InProgress)
                {
                    throw new InvalidOperationException("Nową reklamację można zmienić tylko na 'W toku'.");
                }
                if (complaint.Status == ComplaintStatus.InProgress && (newStatus != ComplaintStatus.Resolved && newStatus != ComplaintStatus.New))
                {
                    throw new InvalidOperationException("Reklamację w toku można zmienić na 'Rozwiązaną' lub 'Nową'.");
                }
                if (complaint.Status == ComplaintStatus.Resolved && newStatus != ComplaintStatus.Closed)
                {
                    throw new InvalidOperationException("Rozwiązaną reklamację można tylko zamknąć.");
                }
                if (complaint.Status == ComplaintStatus.Closed && newStatus != ComplaintStatus.New)
                {
                    throw new InvalidOperationException("Zamkniętej reklamacji nie można edytować, chyba że zmienisz jej status na 'Nowa' (np. ponowne otwarcie).");
                }
            }


            complaint.Status = newStatus;
            await _complaintRepository.UpdateAsync(complaint);
            // Tutaj można wywołać NotificationService
        }

        // Przypisuje reklamację do użytkownika
        public async Task AssignComplaintToUser(int complaintId, int? userId) // userId może być null, aby cofnąć przypisanie
        {
            var complaint = await _complaintRepository.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                throw new KeyNotFoundException("Reklamacja nie znaleziona.");
            }

            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    throw new KeyNotFoundException("Użytkownik do przypisania nie znaleziony.");
                }
            }

            complaint.UserId = userId;
            await _complaintRepository.UpdateAsync(complaint);
        }
    }
}
