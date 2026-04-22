using Global_Logistics_Managemant_System_POE.Data;
using Global_Logistics_Managemant_System_POE.Models;
using Global_Logistics_Managemant_System_POE.Patterns.Decorator;
using Global_Logistics_Managemant_System_POE.Patterns.Observer;
using Global_Logistics_Managemant_System_POE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Global_Logistics_Managemant_System_POE.Controllers
{
    
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrencyService _currencyService;
        public ServiceRequestsController(ApplicationDbContext context, CurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // GET: ServiceRequests - displays a list of all service requests 
        public async Task<IActionResult> Index()
        {
            try
            {
                var serviceRequests = await _context.ServiceRequests
                    .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                    .ToListAsync();

                return View(serviceRequests);
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = $"An error occurred while fetching service requests: {ex.Message}";

                return View("Error");
            }

        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                var serviceRequest = await _context.ServiceRequests
                    .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                    .FirstOrDefaultAsync(sr => sr.ServiceRequestId == id.Value);

                if (serviceRequest == null)
                {
                    return NotFound();
                }
                return View(serviceRequest);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving service request details: {ex.Message}";
                return View("Error");
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var activeContracts = await _context.Contracts
                    .Where(c => c.Status == "Active")
                    .Include(c => c.Client)
                    .ToListAsync();

                ViewBag.Contracts = new SelectList(
                    activeContracts, "ContractId", "Client.ClientName");

                ViewBag.Statuses = new SelectList(new List<string>
                {
                    "Pending", "In Progress", "Completed", "Cancelled"
                });

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the create form: {ex.Message}";
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ServiceRequest serviceRequest)
        {
            try
            {

                var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

                if (contract == null)
                {
                    ModelState.AddModelError("", "The selected contract does not exist.");
                }
                else
                {

                    var baseRequest = new ServiceRequestBase { Cost = serviceRequest.CostUSD };
                    var validatedRequest = new ValidationDecorator(baseRequest, contract);

                    try
                    {

                        validatedRequest.CalculateCost();
                    }
                    catch (InvalidOperationException ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                }

                if (ModelState.IsValid)
                {

                    serviceRequest.CostZAR = await _currencyService.ConvertUSDToZARAsync(serviceRequest.CostUSD);

                    serviceRequest.Status = "Pending";

                    _context.ServiceRequests.Add(serviceRequest);
                    await _context.SaveChangesAsync();


                    if (contract != null)
                    {
                        var emailNotifier = new EmailNotifier();
                        contract.Attach(emailNotifier);
                        contract.Notify();
                    }

                    TempData["SuccessMessage"] = "Service request created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the service request: {ex.Message}";
                return View("Error");
            }


            var activeContracts = await _context.Contracts
                .Where(c => c.Status == "Active")
                .Include(c => c.Client)
                .ToListAsync();

            ViewBag.Contracts = new SelectList(activeContracts, "ContractId", "Client.ClientName");
            ViewBag.Statuses = new SelectList(new List<string>
            {
                "Pending", "In Progress", "Completed", "Cancelled"
            });

            return View(serviceRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                var serviceResquest = await _context.ServiceRequests.FindAsync(id);
                if (serviceResquest == null) 
                {
                    return NotFound();
                }

                var activeContracts = await _context.Contracts
                    .Where(c => c.Status == "Active")
                    .Include(c => c.Client)
                    .ToListAsync();

                ViewBag.Contracts = new SelectList(
                    activeContracts, "ContractId", "Client.ClientName", serviceResquest.ContractId);

                ViewBag.Statuses = new SelectList(new List<string>
                    {
                    "Pending", "In Progress", "Completed", "Cancelled"
                }, serviceResquest.Status);

                return View(serviceResquest);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the edit form: {ex.Message}";
                return View("Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceRequestId)
            {
                return NotFound();
            }

            try
            {

                var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

                if (contract == null)
                {
                    ModelState.AddModelError("", "The selected contract does not exist.");
                }
                else
                {
                    var baseRequest = new ServiceRequestBase { Cost = serviceRequest.CostUSD };
                    var validatedRequest = new ValidationDecorator(baseRequest, contract);

                    try
                    {
                        validatedRequest.CalculateCost();
                    }
                    catch (InvalidOperationException ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                }

                if (ModelState.IsValid)
                {

                    serviceRequest.CostZAR = await _currencyService.ConvertUSDToZARAsync(serviceRequest.CostUSD);

                    _context.Update(serviceRequest);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Service request updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ServiceRequests.Any(sr => sr.ServiceRequestId == id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the service request: {ex.Message}";
                return View("Error");
            }

            var activeContracts = await _context.Contracts
                .Where(c => c.Status == "Active")
                .Include(c => c.Client)
                .ToListAsync();

            ViewBag.Contracts = new SelectList(activeContracts, "ContractId", "Client.ClientName");
            ViewBag.Statuses = new SelectList(new List<string>
            {
                "Pending", "In Progress", "Completed", "Cancelled"
            });

            return View(serviceRequest);
        }

        public async Task<IActionResult>Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                var serviceRequest = await _context.ServiceRequests
                    .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                   .FirstOrDefaultAsync(sr => sr.ServiceRequestId == id.Value);

                if (serviceRequest == null)
                {
                    return NotFound();
                }
                return View(serviceRequest);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"]= $"An error occurred while retrieving the service request for deletion: {ex.Message}";
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var serviceRequest = await _context.ServiceRequests.FindAsync(id);

                if (serviceRequest == null)
                {
                    return NotFound();
                }

                _context.ServiceRequests.Remove(serviceRequest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Service request deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the service request: {ex.Message}";
                return View("Error");
            }
        }
    }
}