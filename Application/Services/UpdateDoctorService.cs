using Application.DTOs.Doctors;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UpdateDoctorService : IUpdateDoctorService
    {
        private readonly IDoctorCommand _doctorCommand;
        private readonly IDoctorQuery _doctorQuery;

        public UpdateDoctorService(IDoctorCommand doctorCommand, IDoctorQuery doctorQuery)
        {
            _doctorCommand = doctorCommand;
            _doctorQuery = doctorQuery;
        }

        public async Task<DoctorResponse> UpdateDoctorAsync(UpdateDoctorRequest request)
        {
            // Buscar el doctor existente
            var doctor = await _doctorQuery.GetByIdAsync(request.DoctorId);
            if (doctor == null)
            {
                throw new Exception("Doctor no encontrado");
            }

            // Actualizar solo los campos que no sean null
            if (!string.IsNullOrEmpty(request.Specialty))
                doctor.Specialty = request.Specialty;

            if (!string.IsNullOrEmpty(request.LicenseNumber))
                doctor.LicenseNumber = request.LicenseNumber;

            // Guardar cambios
            var updatedDoctor = await _doctorCommand.UpdateAsync(doctor);

            // Retornar respuesta
            return new DoctorResponse
            {
                DoctorId = (int)updatedDoctor.DoctorId,
                Specialty = updatedDoctor.Specialty,
                LicenseNumber = updatedDoctor.LicenseNumber,
                UserId = (int)updatedDoctor.UserId
            };
        }
    }
}
