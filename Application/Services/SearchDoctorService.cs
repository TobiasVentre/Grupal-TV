using Application.DTOs.Doctors;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SearchDoctorService : ISearchDoctorService
    {
        private readonly IDoctorQuery _doctorQuery;

        public SearchDoctorService(IDoctorQuery doctorQuery)
        {
            _doctorQuery = doctorQuery;
        }

        public async Task<DoctorResponse?> GetByIdAsync(long id)
        {
            var doctor = await _doctorQuery.GetByIdAsync(id);

            if (doctor == null)
                return null;

            return new DoctorResponse
            {
                DoctorId = (int)doctor.DoctorId,
                Specialty = doctor.Specialty,
                LicenseNumber = doctor.LicenseNumber,
                UserId = (int)doctor.UserId
            };
        }

        public async Task<List<DoctorResponse>> GetAllAsync()
        {
            var doctors = await _doctorQuery.GetAllAsync();

            return doctors.Select(d => new DoctorResponse
            {
                DoctorId = (int)d.DoctorId,
                Specialty = d.Specialty,
                LicenseNumber = d.LicenseNumber,
                UserId = (int)d.UserId
            }).ToList();
        }
    }
}
