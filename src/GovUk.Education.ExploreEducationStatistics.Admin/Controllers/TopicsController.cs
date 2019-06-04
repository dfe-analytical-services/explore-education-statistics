using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TopicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Topics
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Topics.Include(t => t.Theme).OrderBy(t => t.Title);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Topics/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics
                .Include(t => t.Theme)
                .Include(t => t.Publications).OrderBy(t => t.Title)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (topic == null)
            {
                return NotFound();
            }

            return View(topic);
        }

        // GET: Topics/Create
        public IActionResult Create()
        {
            ViewData["ThemeId"] = new SelectList(_context.Themes.OrderBy(t => t.Title), "Id", "Title");
            return View();
        }

        // POST: Topics/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Slug,Description,ThemeId,Summary")] Topic topic)
        {
            if (ModelState.IsValid)
            {
                topic.Id = Guid.NewGuid();
                _context.Add(topic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ThemeId"] = new SelectList(_context.Themes, "Id", "Title", topic.ThemeId);
            return View(topic);
        }

        // GET: Topics/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics.FindAsync(id);
            if (topic == null)
            {
                return NotFound();
            }
            ViewData["ThemeId"] = new SelectList(_context.Themes, "Id", "Title", topic.ThemeId);
            return View(topic);
        }

        // POST: Topics/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,Slug,Description,ThemeId,Summary")] Topic topic)
        {
            if (id != topic.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(topic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TopicExists(topic.Id))
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
            ViewData["ThemeId"] = new SelectList(_context.Themes, "Id", "Title", topic.ThemeId);
            return View(topic);
        }

        // GET: Topics/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics
                .Include(t => t.Theme)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (topic == null)
            {
                return NotFound();
            }

            return View(topic);
        }

        // POST: Topics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var topic = await _context.Topics.FindAsync(id);
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TopicExists(Guid id)
        {
            return _context.Topics.Any(e => e.Id == id);
        }
    }
}
