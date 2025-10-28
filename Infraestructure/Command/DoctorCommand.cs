using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Command
{
    public class DoctorCommand : IDoctorCommand
    {
        private readonly AppDbContext _context;

        public DoctorCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Doctor> CreateAsync(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            await _context.Entry(doctor).Reference(d => d.UserNavigation).LoadAsync();
            return doctor;
        }

        //public Task ExecuteAsync()
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<Doctor> UpdateAsync(Doctor doctor)
        {
            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();
            return doctor;
        }
    }
}
