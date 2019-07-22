using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    [ApiExplorerSettings(IgnoreApi=true)]
    [Authorize]
    public class ReleasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReleasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Releases
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Releases.Include(r => r.Publication).OrderBy(r => r.Title);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Releases/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var release = await _context.Releases
                .Include(r => r.Publication)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (release == null)
            {
                return NotFound();
            }

            return View(release);
        }

        // GET: Releases/Create
        public IActionResult Create()
        {
            ViewData["PublicationId"] = new SelectList(_context.Publications.OrderBy(p => p.Title), "Id", "Title");
            return View();
        }

        // POST: Releases/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseName,Published,Slug,Summary,PublicationId,Content,KeyStatistics")] Release release)
        {
            if (ModelState.IsValid)
            {
                release.Id = Guid.NewGuid();
                release.Order = NextOrder(release);
                _context.Add(release);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PublicationId"] = new SelectList(_context.Publications, "Id", "Title", release.PublicationId);
            return View(release);
        }

        // GET: Releases/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var release = await _context.Releases.FindAsync(id);
            if (release == null)
            {
                return NotFound();
            }
            ViewData["PublicationId"] = new SelectList(_context.Publications, "Id", "Title", release.PublicationId);
            return View(release);
        }

        // POST: Releases/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,ReleaseName,Published,Slug,Summary,PublicationId,Content,KeyStatistics")] Release release)
        {
            if (id != release.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(release);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReleaseExists(release.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PublicationId"] = new SelectList(_context.Publications, "Id", "Title", release.PublicationId);
            return View(release);
        }

        // GET: Releases/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var release = await _context.Releases
                .Include(r => r.Publication)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (release == null)
            {
                return NotFound();
            }

            return View(release);
        }

        // POST: Releases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var release = await _context.Releases.FindAsync(id);
            _context.Releases.Remove(release);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReleaseExists(Guid id)
        {
            return _context.Releases.Any(e => e.Id == id);
        }

        private int NextOrder(Release release)
        {
            var rel = _context.Releases.OrderByDescending(r => r.Order).FirstOrDefault(r => r.Publication.Id == release.PublicationId);
            return rel?.Order + 1 ?? 0;
        }
    }
}
