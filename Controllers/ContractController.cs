using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Global_Logistics_Managemant_System_POE.Data;
using Global_Logistics_Managemant_System_POE.Models;
using Global_Logistics_Managemant_System_POE.Patterns.Factory;
using Global_Logistics_Managemant_System_POE.Patterns.Observer;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Global_Logistics_Managemant_System_POE.Controllers
{
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ContractController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Contract
        public async Task<IActionResult> Index(string? status, DateTime? startdate, DateTime? enddate)
        {
            try
            {
                var contracts = _context.Contracts
                    .Include(c => c.Client)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                    contracts = contracts.Where(c => c.Status == status);

                if (startdate.HasValue)
                    contracts = contracts.Where(c => c.StartDate >= startdate.Value);

                if (enddate.HasValue)
                    contracts = contracts.Where(c => c.EndDate <= enddate.Value);

                ViewBag.CurrencyStatus = status;
                ViewBag.CurrencyStartDate = startdate?.ToString("yyyy-MM-dd");
                ViewBag.CurrencyEndDate = enddate?.ToString("yyyy-MM-dd");
                ViewBag.Statuses = new SelectList(new List<string> { "Draft", "Active", "Expired", "On Hold" });

                return View(await contracts.ToListAsync());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving contracts: {ex.Message}";
                return View("Error");
            }
        }

        // GET: Contract/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            try
            {
                var contract = await _context.Contracts
                    .Include(c => c.Client)
                    .Include(c => c.ServiceRequests)
                    .FirstOrDefaultAsync(c => c.ContractId == id);

                if (contract == null) return NotFound();
                return View(contract);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving the contract details: {ex.Message}";
                return View("Error");
            }
        }

        // GET: Contract/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Clients = new SelectList(
                    await _context.Clients.ToListAsync(), "ClientId", "ClientName");
                ViewBag.ContractTypes = new SelectList(new List<string> { "Standard", "SLA" });
                ViewBag.Status = new SelectList(new List<string> { "Draft", "Active", "Expired", "On Hold" });
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while preparing the contract creation form: {ex.Message}";
                return View("Error");
            }
        }

        // POST: Contract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, string contractType, IFormFile? signedAgreement)
        {
            // Validate PDF upload if provided
            if (signedAgreement != null)
            {
                if (signedAgreement.ContentType != "application/pdf")
                    ModelState.AddModelError("", "Only PDF files are allowed.");

                if (signedAgreement.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError("", "File size must be less than 5MB.");
            }

            // Validate contract type separately — don't add to ModelState so it shows clearly
            if (string.IsNullOrWhiteSpace(contractType))
            {
                ModelState.AddModelError("contractType", "Please select a contract type.");
                ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "ClientId", "ClientName");
                return View(contract);
            }

            // Remove model-level validation errors for fields we set manually after factory creation
            ModelState.Remove("Status");
            ModelState.Remove("ServiceLevel");
            ModelState.Remove("SignedAgreementPath");

            if (ModelState.IsValid)
            {
                try
                {
                    // Use factory to build the correct contract type
                    var newContract = ContractFactory.CreateContract(contractType);

                    newContract.ClientId = contract.ClientId;
                    newContract.StartDate = contract.StartDate;
                    newContract.EndDate = contract.EndDate;
                    newContract.Status = contract.Status;
                    newContract.ServiceLevel = contract.ServiceLevel;
                    newContract.SignedAgreementPath = string.Empty;

                    // Handle PDF upload
                    if (signedAgreement != null && signedAgreement.Length > 0)
                    {
                        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
                        Directory.CreateDirectory(uploadFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + signedAgreement.FileName;
                        var filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await signedAgreement.CopyToAsync(stream);
                        }

                        newContract.SignedAgreementPath = "/uploads/" + uniqueFileName;
                    }

                    // Notify observers
                    var emailNotifier = new EmailNotifier();
                    newContract.Attach(emailNotifier);
                    newContract.Notify();

                    _context.Contracts.Add(newContract);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Contract created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while creating the contract: {ex.Message}";
                    return View("Error");
                }
            }

            ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "ClientId", "ClientName");
            return View(contract);
        }

        // GET: Contract/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            try
            {
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null) return NotFound();

                ViewBag.Clients = new SelectList(
                    await _context.Clients.ToListAsync(), "ClientId", "ClientName");
                ViewBag.Statuses = new SelectList(new List<string> { "Draft", "Active", "Expired", "On Hold" });

                return View(contract);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving the contract for editing: {ex.Message}";
                return View("Error");
            }
        }

        // POST: Contract/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? signedAgreement)
        {
            if (id != contract.ContractId) return NotFound();

            if (signedAgreement != null)
            {
                if (signedAgreement.ContentType != "application/pdf")
                    ModelState.AddModelError("", "Only PDF files are allowed for the signed agreement.");

                if (signedAgreement.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError("", "File size cannot exceed 5MB.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the EXISTING record from DB so we keep the old PDF path if no new file uploaded
                    var existing = await _context.Contracts.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.ContractId == id);

                    if (existing == null) return NotFound();

                    if (signedAgreement != null && signedAgreement.Length > 0)
                    {
                        // Delete old file if it exists
                        if (!string.IsNullOrEmpty(existing.SignedAgreementPath))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath,
                                existing.SignedAgreementPath.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);
                        }

                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + signedAgreement.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await signedAgreement.CopyToAsync(stream);
                        }

                        contract.SignedAgreementPath = "/uploads/" + uniqueFileName;
                    }
                    else
                    {
                        // No new file — keep existing path
                        contract.SignedAgreementPath = existing.SignedAgreementPath;
                    }

                    // Notify observers
                    var emailNotifier = new EmailNotifier();
                    contract.Attach(emailNotifier);
                    contract.Notify();

                    _context.Update(contract);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Contract updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Contracts.Any(c => c.ContractId == id))
                        return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Failed to update contract: {ex.Message}");
                }
            }

            ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "ClientId", "ClientName");
            ViewBag.Statuses = new SelectList(new List<string> { "Draft", "Active", "Expired", "On Hold" });
            return View(contract);
        }

        // GET: Contract/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            try
            {
                var contract = await _context.Contracts
                    .Include(c => c.Client)
                    .FirstOrDefaultAsync(c => c.ContractId == id);

                if (contract == null) return NotFound();
                return View(contract);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while retrieving the contract for deletion: {ex.Message}";
                return View("Error");
            }
        }

        // POST: Contract/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null) return NotFound();

                // Remove PDF from disk
                if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                {
                    var filePath = Path.Combine(_env.WebRootPath,
                        contract.SignedAgreementPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Contract deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the contract: {ex.Message}";
                return View("Error");
            }
        }

        // GET: Contract/Download
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null) return NotFound();
            try
            {
                var contract = await _context.Contracts.FindAsync(id);

                if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                {
                    TempData["ErrorMessage"] = "No signed agreement found for this contract.";
                    return RedirectToAction(nameof(Index));
                }

                var filePath = Path.Combine(_env.WebRootPath,
                    contract.SignedAgreementPath.TrimStart('/'));

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] = "The signed agreement file could not be found on the server.";
                    return RedirectToAction(nameof(Index));
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while downloading the file: {ex.Message}";
                return View("Error");
            }
        }
    }
}