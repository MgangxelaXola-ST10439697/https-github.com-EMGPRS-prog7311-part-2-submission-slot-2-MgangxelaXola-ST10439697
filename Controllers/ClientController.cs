using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Global_Logistics_Managemant_System_POE.Data;
using Global_Logistics_Managemant_System_POE.Models;

namespace Global_Logistics_Managemant_System_POE.Controllers
{
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            try
            {
                var clients = await _context.Clients
                    .Include(c => c.Contracts)
                    .ToListAsync();

                return View(clients);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving clients: {ex.Message}";
                return View("Error");
            }
        }

        // GET: Client/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Contracts)
                    .FirstOrDefaultAsync(c => c.ClientId == id);

                if (client == null) return NotFound();
                return View(client);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving client details: {ex.Message}";
                return View("Error");
            }
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(client);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Client created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while creating the client: {ex.Message}";
                    return View(client);
                }
            }
            return View(client);
        }

        // GET: Client/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            try
            {
                var client = await _context.Clients.FindAsync(id);
                if (client == null) return NotFound();
                return View(client);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving the client for editing: {ex.Message}";
                return View("Error");
            }
        }

        // POST: Client/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.ClientId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Client updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Clients.Any(c => c.ClientId == id))
                        return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the client: {ex.Message}";
                    return View(client);
                }
            }
            return View(client);
        }

        // GET: Client/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Contracts)
                    .FirstOrDefaultAsync(c => c.ClientId == id);

                if (client == null) return NotFound();
                return View(client);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving the client for deletion: {ex.Message}";
                return View("Error");
            }
        }

        // POST: Client/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var client = await _context.Clients.FindAsync(id);
                if (client == null) return NotFound();

                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Client deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the client: {ex.Message}";
                return View("Error");
            }
        }
    }
}