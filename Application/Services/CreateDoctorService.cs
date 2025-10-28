using Application.DTOs.Doctors;
using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CreateDoctorService : ICreateDoctorService
    {
        private readonly IDoctorCommand _doctorCommand;
        private readonly IDoctorQuery _doctorQuery;
        private readonly IUserQuery _userQuery;

        public CreateDoctorService(
            IDoctorCommand doctorCommand,
            IDoctorQuery doctorQuery,
            IUserQuery userQuery)
        {
            _doctorCommand = doctorCommand;
            _doctorQuery = doctorQuery;
            _userQuery = userQuery;
        }

        public async Task<DoctorResponse> CreateDoctorAsync(CreateDoctorRequest request)
        {
            // Crear la entidad Doctor
            var doctor = new Doctor
            {
                Specialty = request.Specialty,
                LicenseNumber = request.LicenseNumber,
                UserId = request.UserId
            };

            // Guardar en la base de datos
            doctor = await _doctorCommand.CreateAsync(doctor);

            // Retornar la respuesta
            return new DoctorResponse
            {
                DoctorId = (int)doctor.DoctorId,
                Specialty = doctor.Specialty,
                LicenseNumber = doctor.LicenseNumber,
                UserId = (int)doctor.UserId
            };
        }
    }
}
