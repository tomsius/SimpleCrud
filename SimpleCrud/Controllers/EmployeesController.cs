using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SimpleCrud.Data;
using SimpleCrud.Models;
using SimpleCrud.Models.Domain;
using System.Data;

namespace SimpleCrud.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        List<Employee> employees = await _context.Employees.ToListAsync();

        return View(employees);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddEmployeeViewModel employee) 
    {
        Employee newEmployee = new() 
        {
            Id = Guid.NewGuid(),
            Name = employee.Name,
            Email= employee.Email,
            Salary = employee.Salary
        };

        await _context.Employees.AddAsync(newEmployee);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Update(Guid id)
    {
        Employee? employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return RedirectToAction("Index");
        }

        UpdateEmployeeViewModel employeeToUpdate = new()
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Salary = employee.Salary
        };

        return View(employeeToUpdate);
    }

    [HttpPost]
    public async Task<IActionResult> Update(UpdateEmployeeViewModel updatedEmployee)
    {
        Employee? employee = await _context.Employees.FindAsync(updatedEmployee.Id);

        if (employee is not null)
        {
            employee.Name = updatedEmployee.Name;
            employee.Email = updatedEmployee.Email;
            employee.Salary = updatedEmployee.Salary;

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        Employee? employee = await _context.Employees.FindAsync(id);

        if (employee is not null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}
